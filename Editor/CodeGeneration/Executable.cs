namespace BladeEngine.Editor.CodeGeneration;

public sealed class Executable
{
	private readonly Project _project;

	public Executable(Project project)
	{
		_project = project;
	}

	public async Task<int> Build(Action<string?>? consoleOutput = null)
	{
		return await Concurrency.ScheduleTask(async () =>
		{
			#region Setup Project

			consoleOutput?.Invoke("Generating build files...");
			var exeProj = ProjectRootElement.Create();
			exeProj.Sdk = "Microsoft.NET.Sdk";

			var general = exeProj.AddPropertyGroup();
			general.AddProperty("OutputType", "WinExe");
			general.AddProperty("TargetFramework", "net6.0");
			general.AddProperty("ImplicitUsings", "enable");
			general.AddProperty("Nullable", "enable");
			general.AddProperty("AssemblyName", _project.AssemblyName + "_build");
			general.AddProperty("RootNamespace", _project.RootNamespace);
			general.AddProperty("AllowUnsafeBlocks", "true");

			var build = exeProj.AddPropertyGroup();
			build.AddProperty("PlatformTarget", _project.Architecture.ToString());

			var silkVer = EnvironmentVariables.SilkVersion.ToString();
			var include = exeProj.AddItemGroup();
			include.Label = "Packages";
			include.AddItem("ProjectReference", $"../{_project.AssemblyName}.csproj");
			include.AddItem("PackageReference", "Silk.NET.Core").AddMetadata("Version", silkVer);
			include.AddItem("PackageReference", "Silk.NET.GLFW").AddMetadata("Version", silkVer);
			include.AddItem("PackageReference", "Silk.NET.Windowing").AddMetadata("Version", silkVer);
			var core = include.AddItem("Reference", "BladeEngine.Core");
			core.AddMetadata("HintPath", $"{EnvironmentVariables.EngineAssemblyPath}");

			var tmpDir = new DirectoryInfo(Path.Combine(_project.File.Directory!.FullName, ".build"));
			tmpDir.Create();
			tmpDir.Attributes |= FileAttributes.Hidden;
			exeProj.Save(Path.Combine(tmpDir.FullName, $"{_project.AssemblyName}.csproj"));

			#endregion

			#region Generate Files

			await File.WriteAllTextAsync(Path.Combine(tmpDir.FullName, "Program.cs"), GenerateProgram());

			#endregion

			consoleOutput?.Invoke("Starting MSBuild...");
			var buildPath = Path.Combine(_project.File.Directory!.FullName, "Build");
			var dotnet = Process.Start(new ProcessStartInfo("dotnet", $"build -o {buildPath}")
			{
				WorkingDirectory = tmpDir.FullName, UseShellExecute = false, CreateNoWindow = true,
				RedirectStandardOutput = true, RedirectStandardInput = true, RedirectStandardError = true,
			});

			if (dotnet is null) return -1;
			if (consoleOutput is not null)
			{
				dotnet.BeginOutputReadLine();
				dotnet.OutputDataReceived += (_, args) => consoleOutput.Invoke(args.Data);
			}

			await dotnet.WaitForExitAsync();
			tmpDir.Delete(true);

			if(Environment.OSVersion.Platform == PlatformID.Win32NT)
				Process.Start("explorer.exe", buildPath);
			
			consoleOutput?.Invoke($"Generated executable at '{Path.Combine(buildPath, $"{_project.AssemblyName}_build.exe")}'");
			return dotnet.ExitCode;
		}, "Build project");
	}

	private string GenerateProgram()
	{
		return @"
using BladeEngine.Core.Rendering.Common;

Graphics.Initialize(new GraphicsSettings());
while (Graphics.IsRendering)
{
	Graphics.HandleWindowEvents();
	Graphics.SwapBuffers();
}
Graphics.Terminate();
";
	}
}