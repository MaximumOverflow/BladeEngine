using System.Reflection;

namespace BladeEngine.Editor;

internal static class EnvironmentVariables
{
	public static readonly Version SilkVersion = Assembly.Load("Silk.NET.Core").GetName().Version ?? new Version();
	public static readonly string EngineAssemblyPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BladeEngine.Core.dll"));
}