namespace BladeEngine.ECS;

public struct Entity
{
	internal int Version;
	internal EntityInstance Instance;

	public static bool operator ==(Entity a, Entity b)
		=> a.Instance == b.Instance && a.Version == b.Version;

	public static bool operator !=(Entity a, Entity b) 
		=> a.Instance != b.Instance || a.Version != b.Version;

	public bool Equals(Entity other)
		=> Instance == other.Instance && Version == other.Version;

	public override bool Equals(object? obj)
		=> obj is Entity other && Equals(other);

	public override int GetHashCode()
		=> unchecked((Instance.GetHashCode() * 397) ^ Version);
}

internal class EntityInstance
{
	internal int Version;
	internal ArchetypeSlot? Slot;
}