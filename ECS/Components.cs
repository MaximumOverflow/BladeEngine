using System.Runtime.CompilerServices;

namespace BladeEngine.ECS;

public readonly struct ComponentType
{ 
	internal int Id { get; init; }
	internal int Size { get; init; }
	internal Func<int, object> CreateArray { get; init; }
	public override int GetHashCode() => Id;

	private static int _id;
	internal static int Count => _id;
	public static readonly List<ComponentType> Types = new();
	internal static int GetId() => Interlocked.Increment(ref _id) - 1;
}

public struct ComponentType<T> where T : struct
{
	public static readonly int ComponentId;

	static ComponentType()
	{
		lock (ComponentType.Types)
		{
			ComponentId = ComponentType.GetId();
			ComponentType.Types.Add(new ComponentType<T>());
		}
	}
	
	public static implicit operator ComponentType(in ComponentType<T> _) => new()
	{
		Id = ComponentId, 
		Size = Unsafe.SizeOf<T>(),
		CreateArray = CreateArray
	};

	public static T[] CreateArray(int size) => new T[size];
}

public static class ComponentTypeUtilities
{
	public static BitfieldEnumerator EnumerateComponentTypes(this Bitfield bitfield)
	{
		return new BitfieldEnumerator(bitfield, ComponentType.Types);
	}
	
	public static NewBitfieldEnumerator NewEnumerateComponentTypes(this Bitfield bitfield)
	{
		return new NewBitfieldEnumerator(bitfield, ComponentType.Types);
	}

	public struct BitfieldEnumerator
	{
		private int _index;
		private readonly Bitfield _bitfield;
		private readonly List<ComponentType> _types;

		public ComponentType Current { get; private set; }

		public BitfieldEnumerator(in Bitfield bitfield, List<ComponentType> types)
		{
			_index = 0;
			_types = types;
			Current = default;
			_bitfield = bitfield;
			Monitor.Enter(types);
		}

		public bool MoveNext()
		{
			while (_index < _bitfield.BitCount)
				if (_bitfield[_index++])
				{
					Current = _types[_index - 1];
					return true;
				}

			return false;
		}

		public void Reset() => _index = 0;

		public void Dispose()
		{
			Monitor.Exit(_types);
		}
		public BitfieldEnumerator GetEnumerator() => this;
	}
	
	public struct NewBitfieldEnumerator
	{
		private int _index;
		private readonly int _limit;
		private readonly Bitfield _bitfield;
		private readonly List<ComponentType> _types;
		
		public ComponentType Current => _types[_index];

		public NewBitfieldEnumerator(in Bitfield bitfield, List<ComponentType> types)
		{
			_index = -1;
			_types = types;
			_bitfield = bitfield;
			_limit = bitfield.BitCount - 1;
			Monitor.Enter(types);
		}

		public bool MoveNext()
		{
			while (_index < _limit)
				if (_bitfield[++_index])
					return true;

			return false;
		}

		public void Reset() => _index = 0;

		public void Dispose()
		{
			Monitor.Exit(_types);
		}
		public NewBitfieldEnumerator GetEnumerator() => this;
	}
}