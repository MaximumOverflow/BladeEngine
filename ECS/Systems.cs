
namespace BladeEngine.ECS;

public interface ISystem
{
	public Archetype Archetype { get; }
	internal void Run(ArchetypeBuffer buffer);
	protected static readonly ParallelOptions ParallelOptions = new()
	{
		MaxDegreeOfParallelism = Environment.ProcessorCount / 2
	};
}


public abstract class System<T0> : ISystem
where T0 : unmanaged, IComponent
{
	public abstract Archetype Archetype { get; }
	protected abstract void Run(ref T0 t0);
	
	void ISystem.Run(ArchetypeBuffer buffer)
	{
		Parallel.ForEach(buffer.Chunks, ISystem.ParallelOptions, chunk =>
		{
			chunk.Compact();
			var t0 = chunk.GetComponentSpan<T0>();
			for(var i = 0; i < t0.Length; i++) Run(ref t0[i]);
		});
	}
}
public abstract class System<T0, T1> : ISystem
where T0 : unmanaged, IComponent
where T1 : unmanaged, IComponent
{
	public abstract Archetype Archetype { get; }
	protected abstract void Run(ref T0 t0, ref T1 t1);
	
	void ISystem.Run(ArchetypeBuffer buffer)
	{
		Parallel.ForEach(buffer.Chunks, ISystem.ParallelOptions, chunk =>
		{
			chunk.Compact();
			var t0 = chunk.GetComponentSpan<T0>();
			var t1 = chunk.GetComponentSpan<T1>();
			for(var i = 0; i < t0.Length; i++) Run(ref t0[i], ref t1[i]);
		});
	}
}
public abstract class System<T0, T1, T2> : ISystem
where T0 : unmanaged, IComponent
where T1 : unmanaged, IComponent
where T2 : unmanaged, IComponent
{
	public abstract Archetype Archetype { get; }
	protected abstract void Run(ref T0 t0, ref T1 t1, ref T2 t2);
	
	void ISystem.Run(ArchetypeBuffer buffer)
	{
		Parallel.ForEach(buffer.Chunks, ISystem.ParallelOptions, chunk =>
		{
			chunk.Compact();
			var t0 = chunk.GetComponentSpan<T0>();
			var t1 = chunk.GetComponentSpan<T1>();
			var t2 = chunk.GetComponentSpan<T2>();
			for(var i = 0; i < t0.Length; i++) Run(ref t0[i], ref t1[i], ref t2[i]);
		});
	}
}
public abstract class System<T0, T1, T2, T3> : ISystem
where T0 : unmanaged, IComponent
where T1 : unmanaged, IComponent
where T2 : unmanaged, IComponent
where T3 : unmanaged, IComponent
{
	public abstract Archetype Archetype { get; }
	protected abstract void Run(ref T0 t0, ref T1 t1, ref T2 t2, ref T3 t3);
	
	void ISystem.Run(ArchetypeBuffer buffer)
	{
		Parallel.ForEach(buffer.Chunks, ISystem.ParallelOptions, chunk =>
		{
			chunk.Compact();
			var t0 = chunk.GetComponentSpan<T0>();
			var t1 = chunk.GetComponentSpan<T1>();
			var t2 = chunk.GetComponentSpan<T2>();
			var t3 = chunk.GetComponentSpan<T3>();
			for(var i = 0; i < t0.Length; i++) Run(ref t0[i], ref t1[i], ref t2[i], ref t3[i]);
		});
	}
}
public abstract class System<T0, T1, T2, T3, T4> : ISystem
where T0 : unmanaged, IComponent
where T1 : unmanaged, IComponent
where T2 : unmanaged, IComponent
where T3 : unmanaged, IComponent
where T4 : unmanaged, IComponent
{
	public abstract Archetype Archetype { get; }
	protected abstract void Run(ref T0 t0, ref T1 t1, ref T2 t2, ref T3 t3, ref T4 t4);
	
	void ISystem.Run(ArchetypeBuffer buffer)
	{
		Parallel.ForEach(buffer.Chunks, ISystem.ParallelOptions, chunk =>
		{
			chunk.Compact();
			var t0 = chunk.GetComponentSpan<T0>();
			var t1 = chunk.GetComponentSpan<T1>();
			var t2 = chunk.GetComponentSpan<T2>();
			var t3 = chunk.GetComponentSpan<T3>();
			var t4 = chunk.GetComponentSpan<T4>();
			for(var i = 0; i < t0.Length; i++) Run(ref t0[i], ref t1[i], ref t2[i], ref t3[i], ref t4[i]);
		});
	}
}
public abstract class System<T0, T1, T2, T3, T4, T5> : ISystem
where T0 : unmanaged, IComponent
where T1 : unmanaged, IComponent
where T2 : unmanaged, IComponent
where T3 : unmanaged, IComponent
where T4 : unmanaged, IComponent
where T5 : unmanaged, IComponent
{
	public abstract Archetype Archetype { get; }
	protected abstract void Run(ref T0 t0, ref T1 t1, ref T2 t2, ref T3 t3, ref T4 t4, ref T5 t5);
	
	void ISystem.Run(ArchetypeBuffer buffer)
	{
		Parallel.ForEach(buffer.Chunks, ISystem.ParallelOptions, chunk =>
		{
			chunk.Compact();
			var t0 = chunk.GetComponentSpan<T0>();
			var t1 = chunk.GetComponentSpan<T1>();
			var t2 = chunk.GetComponentSpan<T2>();
			var t3 = chunk.GetComponentSpan<T3>();
			var t4 = chunk.GetComponentSpan<T4>();
			var t5 = chunk.GetComponentSpan<T5>();
			for(var i = 0; i < t0.Length; i++) Run(ref t0[i], ref t1[i], ref t2[i], ref t3[i], ref t4[i], ref t5[i]);
		});
	}
}
public abstract class System<T0, T1, T2, T3, T4, T5, T6> : ISystem
where T0 : unmanaged, IComponent
where T1 : unmanaged, IComponent
where T2 : unmanaged, IComponent
where T3 : unmanaged, IComponent
where T4 : unmanaged, IComponent
where T5 : unmanaged, IComponent
where T6 : unmanaged, IComponent
{
	public abstract Archetype Archetype { get; }
	protected abstract void Run(ref T0 t0, ref T1 t1, ref T2 t2, ref T3 t3, ref T4 t4, ref T5 t5, ref T6 t6);
	
	void ISystem.Run(ArchetypeBuffer buffer)
	{
		Parallel.ForEach(buffer.Chunks, ISystem.ParallelOptions, chunk =>
		{
			chunk.Compact();
			var t0 = chunk.GetComponentSpan<T0>();
			var t1 = chunk.GetComponentSpan<T1>();
			var t2 = chunk.GetComponentSpan<T2>();
			var t3 = chunk.GetComponentSpan<T3>();
			var t4 = chunk.GetComponentSpan<T4>();
			var t5 = chunk.GetComponentSpan<T5>();
			var t6 = chunk.GetComponentSpan<T6>();
			for(var i = 0; i < t0.Length; i++) Run(ref t0[i], ref t1[i], ref t2[i], ref t3[i], ref t4[i], ref t5[i], ref t6[i]);
		});
	}
}
public abstract class System<T0, T1, T2, T3, T4, T5, T6, T7> : ISystem
where T0 : unmanaged, IComponent
where T1 : unmanaged, IComponent
where T2 : unmanaged, IComponent
where T3 : unmanaged, IComponent
where T4 : unmanaged, IComponent
where T5 : unmanaged, IComponent
where T6 : unmanaged, IComponent
where T7 : unmanaged, IComponent
{
	public abstract Archetype Archetype { get; }
	protected abstract void Run(ref T0 t0, ref T1 t1, ref T2 t2, ref T3 t3, ref T4 t4, ref T5 t5, ref T6 t6, ref T7 t7);
	
	void ISystem.Run(ArchetypeBuffer buffer)
	{
		Parallel.ForEach(buffer.Chunks, ISystem.ParallelOptions, chunk =>
		{
			chunk.Compact();
			var t0 = chunk.GetComponentSpan<T0>();
			var t1 = chunk.GetComponentSpan<T1>();
			var t2 = chunk.GetComponentSpan<T2>();
			var t3 = chunk.GetComponentSpan<T3>();
			var t4 = chunk.GetComponentSpan<T4>();
			var t5 = chunk.GetComponentSpan<T5>();
			var t6 = chunk.GetComponentSpan<T6>();
			var t7 = chunk.GetComponentSpan<T7>();
			for(var i = 0; i < t0.Length; i++) Run(ref t0[i], ref t1[i], ref t2[i], ref t3[i], ref t4[i], ref t5[i], ref t6[i], ref t7[i]);
		});
	}
}
public abstract class System<T0, T1, T2, T3, T4, T5, T6, T7, T8> : ISystem
where T0 : unmanaged, IComponent
where T1 : unmanaged, IComponent
where T2 : unmanaged, IComponent
where T3 : unmanaged, IComponent
where T4 : unmanaged, IComponent
where T5 : unmanaged, IComponent
where T6 : unmanaged, IComponent
where T7 : unmanaged, IComponent
where T8 : unmanaged, IComponent
{
	public abstract Archetype Archetype { get; }
	protected abstract void Run(ref T0 t0, ref T1 t1, ref T2 t2, ref T3 t3, ref T4 t4, ref T5 t5, ref T6 t6, ref T7 t7, ref T8 t8);
	
	void ISystem.Run(ArchetypeBuffer buffer)
	{
		Parallel.ForEach(buffer.Chunks, ISystem.ParallelOptions, chunk =>
		{
			chunk.Compact();
			var t0 = chunk.GetComponentSpan<T0>();
			var t1 = chunk.GetComponentSpan<T1>();
			var t2 = chunk.GetComponentSpan<T2>();
			var t3 = chunk.GetComponentSpan<T3>();
			var t4 = chunk.GetComponentSpan<T4>();
			var t5 = chunk.GetComponentSpan<T5>();
			var t6 = chunk.GetComponentSpan<T6>();
			var t7 = chunk.GetComponentSpan<T7>();
			var t8 = chunk.GetComponentSpan<T8>();
			for(var i = 0; i < t0.Length; i++) Run(ref t0[i], ref t1[i], ref t2[i], ref t3[i], ref t4[i], ref t5[i], ref t6[i], ref t7[i], ref t8[i]);
		});
	}
}
public abstract class System<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> : ISystem
where T0 : unmanaged, IComponent
where T1 : unmanaged, IComponent
where T2 : unmanaged, IComponent
where T3 : unmanaged, IComponent
where T4 : unmanaged, IComponent
where T5 : unmanaged, IComponent
where T6 : unmanaged, IComponent
where T7 : unmanaged, IComponent
where T8 : unmanaged, IComponent
where T9 : unmanaged, IComponent
{
	public abstract Archetype Archetype { get; }
	protected abstract void Run(ref T0 t0, ref T1 t1, ref T2 t2, ref T3 t3, ref T4 t4, ref T5 t5, ref T6 t6, ref T7 t7, ref T8 t8, ref T9 t9);
	
	void ISystem.Run(ArchetypeBuffer buffer)
	{
		Parallel.ForEach(buffer.Chunks, ISystem.ParallelOptions, chunk =>
		{
			chunk.Compact();
			var t0 = chunk.GetComponentSpan<T0>();
			var t1 = chunk.GetComponentSpan<T1>();
			var t2 = chunk.GetComponentSpan<T2>();
			var t3 = chunk.GetComponentSpan<T3>();
			var t4 = chunk.GetComponentSpan<T4>();
			var t5 = chunk.GetComponentSpan<T5>();
			var t6 = chunk.GetComponentSpan<T6>();
			var t7 = chunk.GetComponentSpan<T7>();
			var t8 = chunk.GetComponentSpan<T8>();
			var t9 = chunk.GetComponentSpan<T9>();
			for(var i = 0; i < t0.Length; i++) Run(ref t0[i], ref t1[i], ref t2[i], ref t3[i], ref t4[i], ref t5[i], ref t6[i], ref t7[i], ref t8[i], ref t9[i]);
		});
	}
}
public abstract class System<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : ISystem
where T0 : unmanaged, IComponent
where T1 : unmanaged, IComponent
where T2 : unmanaged, IComponent
where T3 : unmanaged, IComponent
where T4 : unmanaged, IComponent
where T5 : unmanaged, IComponent
where T6 : unmanaged, IComponent
where T7 : unmanaged, IComponent
where T8 : unmanaged, IComponent
where T9 : unmanaged, IComponent
where T10 : unmanaged, IComponent
{
	public abstract Archetype Archetype { get; }
	protected abstract void Run(ref T0 t0, ref T1 t1, ref T2 t2, ref T3 t3, ref T4 t4, ref T5 t5, ref T6 t6, ref T7 t7, ref T8 t8, ref T9 t9, ref T10 t10);
	
	void ISystem.Run(ArchetypeBuffer buffer)
	{
		Parallel.ForEach(buffer.Chunks, ISystem.ParallelOptions, chunk =>
		{
			chunk.Compact();
			var t0 = chunk.GetComponentSpan<T0>();
			var t1 = chunk.GetComponentSpan<T1>();
			var t2 = chunk.GetComponentSpan<T2>();
			var t3 = chunk.GetComponentSpan<T3>();
			var t4 = chunk.GetComponentSpan<T4>();
			var t5 = chunk.GetComponentSpan<T5>();
			var t6 = chunk.GetComponentSpan<T6>();
			var t7 = chunk.GetComponentSpan<T7>();
			var t8 = chunk.GetComponentSpan<T8>();
			var t9 = chunk.GetComponentSpan<T9>();
			var t10 = chunk.GetComponentSpan<T10>();
			for(var i = 0; i < t0.Length; i++) Run(ref t0[i], ref t1[i], ref t2[i], ref t3[i], ref t4[i], ref t5[i], ref t6[i], ref t7[i], ref t8[i], ref t9[i], ref t10[i]);
		});
	}
}
public abstract class System<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> : ISystem
where T0 : unmanaged, IComponent
where T1 : unmanaged, IComponent
where T2 : unmanaged, IComponent
where T3 : unmanaged, IComponent
where T4 : unmanaged, IComponent
where T5 : unmanaged, IComponent
where T6 : unmanaged, IComponent
where T7 : unmanaged, IComponent
where T8 : unmanaged, IComponent
where T9 : unmanaged, IComponent
where T10 : unmanaged, IComponent
where T11 : unmanaged, IComponent
{
	public abstract Archetype Archetype { get; }
	protected abstract void Run(ref T0 t0, ref T1 t1, ref T2 t2, ref T3 t3, ref T4 t4, ref T5 t5, ref T6 t6, ref T7 t7, ref T8 t8, ref T9 t9, ref T10 t10, ref T11 t11);
	
	void ISystem.Run(ArchetypeBuffer buffer)
	{
		Parallel.ForEach(buffer.Chunks, ISystem.ParallelOptions, chunk =>
		{
			chunk.Compact();
			var t0 = chunk.GetComponentSpan<T0>();
			var t1 = chunk.GetComponentSpan<T1>();
			var t2 = chunk.GetComponentSpan<T2>();
			var t3 = chunk.GetComponentSpan<T3>();
			var t4 = chunk.GetComponentSpan<T4>();
			var t5 = chunk.GetComponentSpan<T5>();
			var t6 = chunk.GetComponentSpan<T6>();
			var t7 = chunk.GetComponentSpan<T7>();
			var t8 = chunk.GetComponentSpan<T8>();
			var t9 = chunk.GetComponentSpan<T9>();
			var t10 = chunk.GetComponentSpan<T10>();
			var t11 = chunk.GetComponentSpan<T11>();
			for(var i = 0; i < t0.Length; i++) Run(ref t0[i], ref t1[i], ref t2[i], ref t3[i], ref t4[i], ref t5[i], ref t6[i], ref t7[i], ref t8[i], ref t9[i], ref t10[i], ref t11[i]);
		});
	}
}
public abstract class System<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> : ISystem
where T0 : unmanaged, IComponent
where T1 : unmanaged, IComponent
where T2 : unmanaged, IComponent
where T3 : unmanaged, IComponent
where T4 : unmanaged, IComponent
where T5 : unmanaged, IComponent
where T6 : unmanaged, IComponent
where T7 : unmanaged, IComponent
where T8 : unmanaged, IComponent
where T9 : unmanaged, IComponent
where T10 : unmanaged, IComponent
where T11 : unmanaged, IComponent
where T12 : unmanaged, IComponent
{
	public abstract Archetype Archetype { get; }
	protected abstract void Run(ref T0 t0, ref T1 t1, ref T2 t2, ref T3 t3, ref T4 t4, ref T5 t5, ref T6 t6, ref T7 t7, ref T8 t8, ref T9 t9, ref T10 t10, ref T11 t11, ref T12 t12);
	
	void ISystem.Run(ArchetypeBuffer buffer)
	{
		Parallel.ForEach(buffer.Chunks, ISystem.ParallelOptions, chunk =>
		{
			chunk.Compact();
			var t0 = chunk.GetComponentSpan<T0>();
			var t1 = chunk.GetComponentSpan<T1>();
			var t2 = chunk.GetComponentSpan<T2>();
			var t3 = chunk.GetComponentSpan<T3>();
			var t4 = chunk.GetComponentSpan<T4>();
			var t5 = chunk.GetComponentSpan<T5>();
			var t6 = chunk.GetComponentSpan<T6>();
			var t7 = chunk.GetComponentSpan<T7>();
			var t8 = chunk.GetComponentSpan<T8>();
			var t9 = chunk.GetComponentSpan<T9>();
			var t10 = chunk.GetComponentSpan<T10>();
			var t11 = chunk.GetComponentSpan<T11>();
			var t12 = chunk.GetComponentSpan<T12>();
			for(var i = 0; i < t0.Length; i++) Run(ref t0[i], ref t1[i], ref t2[i], ref t3[i], ref t4[i], ref t5[i], ref t6[i], ref t7[i], ref t8[i], ref t9[i], ref t10[i], ref t11[i], ref t12[i]);
		});
	}
}
public abstract class System<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> : ISystem
where T0 : unmanaged, IComponent
where T1 : unmanaged, IComponent
where T2 : unmanaged, IComponent
where T3 : unmanaged, IComponent
where T4 : unmanaged, IComponent
where T5 : unmanaged, IComponent
where T6 : unmanaged, IComponent
where T7 : unmanaged, IComponent
where T8 : unmanaged, IComponent
where T9 : unmanaged, IComponent
where T10 : unmanaged, IComponent
where T11 : unmanaged, IComponent
where T12 : unmanaged, IComponent
where T13 : unmanaged, IComponent
{
	public abstract Archetype Archetype { get; }
	protected abstract void Run(ref T0 t0, ref T1 t1, ref T2 t2, ref T3 t3, ref T4 t4, ref T5 t5, ref T6 t6, ref T7 t7, ref T8 t8, ref T9 t9, ref T10 t10, ref T11 t11, ref T12 t12, ref T13 t13);
	
	void ISystem.Run(ArchetypeBuffer buffer)
	{
		Parallel.ForEach(buffer.Chunks, ISystem.ParallelOptions, chunk =>
		{
			chunk.Compact();
			var t0 = chunk.GetComponentSpan<T0>();
			var t1 = chunk.GetComponentSpan<T1>();
			var t2 = chunk.GetComponentSpan<T2>();
			var t3 = chunk.GetComponentSpan<T3>();
			var t4 = chunk.GetComponentSpan<T4>();
			var t5 = chunk.GetComponentSpan<T5>();
			var t6 = chunk.GetComponentSpan<T6>();
			var t7 = chunk.GetComponentSpan<T7>();
			var t8 = chunk.GetComponentSpan<T8>();
			var t9 = chunk.GetComponentSpan<T9>();
			var t10 = chunk.GetComponentSpan<T10>();
			var t11 = chunk.GetComponentSpan<T11>();
			var t12 = chunk.GetComponentSpan<T12>();
			var t13 = chunk.GetComponentSpan<T13>();
			for(var i = 0; i < t0.Length; i++) Run(ref t0[i], ref t1[i], ref t2[i], ref t3[i], ref t4[i], ref t5[i], ref t6[i], ref t7[i], ref t8[i], ref t9[i], ref t10[i], ref t11[i], ref t12[i], ref t13[i]);
		});
	}
}
public abstract class System<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> : ISystem
where T0 : unmanaged, IComponent
where T1 : unmanaged, IComponent
where T2 : unmanaged, IComponent
where T3 : unmanaged, IComponent
where T4 : unmanaged, IComponent
where T5 : unmanaged, IComponent
where T6 : unmanaged, IComponent
where T7 : unmanaged, IComponent
where T8 : unmanaged, IComponent
where T9 : unmanaged, IComponent
where T10 : unmanaged, IComponent
where T11 : unmanaged, IComponent
where T12 : unmanaged, IComponent
where T13 : unmanaged, IComponent
where T14 : unmanaged, IComponent
{
	public abstract Archetype Archetype { get; }
	protected abstract void Run(ref T0 t0, ref T1 t1, ref T2 t2, ref T3 t3, ref T4 t4, ref T5 t5, ref T6 t6, ref T7 t7, ref T8 t8, ref T9 t9, ref T10 t10, ref T11 t11, ref T12 t12, ref T13 t13, ref T14 t14);
	
	void ISystem.Run(ArchetypeBuffer buffer)
	{
		Parallel.ForEach(buffer.Chunks, ISystem.ParallelOptions, chunk =>
		{
			chunk.Compact();
			var t0 = chunk.GetComponentSpan<T0>();
			var t1 = chunk.GetComponentSpan<T1>();
			var t2 = chunk.GetComponentSpan<T2>();
			var t3 = chunk.GetComponentSpan<T3>();
			var t4 = chunk.GetComponentSpan<T4>();
			var t5 = chunk.GetComponentSpan<T5>();
			var t6 = chunk.GetComponentSpan<T6>();
			var t7 = chunk.GetComponentSpan<T7>();
			var t8 = chunk.GetComponentSpan<T8>();
			var t9 = chunk.GetComponentSpan<T9>();
			var t10 = chunk.GetComponentSpan<T10>();
			var t11 = chunk.GetComponentSpan<T11>();
			var t12 = chunk.GetComponentSpan<T12>();
			var t13 = chunk.GetComponentSpan<T13>();
			var t14 = chunk.GetComponentSpan<T14>();
			for(var i = 0; i < t0.Length; i++) Run(ref t0[i], ref t1[i], ref t2[i], ref t3[i], ref t4[i], ref t5[i], ref t6[i], ref t7[i], ref t8[i], ref t9[i], ref t10[i], ref t11[i], ref t12[i], ref t13[i], ref t14[i]);
		});
	}
}
public abstract class System<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> : ISystem
where T0 : unmanaged, IComponent
where T1 : unmanaged, IComponent
where T2 : unmanaged, IComponent
where T3 : unmanaged, IComponent
where T4 : unmanaged, IComponent
where T5 : unmanaged, IComponent
where T6 : unmanaged, IComponent
where T7 : unmanaged, IComponent
where T8 : unmanaged, IComponent
where T9 : unmanaged, IComponent
where T10 : unmanaged, IComponent
where T11 : unmanaged, IComponent
where T12 : unmanaged, IComponent
where T13 : unmanaged, IComponent
where T14 : unmanaged, IComponent
where T15 : unmanaged, IComponent
{
	public abstract Archetype Archetype { get; }
	protected abstract void Run(ref T0 t0, ref T1 t1, ref T2 t2, ref T3 t3, ref T4 t4, ref T5 t5, ref T6 t6, ref T7 t7, ref T8 t8, ref T9 t9, ref T10 t10, ref T11 t11, ref T12 t12, ref T13 t13, ref T14 t14, ref T15 t15);
	
	void ISystem.Run(ArchetypeBuffer buffer)
	{
		Parallel.ForEach(buffer.Chunks, ISystem.ParallelOptions, chunk =>
		{
			chunk.Compact();
			var t0 = chunk.GetComponentSpan<T0>();
			var t1 = chunk.GetComponentSpan<T1>();
			var t2 = chunk.GetComponentSpan<T2>();
			var t3 = chunk.GetComponentSpan<T3>();
			var t4 = chunk.GetComponentSpan<T4>();
			var t5 = chunk.GetComponentSpan<T5>();
			var t6 = chunk.GetComponentSpan<T6>();
			var t7 = chunk.GetComponentSpan<T7>();
			var t8 = chunk.GetComponentSpan<T8>();
			var t9 = chunk.GetComponentSpan<T9>();
			var t10 = chunk.GetComponentSpan<T10>();
			var t11 = chunk.GetComponentSpan<T11>();
			var t12 = chunk.GetComponentSpan<T12>();
			var t13 = chunk.GetComponentSpan<T13>();
			var t14 = chunk.GetComponentSpan<T14>();
			var t15 = chunk.GetComponentSpan<T15>();
			for(var i = 0; i < t0.Length; i++) Run(ref t0[i], ref t1[i], ref t2[i], ref t3[i], ref t4[i], ref t5[i], ref t6[i], ref t7[i], ref t8[i], ref t9[i], ref t10[i], ref t11[i], ref t12[i], ref t13[i], ref t14[i], ref t15[i]);
		});
	}
}
