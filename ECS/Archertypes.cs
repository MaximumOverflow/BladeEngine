using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace BladeEngine.ECS;

public readonly struct Archetype
{
	internal int Id { get; init; }
	internal IReadOnlySet<ComponentType> ComponentTypes { get; init; }
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

	internal ArchetypeBuffer(Archetype archetype)
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
}

public unsafe class ArchetypeBufferChunk
{
	internal const int Size = 256;
	private readonly Archetype Archetype;

	//Do not edit outside of this class
	public int UsedSlots;
	private bool _requiresCompacting;

	private readonly ArchetypeSlot[] _slots;
	private readonly Dictionary<int, object> _buffers;

	internal ArchetypeBufferChunk(ArchetypeBuffer buffer)
	{
		Archetype = buffer.Archetype;
		_slots = new ArchetypeSlot[Size];
		_buffers = new Dictionary<int, object>(buffer.Archetype.ComponentTypes.Count);
		
		for (var i = 0; i < _slots.Length; i++)
			_slots[i] = new ArchetypeSlot(i, this);
		
		foreach (var type in buffer.Archetype.ComponentTypes)
			_buffers.Add(type.Id, type.CreateArray(Size));
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
		foreach (var type in types)
		{
			fixed (byte* buffer = Unsafe.As<byte[]>(_buffers[type.Id]))
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
	public Span<T> GetComponentSpan<T>() where T : struct, IComponent
	{
		return Unsafe.As<T[]>(_buffers[ComponentType<T>.ComponentId]).AsSpan(0, UsedSlots);
	}
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ref T GetComponentRefUnsafe<T>(int index) where T : struct, IComponent
	{
		return ref Unsafe.As<T[]>(_buffers[ComponentType<T>.ComponentId])[index];
	}
}