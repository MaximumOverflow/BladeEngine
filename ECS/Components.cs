﻿namespace BladeEngine.ECS;

public interface IComponent {}

public readonly struct ComponentType
{
	private static int _id;
	internal static int Count => _id;
	internal static int GetId() => Interlocked.Increment(ref _id) - 1;
	
	internal int Id { get; init; }
	internal int Size { get; init; }
	public override int GetHashCode() => Id;
}

public unsafe class ComponentType<T> where T : unmanaged, IComponent
{
	public static readonly int ComponentId = ComponentType.GetId();
	public static implicit operator ComponentType(ComponentType<T> _) => new()
	{
		Id = ComponentId, Size = sizeof(T)
	};
}