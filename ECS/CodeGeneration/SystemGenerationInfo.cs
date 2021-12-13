using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BladeEngine.ECS.CodeGeneration;

public sealed class SystemGenerationInfo
{
	public string FullName;
	public string[] Components;
	public BlockSyntax? RunScope;
	public string[] ParameterNames;
	public string[] Usings;
	
	public override string ToString()
	{
		return $"{{ FullName = {FullName}, Components = [{string.Join(", ", Components)}] }}";
	}
}