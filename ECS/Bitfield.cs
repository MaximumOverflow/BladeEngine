using System.Runtime.CompilerServices;

namespace BladeEngine.ECS;

public readonly struct Bitfield
{
	public readonly int BitCount;
	internal readonly ulong[] Bits;
	public int Capacity => Bits.Length * 64;

	public Bitfield()
	{
		BitCount = 64;
		Bits = new ulong[1];
	}

	public Bitfield(int minBitCount)
	{
		BitCount = minBitCount;
		Bits = new ulong[(int) MathF.Ceiling((float) minBitCount / sizeof(ulong))];
	}

	public Bitfield(in Bitfield other)
	{
		BitCount = other.BitCount;
		Bits = new ulong[other.Bits.Length];
		other.Bits.AsSpan().CopyTo(Bits);
	}

	public bool this[int index]
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			var b = Bits[index / 64];
			var i = index % 64;
			return (b & (1ul << i)) != 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			var i = index % 64;
			ref var b = ref Bits[index / 64];
			b = value ? b | (1ul << i) : b & ~(1ul << i);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveOptimization)]
	public bool IsSubsetOf(in Bitfield bitfield)
	{
		var size = Math.Min(Bits.Length, bitfield.Bits.Length); 
		
		for(var i = 0; i < size; i++)
			if ((bitfield.Bits[i] & Bits[i]) != Bits[i])
				return false;

		return true;
	}

	public void Clear()
	{
		Bits.AsSpan().Clear();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override int GetHashCode()
	{
		var hash = new HashCode();
		foreach (var i in Bits) hash.Add(i);
		return hash.ToHashCode();
	}
}