using System.Runtime.CompilerServices;

namespace BladeEngine.ECS;

public sealed class EcsContext
{
	private InternalEntity[] _entities;
	private readonly List<ArchetypeBuffer> _buffers;
	private readonly Dictionary<Type, ISystem> _systems;
	private readonly Dictionary<int, Archetype> _archetypes;

	private int[] _freeEntities;
	private int _freeEntityCount;
	
	private Bitfield _bitfield = new();

	public EcsContext(int initialCapacity = 128)
	{
		_freeEntities = new int[initialCapacity];
		_buffers = new List<ArchetypeBuffer>();
		_systems = new Dictionary<Type, ISystem>();
		_entities = new InternalEntity[initialCapacity];
		_archetypes = new Dictionary<int, Archetype>();
		for (var i = 0; i < initialCapacity; i++) _entities[i] = new InternalEntity{Version = 1};
		for (var i = initialCapacity - 1; i >= 0; i--) _freeEntities[_freeEntityCount++] = i;
	}

	public int ArchetypeCount => _archetypes.Count;

	public Entity CreateEntity()
	{
		if (_freeEntityCount > 0)
		{
			var id = _freeEntities[--_freeEntityCount];
			return new Entity {Id = id, Version = ++_entities[id].Version};
		}
		
		DoubleEntityBuffer();
		var id0 = _freeEntities[--_freeEntityCount];
		return new Entity {Id = id0, Version = ++_entities[id0].Version};
	}

	[MethodImpl(MethodImplOptions.AggressiveOptimization)]
	public Entity CreateEntity(Archetype archetype)
	{
		var buffer = _buffers[archetype.Id];
		var slot = buffer.GetSlot();

		Entity entity;
		if (_freeEntityCount > 0)
		{
			var id = _freeEntities[--_freeEntityCount];
			entity = new  Entity {Id = id, Version = ++_entities[id].Version};
		}
		else
		{
			DoubleEntityBuffer();
			var id0 = _freeEntities[--_freeEntityCount];
			entity = new Entity {Id = id0, Version = ++_entities[id0].Version};
		}
		
		_entities[entity.Id].Slot = slot;
		return entity;
	}

	public void DestroyEntity(Entity entity)
	{
		if (_entities[entity.Id].Version != entity.Version)
			throw new InvalidOperationException("Entity already destroyed.");
		
		if (_freeEntities.Length == _freeEntityCount)
			Array.Resize(ref _freeEntities, _freeEntityCount * 2);
		
		_freeEntities[_freeEntityCount++] = entity.Id;
		var e = _entities[entity.Id];
		e.Slot?.Chunk.ReturnSlot(e.Slot);
		e.Version++; e.Slot = null;
	}
	
	public void DestroyEntity(Span<Entity> entities)
	{
		if (_freeEntities.Length < _freeEntityCount + entities.Length)
			Array.Resize(ref _freeEntities, _freeEntityCount + entities.Length);
		
		for (var i = 0; i < entities.Length; i++)
		{
			var entity = entities[i];
			if (_entities[entity.Id].Version != entity.Version)
				throw new InvalidOperationException("Entity already destroyed.");
			
			_freeEntities[_freeEntityCount++] = entity.Id;
			var e = _entities[entity.Id];
			e.Slot?.Chunk.ReturnSlot(e.Slot);
			e.Version++; e.Slot = null;
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
		var e = _entities[entity.Id];
		if (e.Slot is null) throw new InvalidOperationException("Component is not attached to the entity.");
		return ref e.Slot.Chunk.GetComponentRefUnsafe<T>(e.Slot.Slot);
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

	private void DoubleEntityBuffer()
	{
		var size = _entities.Length;
		var newSize = _entities.Length * 2;
		Array.Resize(ref _entities, newSize);
		for (var i = size; i < newSize; i++) _entities[i] = new InternalEntity{Version = 1};

		if (_freeEntities.Length - _freeEntityCount < size)
			Array.Resize(ref _freeEntities, _freeEntities.Length + size - _freeEntityCount);

		for (var i = newSize - 1; i >= size; i--) _freeEntities[_freeEntityCount++] = i;
	}
}