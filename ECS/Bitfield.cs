using System.Runtime.CompilerServices;
using System.Collections;

namespace BladeEngine.ECS;

public readonly struct Bitfield : IEnumerable<bool>
{
	public readonly int Bits;
	private readonly ulong[] _bits;
	public int Capacity => _bits.Length * 64;

	public Bitfield()
	{
		Bits = 64;
		_bits = new ulong[1];
	}

	public Bitfield(int minBits)
	{
		Bits = minBits;
		_bits = new ulong[(int) MathF.Ceiling((float) minBits / sizeof(ulong))];
	}

	public bool this[int index]
	{
		get
		{
			var b = _bits[index / 64];
			var i = index % 64;
			return (b & (1ul << i)) != 0;
		}

		set
		{
			var i = index % 64;
			ref var b = ref _bits[index / 64];
			b = value ? b | (1ul << i) : b & ~(1ul << i);
		}
	}

	public void Clear()
	{
		_bits.AsSpan().Clear();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override int GetHashCode()
	{
		var hash = new HashCode();
		foreach (var i in _bits) hash.Add(i);
		return hash.ToHashCode();
	}

	public IEnumerator<bool> GetEnumerator() => new BoolEnumerator(this);
	IEnumerator IEnumerable.GetEnumerator()  => new BoolEnumerator(this);

	private record struct BoolEnumerator(Bitfield Bitfield) : IEnumerator<bool>
	{
		private int _i = -1;
		public bool Current => Bitfield[_i];
		object IEnumerator.Current => Current;
		public bool MoveNext() => ++_i < Bitfield.Bits;
		public void Reset() => _i = -1;
		public void Dispose() {}
	}
}