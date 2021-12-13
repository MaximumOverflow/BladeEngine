using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using System.CodeDom.Compiler;
using Microsoft.CodeAnalysis;
using System.Text;

namespace BladeEngine.ECS.CodeGeneration;

[Generator]
internal class SystemGenerator : ISourceGenerator
{
	public void Initialize(GeneratorInitializationContext context)
	{
		context.RegisterForSyntaxNotifications(() => new SystemSyntaxReceiver());
	}

	public void Execute(GeneratorExecutionContext context)
	{
		if(context.SyntaxContextReceiver is not SystemSyntaxReceiver receiver)
			return;

		var text = new StringWriter();
		var src = new IndentedTextWriter(text);

		foreach (var error in receiver.Errors)
			src.WriteLine($"#error {error}");

		foreach (var info in receiver.Info)
		{
			var ns = info.FullName[..info.FullName.LastIndexOf('.')];
			var name = info.FullName[(info.FullName.LastIndexOf('.') + 1)..];
			
			src.WriteLine($"namespace {ns}");
			src.WriteLine('{'); src.Indent++;
			
			foreach (var import in info.Usings)
				src.WriteLine(import.Trim());

			src.WriteLine();
			src.WriteLine($"public partial class {name}");
			src.WriteLine('{'); src.Indent++;
			src.WriteLine("protected override Action<ArchetypeBuffer> ExecutionDelegate => _GeneratedRun;\n");
			
			src.WriteLine("private void _GeneratedRun(ArchetypeBuffer _gen_buffer)");
			src.WriteLine('{'); src.Indent++;
			src.WriteLine("Parallel.ForEach(_gen_buffer.Chunks, ISystem.ParallelOptions, (_gen_chunk) =>");
			src.WriteLine('{'); src.Indent++;
			
			src.WriteLine("_gen_chunk.Compact();");
			for (var i = 0; i < info.Components.Length; i++)
			{
				var component = info.Components[i];
				src.WriteLine($"var _gen_t{i} = _gen_chunk.GetComponentSpan<{component}>();");
			}
			
			src.WriteLine();
			src.WriteLine("for(var _gen_i = 0; _gen_i < _gen_t0.Length; _gen_i++)");
			src.WriteLine('{'); src.Indent++;
			
			for (var p = 0; p < info.ParameterNames.Length; p++)
				src.WriteLine($"ref var {info.ParameterNames[p]} = ref _gen_t{p}[_gen_i];");
			
			src.WriteLine();
			src.WriteLine("//Inlined code. This will not look pretty.");
			src.WriteLine(info.RunScope?.GetText().ToString());
			src.Indent--; src.WriteLine('}');

			src.Indent--; src.WriteLine("});");
			src.Indent--; src.WriteLine('}');
			
			src.Indent--; src.WriteLine('}');
			
			src.Indent--; src.WriteLine('}');
			src.WriteLine();
		}
		
		context.AddSource("GeneratedSystems.cs", text.ToString());

		#if DEBUG
		var debugText = new StringWriter();
		var debugSrc = new IndentedTextWriter(debugText);
		debugSrc.WriteLine($"namespace {context.Compilation.Assembly.Identity.Name};");
		debugSrc.WriteLine("public static class _SystemGenDebug");
		debugSrc.WriteLine("{"); debugSrc.Indent++;
		debugSrc.WriteLine($"public static void PrintSource() => Console.WriteLine(@\"{text}\");");
		debugSrc.Indent--; debugSrc.WriteLine("}");
		
		context.AddSource("Debug.cs", debugText.ToString());
		#endif
	}
}

internal class SystemSyntaxReceiver : ISyntaxContextReceiver
{
	private readonly List<string> _errors = new();
	private readonly List<SystemGenerationInfo> _info = new();

	public IEnumerable<string> Errors => _errors;
	public IEnumerable<SystemGenerationInfo> Info => _info;


	public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
	{
		if(context.Node is not ClassDeclarationSyntax {BaseList: not null} declaration) return;
		var model = context.SemanticModel;

		if (declaration.Parent is ClassDeclarationSyntax or StructDeclarationSyntax)
		{
			_errors.Add($"Class '{GetFullName(declaration)}' is a system. Systems cannot be nested classes.");
			return;
		}

		if (!declaration.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
		{
			_errors.Add($"Class '{GetFullName(declaration)}' is a system. Systems must be declared as partial.");
			return;
		}
		
		if (!declaration.Modifiers.Any(m => m.IsKind(SyntaxKind.SealedKeyword)))
		{
			_errors.Add($"Class '{GetFullName(declaration)}' is a system. Systems must be declared as sealed.");
			return;
		}

		var baseClass = declaration.BaseList.Types[0].Type;
		var baseType = ModelExtensions.GetTypeInfo(model, baseClass);
		
		if (baseType.Type is not {ContainingNamespace: {Name: "ECS", ContainingNamespace.Name: "BladeEngine"}}) 
			return;
		
		if(baseClass.ChildNodes().First() is not TypeArgumentListSyntax genericTypes)
			return;

		var info = new SystemGenerationInfo
		{
			FullName = GetFullName(declaration),
			Components = new string[genericTypes.Arguments.Count]
		};

		for (var i = 0; i < info.Components.Length; i++)
		{
			var type = GetFullName(model.GetSymbolInfo(genericTypes.Arguments[i]).Symbol);
			info.Components[i] = type;
		}
		
		foreach (var member in declaration.Members)
		{
			if(member is not MethodDeclarationSyntax{Identifier.Text: "Run" } method) continue;
			var param = method.ParameterList.Parameters;

			//TODO Add type checking
			if(param.Count != info.Components.Length) continue;

			info.RunScope = method.Body;
			info.ParameterNames = param.Select(p => p.Identifier.Text).ToArray();
			break;
		}

		SyntaxNode file = declaration;
		while (file is not CompilationUnitSyntax) file = file.Parent;
		var unit = (CompilationUnitSyntax) file;
		info.Usings = unit.Usings.Select(u => u.GetText().ToString()).ToArray();
		
		_info.Add(info);
	}

	private static string GetFullName(ISymbol? symbol) => (symbol switch
	{
		null => String.Empty,
		{ContainingSymbol: IModuleSymbol} => symbol.Name,
		{ContainingSymbol: not null} => $"{GetFullName(symbol.ContainingSymbol)}.{symbol.Name}",
		_ => symbol.Name
	}).TrimStart('.');

	public string GetFullName(ClassDeclarationSyntax source)
	{
		try
		{
			var items = new List<string>();
			var parent = source.Parent;
			while (parent.IsKind(SyntaxKind.ClassDeclaration))
			{
				var parentClass = (ClassDeclarationSyntax) parent;
				items.Add(parentClass.Identifier.Text);
	
				parent = parent.Parent;
			}
	
			switch (parent)
			{
				case NamespaceDeclarationSyntax nameSpace:
				{
					var sb = new StringBuilder().Append(nameSpace.Name).Append('.');
					items.Reverse();
					items.ForEach(i => { sb.Append(i).Append('.'); });
					sb.Append(source.Identifier.Text);
					return sb.ToString();
				}
				
				case FileScopedNamespaceDeclarationSyntax nameSpace:
				{
					var sb = new StringBuilder().Append(nameSpace.Name).Append('.');
					items.Reverse();
					items.ForEach(i => { sb.Append(i).Append('.'); });
					sb.Append(source.Identifier.Text);
					return sb.ToString();
				}
				
				default: 
					_errors.Add($"{parent?.GetType().FullName ?? "null"} is not a supported parent for {nameof(GetFullName)}.");
					return "";
			}
		}
		catch (Exception e)
		{
			return e.ToString();
		}
	}
}