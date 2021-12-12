namespace BladeEngine.ECS;

public struct Entity
{
	internal int Id;
	internal int Version;

	public static bool operator ==(Entity a, Entity b)
		=> a.Id == b.Id && a.Version == b.Version;

	public static bool operator !=(Entity a, Entity b) 
		=> a.Id != b.Id || a.Version != b.Version;

	public bool Equals(Entity other)
		=> Id == other.Id && Version == other.Version;

	public override bool Equals(object? obj)
		=> obj is Entity other && Equals(other);

	public override int GetHashCode()
		=> unchecked((Id * 397) ^ Version);

	public override string ToString()
		=> $"Entity {{ Id = {Id}, Version = {Version} }}";
}

internal class InternalEntity
{
	internal int Version;
	internal ArchetypeSlot? Slot;
}