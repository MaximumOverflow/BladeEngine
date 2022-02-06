using FastExpressionCompiler.LightExpression;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Collections.Concurrent;
using System.Reflection;

namespace BladeEngine.ECS;

public readonly struct Archetype
{
	internal int Id { get; init; }
	public Bitfield ComponentTypes { get; init; }

	public override int GetHashCode() => Id;
	public static bool operator ==(in Archetype a, in Archetype b) => a.Id == b.Id;
	public static bool operator !=(in Archetype a, in Archetype b) => a.Id != b.Id;

	public bool Equals(Archetype other) => Id == other.Id;
	public override bool Equals(object? obj) => obj is Archetype other && Equals(other);
}

internal class ArchetypeSlot
{
	public int Slot;
	public bool Taken;
	public readonly ArchetypeBufferChunk Chunk;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ArchetypeSlot(int slot, ArchetypeBufferChunk chunk)
	{
		Slot = slot;
		Chunk = chunk;
	}
}

public class ArchetypeBuffer
{
	internal readonly Archetype Archetype;
	private readonly List<ArchetypeBufferChunk> _chunks;
	public IReadOnlyList<ArchetypeBufferChunk> Chunks => _chunks;

	internal ArchetypeBuffer(in Archetype archetype)
	{
		Archetype = archetype;
		_chunks = new List<ArchetypeBufferChunk>();
	}

	internal ArchetypeSlot GetSlot()
	{
		var span = CollectionsMarshal.AsSpan(_chunks);
		for (var i = 0; i < span.Length; i++)
		{
			var chunk = span[i];
			if (!chunk.TryGetSlot(out var slot)) continue;
			if (chunk.UsedSlots != ArchetypeBufferChunk.Size) return slot!;
			_chunks.RemoveAt(i);
			_chunks.Add(chunk);
			return slot!;
		}

		var newChunk = new ArchetypeBufferChunk(this);
		newChunk.TryGetSlot(out var newSlot);
		_chunks.Insert(0, newChunk);
		return newSlot!;
	}

	public void SortByUsedSlots()
	{
		var span = CollectionsMarshal.AsSpan(_chunks);
		span.Sort((a, b) =>
		{
			if (a.UsedSlots == b.UsedSlots) return 0;
			if (b.UsedSlots == ArchetypeBufferChunk.Size) return -1;
			if (a.UsedSlots == ArchetypeBufferChunk.Size) return +1;
			if (a.UsedSlots == 0) return +1;
			if (b.UsedSlots == 0) return -1;
			return a.UsedSlots.CompareTo(b.UsedSlots);
		});
	}

	public void Trim()
	{
		_chunks.RemoveAll(c => c.UsedSlots == 0);
	}
}

public unsafe class ArchetypeBufferChunk
{
	internal const int Size = 256;
	internal readonly Archetype Archetype;

	//Do not edit outside of this class
	public int UsedSlots;
	private bool _requiresCompacting;

	private readonly object[] _buffers;
	private readonly ArchetypeSlot[] _slots;
	private readonly GetBufferFunc GetBuffer;

	internal ArchetypeBufferChunk(ArchetypeBuffer buffer)
	{
		Archetype = buffer.Archetype;
		_slots = new ArchetypeSlot[Size];
		
		for (var i = 0; i < _slots.Length; i++)
			_slots[i] = new ArchetypeSlot(i, this);

		var linear = new List<object>();
		foreach (var type in buffer.Archetype.ComponentTypes.EnumerateComponentTypes())
			linear.Add(type.CreateArray(Size));
		_buffers = linear.ToArray();
		GetBuffer = MakeGetBufferFunc(Archetype);
	}
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal bool TryGetSlot(out ArchetypeSlot? slot)
	{
		if (UsedSlots == Size)
		{
			slot = default;
			return false;
		}

		slot = _slots[UsedSlots++];
		slot.Taken = true;
		return true;
	}

	internal void ReturnSlot(ArchetypeSlot slot)
	{
		_requiresCompacting = true;
		slot.Taken = false;
		UsedSlots--;
	}

	public bool Compact()
	{
		if(!_requiresCompacting) return false;
		//Put used buffers at the start
		Array.Sort(_slots, (a, b) => -a.Taken.CompareTo(b.Taken));
		
		//Compact buffer memory
		var types = _slots[0].Chunk.Archetype.ComponentTypes;
		foreach (var type in types.EnumerateComponentTypes())
		{
			fixed (byte* buffer = Unsafe.As<byte[]>(GetBuffer(this, type.Id)))
			{
				var insert = buffer;
				for (var i = 0; i < UsedSlots; i++)
				{
					var slot = _slots[i];
					var source = buffer + type.Size * slot.Slot;
					Unsafe.CopyBlock(insert, source, (uint) type.Size);
					insert += type.Size;
				}
				
				Unsafe.InitBlock(buffer + type.Size * UsedSlots, 0, (uint) (type.Size * (Size - UsedSlots)));
			}
		}

		//Rewire indices
		for (var i = 0; i < Size; i++) _slots[i].Slot = i;

		_requiresCompacting = false;
		return true;
	}
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Span<T> GetComponentSpan<T>() where T : struct
	{
		var buffer = GetBuffer(this, ComponentType<T>.ComponentId);
		return Unsafe.As<T[]>(buffer).AsSpan(0, UsedSlots);
	}
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ref T GetComponentRefUnsafe<T>(int index) where T : struct
	{
		var buffer = GetBuffer(this, ComponentType<T>.ComponentId);
		return ref Unsafe.As<T[]>(buffer)[index];
	}
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void* GetComponentPtrUnsafe(in ComponentType type, int index)
	{
		var buffer = GetBuffer(this, type.Id);
		fixed (byte* ptr = Unsafe.As<byte[]>(buffer))
			return ptr + type.Size * index;
	}

	#region CodeGeneration

	private delegate object GetBufferFunc(ArchetypeBufferChunk chunk, int typeId);
	private static readonly ConcurrentDictionary<int, GetBufferFunc> GetBufferFuncs = new();

	private static GetBufferFunc MakeGetBufferFunc(Archetype archetype)
	{
		if (GetBufferFuncs.TryGetValue(archetype.Id, out var func))
			return func;

		//Parameters
		var chunk = Expression.Parameter(typeof(ArchetypeBufferChunk), "chunk");
		var typeId = Expression.Parameter(typeof(int), "typeId");

		//Switch
		var cases = new List<SwitchCase>();
		var buffers = typeof(ArchetypeBufferChunk).GetField(nameof(_buffers), BindingFlags.NonPublic | BindingFlags.Instance)!;
		foreach (var type in archetype.ComponentTypes.EnumerateComponentTypes())
		{
			var value = Expression.ArrayAccess(Expression.MakeMemberAccess(chunk, buffers), Expression.Constant(cases.Count));
			cases.Add(Expression.SwitchCase(value, Expression.Constant(type.Id)));
		}
		var @switch = Expression.Switch(typeId, Expression.Constant(null, typeof(object)), cases.ToArray());
		func = Expression.Lambda<GetBufferFunc>(@switch, chunk, typeId).CompileFast();
		GetBufferFuncs.TryAdd(archetype.Id, func);
		return func;
	}

	#endregion
}