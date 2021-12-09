using System.CodeDom.Compiler;

namespace BladeEngine.Editor.CodeGeneration;

public readonly record struct CompiledMaterial(uint Id)
{
	public override string ToString() => $"Material{Id}";
	public static implicit operator string(CompiledMaterial m) => m.ToString();
}

public sealed class MaterialDefinitionFile
{
	private uint _materials;
	private readonly StringWriter _text;
	private readonly IndentedTextWriter _src;
	private readonly Dictionary<Shader, string> _shaders;

	public MaterialDefinitionFile()
	{
		_materials = 0;
		_text = new StringWriter();
		_src = new IndentedTextWriter(_text);
		_shaders = new Dictionary<Shader, string>();
		_src.WriteLine("using BladeEngine.Core.Rendering.Common;");
		_src.WriteLine("using System.Numerics;");
		_src.WriteLine();
	}

	public CompiledMaterial Add(Shader shader, IReadOnlyDictionary<string, object> parameters)
	{
		var baseType = GetShaderBase(shader);
		_src.WriteLine($"public sealed class Material{_materials++} : {baseType}");
		_src.WriteLine('{'); _src.Indent++;
		_src.WriteLine("public override void Use()");
		_src.WriteLine('{'); _src.Indent++;
		_src.WriteLine("Shader.Use();");
		foreach (var (key, value) in parameters)
		{
			var val = value switch
			{
				byte v => v.ToString(),
				sbyte v => v.ToString(),
				short v => v.ToString(),
				ushort v => v.ToString(),
				int v => v.ToString(),
				uint v => v.ToString(),
				long v => v.ToString(),
				ulong v => v.ToString(),
				Vector2 v => $"new Vector2({v.X}, {v.Y})",
				Vector3 v => $"new Vector3({v.X}, {v.Y}, {v.Z})",
				Vector4 v => $"new Vector4({v.X}, {v.Y}, {v.Z}, {v.W})",
				_ => throw new NotImplementedException(),
			};
			
			_src.WriteLine($"Shader.SetParameter(\"{key}\", {val});");
		}
		_src.Indent--; _src.WriteLine('}');
		_src.Indent--; _src.WriteLine('}');
		return default;
	}

	private string GetShaderBase(Shader shader)
	{
		if (_shaders.TryGetValue(shader, out var name))
			return name;

		name = $"Shader{_shaders.Count}";
		_shaders.Add(shader, name);
		
		_src.WriteLine($"public abstract class {name} : Material");
		_src.WriteLine('{'); _src.Indent++;
		_src.WriteLine($"public const string Source = @\"{shader.Source}\";");
		_src.WriteLine();
		_src.WriteLine("private static Shader? _shaderInstance;");
		_src.WriteLine("public override Shader Shader");
		_src.WriteLine('{'); _src.Indent++;
		_src.WriteLine("get");
		_src.WriteLine('{'); _src.Indent++;
		_src.WriteLine("_shaderInstance ??= Shader.FromSource(Source);");
		_src.WriteLine("return _shaderInstance!;");
		_src.Indent--; _src.WriteLine('}');
		_src.Indent--; _src.WriteLine('}');
		_src.Indent--; _src.WriteLine('}');
		_src.WriteLine();
		return name;
	}

	public override string ToString() => _text.ToString();
}