using System.Linq.Expressions;
using System.Reflection;
using System.Numerics;

namespace BladeEngine.Core.Rendering.Common;

public abstract class Material
{
	public abstract Shader Shader { get; }
	public abstract void Use();
}

public sealed class RuntimeMaterial : Material
{
	public override Shader Shader { get; }
	public readonly IReadOnlyDictionary<string, object> Parameters;
	
	private readonly Action _setParameters;

	public RuntimeMaterial(Shader shader, IReadOnlyDictionary<string, object> parameters)
	{
		Shader = shader;
		Parameters = parameters;

		// ReSharper disable once BadChildStatementIndent
		if (parameters.Count != shader.Parameters.Count)
		if (shader.Parameters.Keys.Any(p => !parameters.ContainsKey(p) && !p.StartsWith("_")))
			Debug.LogWarning("This material does not set some of its shader's parameters, things might not work as intended.");

		var calls = new List<Expression>(parameters.Count);
		var set = Shader.GetType().GetMethod(nameof(Shader.SetParameter), BindingFlags.Instance | BindingFlags.Public)!;
		
		var s = Expression.Constant(shader);
		foreach (var (key, value) in parameters)
		{
			var k = Expression.Constant(key);
			var v = Expression.Constant(value);
			var f = set.MakeGenericMethod(value.GetType());
			calls.Add(Expression.Call(s, f, k, v));
		}

		if (shader.Parameters.ContainsKey("_defaultProjection"))
		{
			var k = Expression.Constant("_defaultProjection");
			var v = Expression.Constant(Graphics.DefaultProjection);
			var f = set.MakeGenericMethod(typeof(Matrix4x4));
			calls.Add(Expression.Call(s, f, k, v));
		}
		
		_setParameters = Expression.Lambda<Action>(Expression.Block(calls)).Compile();
	}
	
	public override void Use()
	{
		Shader.Use();
		_setParameters();
	}
}