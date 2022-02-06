using FastExpressionCompiler.LightExpression;
using System.Runtime.CompilerServices;
using System.Diagnostics;

namespace BladeEngine.ECS;

public sealed class EcsContext
{
	private Bitfield _bitfield = new(0);
	private readonly List<ArchetypeBuffer> _buffers;
	private readonly Dictionary<Type, ISystem> _systems;

	private int _freeEntityCount;
	private EntityInstance[] _freeEntities;

	#if DEBUG
	private readonly int _threadId = Environment.CurrentManagedThreadId;
	#endif

	public EcsContext(int initialCapacity = 512)
	{
		
		_buffers = new List<ArchetypeBuffer>();
		_systems = new Dictionary<Type, ISystem>();
		_freeEntities = new EntityInstance[initialCapacity];
	}

	public int ArchetypeCount { get; private set; }

	public Entity CreateEntity()
	{
		CheckThreadValidity();
		
		if (_freeEntityCount == 0)
			return new Entity {Instance = new EntityInstance {Version = 1}, Version = 1};
		
		var instance = _freeEntities[--_freeEntityCount];
		return new Entity {Instance = instance, Version = ++instance.Version};
	}

	[MethodImpl(MethodImplOptions.AggressiveOptimization)]
	public Entity CreateEntity(in Archetype archetype)
	{
		CheckThreadValidity();
		
		var buffer = _buffers[archetype.Id];
		var slot = buffer.GetSlot();

		if (_freeEntityCount == 0)
			return new Entity {Instance = new EntityInstance {Version = 1, Slot = slot}, Version = 1};
		
		var instance = _freeEntities[--_freeEntityCount];

		instance.Slot = slot;
		return new Entity {Instance = instance, Version = ++instance.Version};
	}

	public void DestroyEntity(in Entity entity)
	{
		CheckThreadValidity();
		CheckEntityValidity(entity, "Entity already destroyed.");
		
		if (_freeEntities.Length == _freeEntityCount)
			Array.Resize(ref _freeEntities, _freeEntityCount * 2);
		
		var instance = entity.Instance;
		_freeEntities[_freeEntityCount++] = instance;
		instance.Slot?.Chunk.ReturnSlot(instance.Slot);
		instance.Version++; instance.Slot = null;
	}
	
	public void DestroyEntity(in Span<Entity> entities)
	{
		CheckThreadValidity();

		if (_freeEntities.Length < _freeEntityCount + entities.Length)
			Array.Resize(ref _freeEntities, _freeEntityCount + entities.Length);
		
		for (var i = 0; i < entities.Length; i++)
		{
			var entity = entities[i];
			var instance = entity.Instance;
			CheckEntityValidity(entity, "Entity already destroyed.");
			
			_freeEntities[_freeEntityCount++] = instance;
			instance.Slot?.Chunk.ReturnSlot(instance.Slot);
			instance.Version++; instance.Slot = null;
		}
	}
	
	public Archetype CreateArchetype(params ComponentType[] types)
	{
		CheckThreadValidity();

		if (_bitfield.BitCount < ComponentType.Count)
			_bitfield = new Bitfield(ComponentType.Count);

		_bitfield.Clear();
		foreach (var t in types) _bitfield[t.Id] = true;

		var key = _bitfield.GetHashCode();
		if (_tryGetArchetype(key, out var archetype)) return archetype;
		AddArchetype(key, archetype = new Archetype { Id = ArchetypeCount++, ComponentTypes = new Bitfield(_bitfield) });
		return archetype;
	}

	public unsafe bool AddComponent<T>(in Entity entity, in T component = default) where T : struct
	{
		CheckThreadValidity();
		CheckEntityValidity(entity);
		var componentId = ComponentType<T>.ComponentId;

		if (_bitfield.BitCount < ComponentType.Count)
			_bitfield = new Bitfield(ComponentType.Count);
		
		//Populate bitfield
		var slot = entity.Instance.Slot;
		if (slot is not null)
		{
			var types = slot.Chunk.Archetype.ComponentTypes;
			if (types[componentId]) return false;
			types.Bits.AsSpan().CopyTo(_bitfield.Bits);
		}
		_bitfield[ComponentType<T>.ComponentId] = true;

		//Compute new archetype
		var key = _bitfield.GetHashCode();
		if (!_tryGetArchetype(key, out var newArchetype))
			AddArchetype(key, newArchetype = new Archetype { Id = ArchetypeCount++, ComponentTypes = new Bitfield(_bitfield) });

		//Copy data to the new archetype slot
		var newSlot = _buffers[newArchetype.Id].GetSlot();
		if (slot is not null)
		{
			var components = ComponentType.Types;
			var types = slot.Chunk.Archetype.ComponentTypes;
			lock (components)
			{
				for (var i = 0; i < types.BitCount; i++)
				{
					if(!types[i]) continue;
					var t = components[i];
					var src = slot.Chunk.GetComponentPtrUnsafe(t, slot.Slot);
					var dest = newSlot.Chunk.GetComponentPtrUnsafe(t, newSlot.Slot);
					Unsafe.CopyBlock(dest, src, (uint) t.Size);
				}
			}
			slot.Chunk.ReturnSlot(slot);
		}

		entity.Instance.Slot = newSlot;
		newSlot.Chunk.GetComponentRefUnsafe<T>(newSlot.Slot) = component;
		return true;
	}
	
	public unsafe bool RemoveComponent<T>(in Entity entity) where T : struct
	{
		CheckThreadValidity();
		CheckEntityValidity(entity);
		var componentId = ComponentType<T>.ComponentId;
		
		if (_bitfield.BitCount < ComponentType.Count)
			_bitfield = new Bitfield(ComponentType.Count);
		
		var slot = entity.Instance.Slot;
		if (slot is null) return false;

		//Populate bitfield
		var types = slot.Chunk.Archetype.ComponentTypes;
		if (!types[componentId]) return false;
		types.Bits.AsSpan().CopyTo(_bitfield.Bits);
		_bitfield[ComponentType<T>.ComponentId] = false;
		
		//Compute new archetype
		var key = _bitfield.GetHashCode();
		if (!_tryGetArchetype(key, out var newArchetype))
			AddArchetype(key, newArchetype = new Archetype { Id = ArchetypeCount++, ComponentTypes = new Bitfield(_bitfield) });

		//Copy data to the new archetype slot
		var newSlot = _buffers[newArchetype.Id].GetSlot();
		var components = ComponentType.Types;
		var newTypes = newArchetype.ComponentTypes;
		lock (components)
		{
			for (var i = 0; i < newTypes.BitCount; i++)
			{
				if(!newTypes[i]) continue;
				var t = components[i];
				var src = slot.Chunk.GetComponentPtrUnsafe(t, slot.Slot);
				var dest = newSlot.Chunk.GetComponentPtrUnsafe(t, newSlot.Slot);
				Unsafe.CopyBlock(dest, src, (uint) t.Size);
			}
		}
		slot.Chunk.ReturnSlot(slot);

		entity.Instance.Slot = newSlot;
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private ref T GetComponentRefUnsafe<T>(in Entity entity) where T : struct
	{
		CheckThreadValidity();
		CheckEntityValidity(entity);
		
		var instance = entity.Instance;
		if (instance.Slot is null || !instance.Slot.Chunk.Archetype.ComponentTypes[ComponentType<T>.ComponentId]) 
			Throw();
		
		void Throw() => throw new InvalidOperationException("Component is not attached to the entity.");
		return ref instance.Slot!.Chunk.GetComponentRefUnsafe<T>(instance.Slot.Slot);
	}

	public T GetComponent<T>(in Entity entity) where T : struct
		=> GetComponentRefUnsafe<T>(in entity);
	
	public T SetComponent<T>(in Entity entity, in T value) where T : struct
		=> GetComponentRefUnsafe<T>(in entity) = value;

	public bool RegisterSystem(ISystem system)
	{
		CheckThreadValidity();
		return _systems.TryAdd(system.GetType(), system);
	}

	public void RunSystems()
	{
		CheckThreadValidity();
		foreach (var system in _systems.Values)
		{
			var archetype = system.Archetype;
			foreach (var buffer in _buffers)
			{
				if(!buffer.Archetype.ComponentTypes.IsSubsetOf(archetype.ComponentTypes)) continue;
				system.ExecutionDelegate(buffer);
			}
		}
	}

	public void Optimise()
	{
		CheckThreadValidity();
		_buffers.RemoveAll(b =>
		{
			b.Trim();
			b.SortByUsedSlots();
			return b.Chunks.Count == 0;
		});
	}

	#region CodeGeneration

	private readonly List<SwitchCase> _archetypeCases = new();
	private delegate bool TryGetArchetypeFunc(int key, out Archetype archetype);
	
	private readonly ParameterExpression[] _getArchetypeParameters =
	{Expression.Parameter(typeof(int)), Expression.Parameter(typeof(Archetype).MakeByRefType())};
	
	private TryGetArchetypeFunc _tryGetArchetype = (int key, out Archetype archetype) =>
	{
		archetype = default;
		return false;
	};

	private void AddArchetype(int key, in Archetype archetype)
	{
		var assign = Expression.Assign(_getArchetypeParameters[1], Expression.Constant(archetype));
		_archetypeCases.Add(Expression.SwitchCase(Expression.Block(assign, Expression.Constant(true)), Expression.Constant(key)));
		var @switch = Expression.Switch(_getArchetypeParameters[0], Expression.Constant(false), _archetypeCases.ToArray());
		_tryGetArchetype = Expression.Lambda<TryGetArchetypeFunc>(@switch, "TryGetArchetype", _getArchetypeParameters).CompileFast();
		_buffers.Add(new ArchetypeBuffer(archetype));
	}

	#endregion

	[Conditional("DEBUG")]
	private void CheckThreadValidity()
	{
		#if DEBUG
		if (Environment.CurrentManagedThreadId != _threadId)
			throw new InvalidOperationException("Function cannot be called from another thread.");
		#endif
	}

	[Conditional("DEBUG")]
	private void CheckEntityValidity(in Entity entity, string message = "Invalid entity")
	{
		if (entity.Version != entity.Instance.Version)
			throw new InvalidOperationException(message);
	}
}