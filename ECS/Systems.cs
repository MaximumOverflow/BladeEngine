
namespace BladeEngine.ECS;

public interface ISystem
{
	public Archetype Archetype { get; }
	internal Action<ArchetypeBuffer> ExecutionDelegate { get; }
	protected static readonly ParallelOptions ParallelOptions = new()
	{
		MaxDegreeOfParallelism = Environment.ProcessorCount / 2
	};
}


public abstract class System<T0> : ISystem
where T0 : struct, IComponent
{
	public abstract Archetype Archetype { get; }
	protected abstract void Run(ref T0 t0);

	protected virtual Action<ArchetypeBuffer> ExecutionDelegate { get; }
	Action<ArchetypeBuffer> ISystem.ExecutionDelegate => ExecutionDelegate;
}

public abstract class System<T0, T1> : ISystem
where T0 : struct, IComponent
where T1 : struct, IComponent
{
	public abstract Archetype Archetype { get; }
	protected abstract void Run(ref T0 t0, ref T1 t1);

	protected virtual Action<ArchetypeBuffer> ExecutionDelegate { get; }
	Action<ArchetypeBuffer> ISystem.ExecutionDelegate => ExecutionDelegate;
}

public abstract class System<T0, T1, T2> : ISystem
where T0 : struct, IComponent
where T1 : struct, IComponent
where T2 : struct, IComponent
{
	public abstract Archetype Archetype { get; }
	protected abstract void Run(ref T0 t0, ref T1 t1, ref T2 t2);

	protected virtual Action<ArchetypeBuffer> ExecutionDelegate { get; }
	Action<ArchetypeBuffer> ISystem.ExecutionDelegate => ExecutionDelegate;
}

public abstract class System<T0, T1, T2, T3> : ISystem
where T0 : struct, IComponent
where T1 : struct, IComponent
where T2 : struct, IComponent
where T3 : struct, IComponent
{
	public abstract Archetype Archetype { get; }
	protected abstract void Run(ref T0 t0, ref T1 t1, ref T2 t2, ref T3 t3);

	protected virtual Action<ArchetypeBuffer> ExecutionDelegate { get; }
	Action<ArchetypeBuffer> ISystem.ExecutionDelegate => ExecutionDelegate;
}

public abstract class System<T0, T1, T2, T3, T4> : ISystem
where T0 : struct, IComponent
where T1 : struct, IComponent
where T2 : struct, IComponent
where T3 : struct, IComponent
where T4 : struct, IComponent
{
	public abstract Archetype Archetype { get; }
	protected abstract void Run(ref T0 t0, ref T1 t1, ref T2 t2, ref T3 t3, ref T4 t4);

	protected virtual Action<ArchetypeBuffer> ExecutionDelegate { get; }
	Action<ArchetypeBuffer> ISystem.ExecutionDelegate => ExecutionDelegate;
}

public abstract class System<T0, T1, T2, T3, T4, T5> : ISystem
where T0 : struct, IComponent
where T1 : struct, IComponent
where T2 : struct, IComponent
where T3 : struct, IComponent
where T4 : struct, IComponent
where T5 : struct, IComponent
{
	public abstract Archetype Archetype { get; }
	protected abstract void Run(ref T0 t0, ref T1 t1, ref T2 t2, ref T3 t3, ref T4 t4, ref T5 t5);

	protected virtual Action<ArchetypeBuffer> ExecutionDelegate { get; }
	Action<ArchetypeBuffer> ISystem.ExecutionDelegate => ExecutionDelegate;
}

public abstract class System<T0, T1, T2, T3, T4, T5, T6> : ISystem
where T0 : struct, IComponent
where T1 : struct, IComponent
where T2 : struct, IComponent
where T3 : struct, IComponent
where T4 : struct, IComponent
where T5 : struct, IComponent
where T6 : struct, IComponent
{
	public abstract Archetype Archetype { get; }
	protected abstract void Run(ref T0 t0, ref T1 t1, ref T2 t2, ref T3 t3, ref T4 t4, ref T5 t5, ref T6 t6);

	protected virtual Action<ArchetypeBuffer> ExecutionDelegate { get; }
	Action<ArchetypeBuffer> ISystem.ExecutionDelegate => ExecutionDelegate;
}

public abstract class System<T0, T1, T2, T3, T4, T5, T6, T7> : ISystem
where T0 : struct, IComponent
where T1 : struct, IComponent
where T2 : struct, IComponent
where T3 : struct, IComponent
where T4 : struct, IComponent
where T5 : struct, IComponent
where T6 : struct, IComponent
where T7 : struct, IComponent
{
	public abstract Archetype Archetype { get; }
	protected abstract void Run(ref T0 t0, ref T1 t1, ref T2 t2, ref T3 t3, ref T4 t4, ref T5 t5, ref T6 t6, ref T7 t7);

	protected virtual Action<ArchetypeBuffer> ExecutionDelegate { get; }
	Action<ArchetypeBuffer> ISystem.ExecutionDelegate => ExecutionDelegate;
}

public abstract class System<T0, T1, T2, T3, T4, T5, T6, T7, T8> : ISystem
where T0 : struct, IComponent
where T1 : struct, IComponent
where T2 : struct, IComponent
where T3 : struct, IComponent
where T4 : struct, IComponent
where T5 : struct, IComponent
where T6 : struct, IComponent
where T7 : struct, IComponent
where T8 : struct, IComponent
{
	public abstract Archetype Archetype { get; }
	protected abstract void Run(ref T0 t0, ref T1 t1, ref T2 t2, ref T3 t3, ref T4 t4, ref T5 t5, ref T6 t6, ref T7 t7, ref T8 t8);

	protected virtual Action<ArchetypeBuffer> ExecutionDelegate { get; }
	Action<ArchetypeBuffer> ISystem.ExecutionDelegate => ExecutionDelegate;
}

public abstract class System<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> : ISystem
where T0 : struct, IComponent
where T1 : struct, IComponent
where T2 : struct, IComponent
where T3 : struct, IComponent
where T4 : struct, IComponent
where T5 : struct, IComponent
where T6 : struct, IComponent
where T7 : struct, IComponent
where T8 : struct, IComponent
where T9 : struct, IComponent
{
	public abstract Archetype Archetype { get; }
	protected abstract void Run(ref T0 t0, ref T1 t1, ref T2 t2, ref T3 t3, ref T4 t4, ref T5 t5, ref T6 t6, ref T7 t7, ref T8 t8, ref T9 t9);

	protected virtual Action<ArchetypeBuffer> ExecutionDelegate { get; }
	Action<ArchetypeBuffer> ISystem.ExecutionDelegate => ExecutionDelegate;
}

public abstract class System<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : ISystem
where T0 : struct, IComponent
where T1 : struct, IComponent
where T2 : struct, IComponent
where T3 : struct, IComponent
where T4 : struct, IComponent
where T5 : struct, IComponent
where T6 : struct, IComponent
where T7 : struct, IComponent
where T8 : struct, IComponent
where T9 : struct, IComponent
where T10 : struct, IComponent
{
	public abstract Archetype Archetype { get; }
	protected abstract void Run(ref T0 t0, ref T1 t1, ref T2 t2, ref T3 t3, ref T4 t4, ref T5 t5, ref T6 t6, ref T7 t7, ref T8 t8, ref T9 t9, ref T10 t10);

	protected virtual Action<ArchetypeBuffer> ExecutionDelegate { get; }
	Action<ArchetypeBuffer> ISystem.ExecutionDelegate => ExecutionDelegate;
}

public abstract class System<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> : ISystem
where T0 : struct, IComponent
where T1 : struct, IComponent
where T2 : struct, IComponent
where T3 : struct, IComponent
where T4 : struct, IComponent
where T5 : struct, IComponent
where T6 : struct, IComponent
where T7 : struct, IComponent
where T8 : struct, IComponent
where T9 : struct, IComponent
where T10 : struct, IComponent
where T11 : struct, IComponent
{
	public abstract Archetype Archetype { get; }
	protected abstract void Run(ref T0 t0, ref T1 t1, ref T2 t2, ref T3 t3, ref T4 t4, ref T5 t5, ref T6 t6, ref T7 t7, ref T8 t8, ref T9 t9, ref T10 t10, ref T11 t11);

	protected virtual Action<ArchetypeBuffer> ExecutionDelegate { get; }
	Action<ArchetypeBuffer> ISystem.ExecutionDelegate => ExecutionDelegate;
}

public abstract class System<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> : ISystem
where T0 : struct, IComponent
where T1 : struct, IComponent
where T2 : struct, IComponent
where T3 : struct, IComponent
where T4 : struct, IComponent
where T5 : struct, IComponent
where T6 : struct, IComponent
where T7 : struct, IComponent
where T8 : struct, IComponent
where T9 : struct, IComponent
where T10 : struct, IComponent
where T11 : struct, IComponent
where T12 : struct, IComponent
{
	public abstract Archetype Archetype { get; }
	protected abstract void Run(ref T0 t0, ref T1 t1, ref T2 t2, ref T3 t3, ref T4 t4, ref T5 t5, ref T6 t6, ref T7 t7, ref T8 t8, ref T9 t9, ref T10 t10, ref T11 t11, ref T12 t12);

	protected virtual Action<ArchetypeBuffer> ExecutionDelegate { get; }
	Action<ArchetypeBuffer> ISystem.ExecutionDelegate => ExecutionDelegate;
}

public abstract class System<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> : ISystem
where T0 : struct, IComponent
where T1 : struct, IComponent
where T2 : struct, IComponent
where T3 : struct, IComponent
where T4 : struct, IComponent
where T5 : struct, IComponent
where T6 : struct, IComponent
where T7 : struct, IComponent
where T8 : struct, IComponent
where T9 : struct, IComponent
where T10 : struct, IComponent
where T11 : struct, IComponent
where T12 : struct, IComponent
where T13 : struct, IComponent
{
	public abstract Archetype Archetype { get; }
	protected abstract void Run(ref T0 t0, ref T1 t1, ref T2 t2, ref T3 t3, ref T4 t4, ref T5 t5, ref T6 t6, ref T7 t7, ref T8 t8, ref T9 t9, ref T10 t10, ref T11 t11, ref T12 t12, ref T13 t13);

	protected virtual Action<ArchetypeBuffer> ExecutionDelegate { get; }
	Action<ArchetypeBuffer> ISystem.ExecutionDelegate => ExecutionDelegate;
}

public abstract class System<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> : ISystem
where T0 : struct, IComponent
where T1 : struct, IComponent
where T2 : struct, IComponent
where T3 : struct, IComponent
where T4 : struct, IComponent
where T5 : struct, IComponent
where T6 : struct, IComponent
where T7 : struct, IComponent
where T8 : struct, IComponent
where T9 : struct, IComponent
where T10 : struct, IComponent
where T11 : struct, IComponent
where T12 : struct, IComponent
where T13 : struct, IComponent
where T14 : struct, IComponent
{
	public abstract Archetype Archetype { get; }
	protected abstract void Run(ref T0 t0, ref T1 t1, ref T2 t2, ref T3 t3, ref T4 t4, ref T5 t5, ref T6 t6, ref T7 t7, ref T8 t8, ref T9 t9, ref T10 t10, ref T11 t11, ref T12 t12, ref T13 t13, ref T14 t14);

	protected virtual Action<ArchetypeBuffer> ExecutionDelegate { get; }
	Action<ArchetypeBuffer> ISystem.ExecutionDelegate => ExecutionDelegate;
}

public abstract class System<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> : ISystem
where T0 : struct, IComponent
where T1 : struct, IComponent
where T2 : struct, IComponent
where T3 : struct, IComponent
where T4 : struct, IComponent
where T5 : struct, IComponent
where T6 : struct, IComponent
where T7 : struct, IComponent
where T8 : struct, IComponent
where T9 : struct, IComponent
where T10 : struct, IComponent
where T11 : struct, IComponent
where T12 : struct, IComponent
where T13 : struct, IComponent
where T14 : struct, IComponent
where T15 : struct, IComponent
{
	public abstract Archetype Archetype { get; }
	protected abstract void Run(ref T0 t0, ref T1 t1, ref T2 t2, ref T3 t3, ref T4 t4, ref T5 t5, ref T6 t6, ref T7 t7, ref T8 t8, ref T9 t9, ref T10 t10, ref T11 t11, ref T12 t12, ref T13 t13, ref T14 t14, ref T15 t15);

	protected virtual Action<ArchetypeBuffer> ExecutionDelegate { get; }
	Action<ArchetypeBuffer> ISystem.ExecutionDelegate => ExecutionDelegate;
}

