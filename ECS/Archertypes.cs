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

internal class ArchetypeBuffer
{
	public readonly Archetype Archetype;
	private readonly List<ArchetypeBufferChunk> _chunks;
	public IReadOnlyList<ArchetypeBufferChunk> Chunks => _chunks;

	public ArchetypeBuffer(Archetype archetype)
	{
		Archetype = archetype;
		_chunks = new List<ArchetypeBufferChunk>();
	}

	public ArchetypeSlot GetSlot()
	{
		var span = CollectionsMarshal.AsSpan(_chunks);
		for (var i = 0; i < span.Length; i++)
		{
			if (span[i].TryGetSlot(out var slot))
			{
				if(i != 0) span.Sort((a, b) =>
				{
					if (a.UsedSlots == b.UsedSlots) return 0;
					return b.UsedSlots == 0 ? -1 : a.UsedSlots.CompareTo(b.UsedSlots);
				});
				return slot!;
			}
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
			return b.UsedSlots == 0 ? -1 : a.UsedSlots.CompareTo(b.UsedSlots);
		});
	}
}

internal unsafe class ArchetypeBufferChunk : IDisposable
{
	public const int Size = 256;
	public readonly Archetype Archetype;

	//Do not edit outside of this class
	public int UsedSlots;
	private bool _requiresCompacting;

	private readonly ArchetypeSlot[] _slots;
	private readonly Dictionary<int, IntPtr> _buffers;

	public ArchetypeBufferChunk(ArchetypeBuffer buffer)
	{
		Archetype = buffer.Archetype;
		_slots = new ArchetypeSlot[Size];
		_buffers = new Dictionary<int, IntPtr>(buffer.Archetype.ComponentTypes.Count);
		
		for (var i = 0; i < _slots.Length; i++)
			_slots[i] = new ArchetypeSlot(i, this);
		
		foreach (var type in buffer.Archetype.ComponentTypes)
			_buffers.Add(type.Id, (IntPtr) NativeMemory.Alloc((nuint) (Size * type.Size)));
	}
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool TryGetSlot(out ArchetypeSlot? slot)
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

	public void ReturnSlot(ArchetypeSlot slot)
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
			var buffer = (byte*) _buffers[type.Id];
			var insert = buffer;
			for (var i = 0; i < UsedSlots; i++)
			{
				var slot = _slots[i];
				var source = buffer + type.Size * slot.Slot;
				Unsafe.CopyBlock(insert, source, (uint) type.Size);
				insert += type.Size;
			}
		}

		//Rewire indices
		for (var i = 0; i < Size; i++) _slots[i].Slot = i;

		_requiresCompacting = false;
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Span<T> GetComponentSpan<T>() where T : unmanaged, IComponent
	{
		return new Span<T>((void*) _buffers[ComponentType<T>.ComponentId], UsedSlots);
	}
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ref T GetComponentRefUnsafe<T>(int index) where T : unmanaged, IComponent
	{
		return ref ((T*) _buffers[ComponentType<T>.ComponentId])[index];
	}

	public void Dispose()
	{
		foreach (var buffer in _buffers.Values)
			NativeMemory.Free((void*) buffer);
	}
}