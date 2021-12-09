using Silk.NET.OpenGL;

namespace BladeEngine.Core.Rendering.OpenGL;

internal sealed class Shader : Common.Shader
{
	public readonly uint Id;
	public override string Source { get; }
	private readonly IReadOnlyDictionary<string, int> _uniforms;
	public override IReadOnlyDictionary<string, (int, DataType)> Parameters { get; }
	private static readonly Regex Split = new("#( |\t)*pragma( |\t)+source( |\t)+", RegexOptions.Compiled);

	private Shader(uint id, string source)
	{
		Id = id;
		Source = source;
		
		//Get all uniforms
		var gl = OpenGL.Api;
		gl.GetProgram(id, GLEnum.ActiveUniforms, out var uniformCount);
		var uniforms = new Dictionary<string, int>();
		var parameters = new Dictionary<string, (int, DataType)>(uniformCount);
		for (var i = 0u; i < uniformCount; i++)
		{
			var name = gl.GetActiveUniform(id, i, out var size, out var type);
			parameters.Add(name, (size, DataTypes.Get(type)));
			uniforms.Add(name, (int) i);
		}
		_uniforms = uniforms;
		Parameters = parameters;
	}

	public override void Use()
	{
		OpenGL.Api.UseProgram(Id);
	}

	public override unsafe bool SetParameter<T>(string name, T value)
	{
		if (!Parameters.TryGetValue(name, out var res))
			return false;

		var gl = OpenGL.Api;
		var (count, type) = res;
		if (count != 1)
		{
			Debug.LogError($"Could not set shader parameter '{name}', expected {type}[], got {type}.");
			return false;
		}

		switch (type)
		{
			case DataType.Float when DataTypes.Get<T>() == DataType.Float:
				gl.ProgramUniform1(Id, _uniforms[name], Unsafe.As<T, float>(ref value));
				return true;

			case DataType.Float2 when DataTypes.Get<T>() == DataType.Float2:
				gl.ProgramUniform2(Id, _uniforms[name], Unsafe.As<T, Vector2>(ref value));
				return true;
			
			case DataType.Float3 when DataTypes.Get<T>() == DataType.Float3:
				gl.ProgramUniform3(Id, _uniforms[name], Unsafe.As<T, Vector3>(ref value));
				return true;
			
			case DataType.Float4 when DataTypes.Get<T>() == DataType.Float4:
				gl.ProgramUniform4(Id, _uniforms[name], Unsafe.As<T, Vector4>(ref value));
				return true;

			case DataType.FloatMat4:
				gl.ProgramUniformMatrix4(Id, _uniforms[name], 1, false, (float*) Unsafe.AsPointer(ref value));
				return true;

			default:
			{
				var t = DataTypes.Get<T>();
				Debug.LogError(t != DataType.Unknown
					? $"Could not set shader parameter '{name}', expected {type}, got {t}."
					: $"Could not set shader parameter '{name}', expected {type}, got {typeof(T).Name}.");
				return false;
			}
		}
	}

	public static Shader? Create(string source)
	{
		var gl = OpenGL.Api;
		var program = gl.CreateProgram();
		var parts = Split.Split(source).Where(p =>!string.IsNullOrEmpty(p));
		var shaders = new List<uint>();
		
		foreach (var part in parts)
		{
			if(string.IsNullOrWhiteSpace(part)) continue;
			var kindId = part.Substring(0, part.IndexOf('\n'));
			if (!Enum.TryParse(kindId, true, out ShaderType kind))
			{
				Debug.LogError($"Could not compile shader, '{kindId}' is not a valid source type.");
				return null;
			}

			var shader = gl.CreateShader(kind);
			gl.ShaderSource(shader, part.Substring(kindId.Length));
			gl.CompileShader(shader);
			var sErr = gl.GetShaderInfoLog(shader);
			if (!string.IsNullOrEmpty(sErr))
			{
				Debug.LogError("Could not compile shader:\n{{0}}", sErr);
				return null;
			}
			
			gl.AttachShader(program, shader);
			shaders.Add(shader);
		}
		
		gl.LinkProgram(program);

		shaders.ForEach(s => gl.DeleteShader(s));
		return new Shader(program, source);
	}
}