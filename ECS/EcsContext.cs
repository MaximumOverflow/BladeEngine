using System.Runtime.CompilerServices;

namespace BladeEngine.ECS;

public sealed class EcsContext
{
	private readonly List<ArchetypeBuffer> _buffers;
	private readonly Dictionary<Type, ISystem> _systems;
	private readonly Dictionary<int, Archetype> _archetypes;

	private int _freeEntityCount;
	private EntityInstance[] _freeEntities;

	private Bitfield _bitfield = new();

	public EcsContext(int initialCapacity = 128)
	{
		_buffers = new List<ArchetypeBuffer>();
		_systems = new Dictionary<Type, ISystem>();
		_archetypes = new Dictionary<int, Archetype>();
		_freeEntities = new EntityInstance[initialCapacity];
	}

	public int ArchetypeCount => _archetypes.Count;

	public Entity CreateEntity()
	{
		if (_freeEntityCount == 0)
			return new Entity {Instance = new EntityInstance {Version = 1}, Version = 1};
		
		var instance = _freeEntities[--_freeEntityCount];
		return new Entity {Instance = instance, Version = ++instance.Version};
	}

	[MethodImpl(MethodImplOptions.AggressiveOptimization)]
	public Entity CreateEntity(Archetype archetype)
	{
		var buffer = _buffers[archetype.Id];
		var slot = buffer.GetSlot();

		if (_freeEntityCount == 0)
			return new Entity {Instance = new EntityInstance {Version = 1, Slot = slot}, Version = 1};
		
		var instance = _freeEntities[--_freeEntityCount];

		instance.Slot = slot;
		return new Entity {Instance = instance, Version = ++instance.Version};
	}

	public void DestroyEntity(Entity entity)
	{
		var instance = entity.Instance;
		if (entity.Version != instance.Version)
			throw new InvalidOperationException("Entity already destroyed.");
		
		if (_freeEntities.Length == _freeEntityCount)
			Array.Resize(ref _freeEntities, _freeEntityCount * 2);
		
		_freeEntities[_freeEntityCount++] = instance;
		instance.Slot?.Chunk.ReturnSlot(instance.Slot);
		instance.Version++; instance.Slot = null;
	}
	
	public void DestroyEntity(Span<Entity> entities)
	{
		if (_freeEntities.Length < _freeEntityCount + entities.Length)
			Array.Resize(ref _freeEntities, _freeEntityCount + entities.Length);
		
		for (var i = 0; i < entities.Length; i++)
		{
			var entity = entities[i];
			var instance = entity.Instance;
			if (entity.Version != instance.Version)
				throw new InvalidOperationException("Entity already destroyed.");
			
			_freeEntities[_freeEntityCount++] = instance;
			instance.Slot?.Chunk.ReturnSlot(instance.Slot);
			instance.Version++; instance.Slot = null;
		}
	}
	
	public Archetype CreateArchetype(ComponentType[] types)
	{
		if (_bitfield.Capacity < ComponentType.Count)
			_bitfield = new Bitfield(ComponentType.Count);

		_bitfield.Clear();
		foreach (var t in types) _bitfield[1 << t.Id] = true;

		var key = _bitfield.GetHashCode();
		if (_archetypes.TryGetValue(key, out var archetype)) return archetype;

		var set = new HashSet<ComponentType>(types);
		archetype = new Archetype
		{
			Id = _archetypes.Count, 
			ComponentTypes = set,
		};
		_archetypes.Add(key, archetype);
		_buffers.Add(new ArchetypeBuffer(archetype));
		return archetype;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ref T GetComponentRef<T>(Entity entity) where T : unmanaged, IComponent
	{
		var instance = entity.Instance;
		if (instance.Slot is null) throw new InvalidOperationException("Component is not attached to the entity.");
		return ref instance.Slot.Chunk.GetComponentRefUnsafe<T>(instance.Slot.Slot);
	}

	public bool RegisterSystem(ISystem system)
	{
		return _systems.TryAdd(system.GetType(), system);
	}

	public void RunSystems()
	{
		foreach (var system in _systems.Values)
		{
			var archetype = system.Archetype;
			foreach (var buffer in _buffers)
			{
				if(!buffer.Archetype.ComponentTypes.IsSubsetOf(archetype.ComponentTypes)) continue;
				system.Run(buffer);
			}
		}
	}
}