using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using NuGet.Versioning;

namespace BladeEngine.Editor.CodeGeneration;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public enum Architecture
{
	AnyCPU, X86, X64,
}

[SuppressMessage("ReSharper", "InconsistentNaming")]
public enum OperatingSystem
{
	Windows, Linux, MacOS
}

[SuppressMessage("ReSharper", "InconsistentNaming")]
public enum BuildType
{
	Debug, Release,
}

public class Project
{
	private readonly object _saveLock = new();
	internal static readonly Regex NameRegex = new("[a-zA-Z][a-zA-Z_0-9]*", RegexOptions.Compiled);
	internal static readonly Regex NamespaceRegex = new("[a-zA-Z][a-zA-Z_0-9]*(\\.[a-zA-Z][a-zA-Z_0-9]*)*", RegexOptions.Compiled);

	private bool _packagesEdited;
	public readonly FileInfo File;
	private readonly ProjectRootElement _root;
	private readonly ProjectItemGroupElement _includes;
	private readonly Dictionary<string, NuGetVersion?> _packages;
	private readonly Dictionary<string, ProjectItemElement> _items;
	private readonly Dictionary<string, ProjectPropertyElement> _properties;

	#region Creation

	public Project(FileInfo file)
	{
		if (!file.Exists) throw new FileNotFoundException("Could not open project.", file.FullName);
		
		File = file;
		_root = ProjectRootElement.Open(file.FullName)!;
		
		_properties = new Dictionary<string, ProjectPropertyElement>();
		foreach (var property in _root.Properties)
			if(!string.IsNullOrEmpty(property.Label))
				_properties.Add(property.Label, property);

		_items = new Dictionary<string, ProjectItemElement>();
		foreach (var item in _root.Items)
			if(!string.IsNullOrEmpty(item.Label))
				_items.Add(item.Label, item);
		
		var includes = _root.ItemGroups.FirstOrDefault(g => g.Label == nameof(Packages));
		if (includes is null)
		{
			includes = _root.AddItemGroup();
			includes.Label = nameof(Packages);
		}
		_includes = includes;
		
		_packages = new Dictionary<string, NuGetVersion?>(StringComparer.InvariantCultureIgnoreCase);
		// ReSharper disable once PossibleInvalidCastExceptionInForeachLoop
		foreach (ProjectItemElement reference in includes.Children)
		{
			var v = reference.Metadata.FirstOrDefault(m => m.Name == "Version")?.Value;
			var version = v is null ? null : new NuGetVersion(v);
			_packages.Add(reference.Include, version);
		}

		if (EngineAssemblyPath != EnvironmentVariables.EngineAssemblyPath)
		{
			Debug.LogWarning($"Project was using an outdated engine assembly path. The path has been updated.");
			EngineAssemblyPath = EnvironmentVariables.EngineAssemblyPath;
			Save();
		}
	}
	
	public static Project Create(string directory, string name, string? assembly = null, string? @namespace = null)
	{
		var dir = new DirectoryInfo(directory);
		if(!dir.Exists) throw new DirectoryNotFoundException($"'{directory}' does not exist.");

		var file = new FileInfo(Path.Combine(directory, name, name + ".csproj"));
		var proj = ProjectRootElement.Create(file.FullName);
		proj.Sdk = "Microsoft.NET.Sdk";

		var general = proj.AddPropertyGroup();
		general.AddProperty("TargetFramework", "net6.0");
		general.AddProperty("ImplicitUsings", "enable");
		general.AddProperty("Nullable", "enable");
		general.AddProperty("AssemblyName", assembly ?? name).Label = nameof(AssemblyName);
		general.AddProperty("RootNamespace", @namespace ?? name).Label = nameof(RootNamespace);
		general.AddProperty("AllowUnsafeBlocks", "true");

		var build = proj.AddPropertyGroup();
		build.Label = "BuildSettings";
		build.AddProperty("BladeSetting", BuildType.Debug.ToString()).Label = nameof(BuildType);
		build.AddProperty("BladeSetting", OperatingSystem.Windows.ToString()).Label = nameof(OperatingSystem);
		build.AddProperty("PlatformTarget", Architecture.X64.ToString()).Label = nameof(Architecture);

		var packages = proj.AddItemGroup();
		packages.Label = nameof(Packages);
		var core = packages.AddItem("Reference", "BladeEngine.Core");
		core.Label = nameof(EngineAssemblyPath);
		core.AddMetadata("HintPath", $"{EnvironmentVariables.EngineAssemblyPath}");

		var createDirectories = new[] {"Assets", "Scenes", "Build"};
		var ignoreDirectories = new[] {"Build"};
		
		dir = file.Directory!;
		foreach (var d in createDirectories) dir.CreateSubdirectory(d);
		
		var ignore = proj.AddItemGroup(); ignore.Label = "Ignore";
		foreach (var d in ignoreDirectories)
		{
			dir.CreateSubdirectory(d);
			var i = ignore.AddItem("None", "placeholder"); i.Include = ""; i.Remove = $"{d}/**";
				i = ignore.AddItem("Compile", "placeholder"); i.Include = ""; i.Remove = $"{d}/**";
				i = ignore.AddItem("EmbeddedResource", "placeholder"); i.Include = ""; i.Remove = $"{d}/**";
		}

		proj.Save();
		return new Project(file){_packagesEdited = true};
	}

	#endregion

	#region Properties

	public bool IsSaving => Monitor.IsEntered(_saveLock);
	
	public IReadOnlyDictionary<string, NuGetVersion?> Packages => _packages;
	public IReadOnlyDictionary<string, ProjectItemElement> Items => _items;
	public IReadOnlyDictionary<string, ProjectPropertyElement> Properties => _properties;

	public string AssemblyName
	{
		get => _properties[nameof(AssemblyName)].Value;
		set => _properties[nameof(AssemblyName)].Value = value;
	}
	
	public string RootNamespace
	{
		get => _properties[nameof(RootNamespace)].Value;
		set => _properties[nameof(RootNamespace)].Value = value;
	}

	public Architecture Architecture
	{
		get => Enum.Parse<Architecture>(_properties[nameof(Architecture)].Value);
		set => _properties[nameof(Architecture)].Value = value.ToString();
	}
	
	public OperatingSystem OperatingSystem
	{
		get => Enum.Parse<OperatingSystem>(_properties[nameof(OperatingSystem)].Value);
		set => _properties[nameof(OperatingSystem)].Value = value.ToString();
	}
	
	public BuildType BuildType
	{
		get => Enum.Parse<BuildType>(_properties[nameof(BuildType)].Value);
		set => _properties[nameof(BuildType)].Value = value.ToString();
	}

	public string EngineAssemblyPath
	{
		get => _items[nameof(EngineAssemblyPath)].Metadata.First().Value;
		init => _items[nameof(EngineAssemblyPath)].Metadata.First().Value = value;
	}

	#endregion

	public bool AddPackage(string package, NuGetVersion version)
	{
		if (!_packages.TryAdd(package, version)) return false;
		_packagesEdited = true;
		_includes.AddItem("Reference", package).AddMetadata("Version", version.ToString(), true);
		return true;
	}

	public bool RemovePackage(string package)
	{
		if (!_packages.Remove(package)) return false;
		_packagesEdited = true;
		_includes.RemoveChild(_includes.Items.First(p => string.Compare(p.Include, package, true, CultureInfo.InvariantCulture) == 0));
		return true;
	}

	public Task Save()
	{
		return Concurrency.ScheduleTask(async () =>
		{
			Monitor.Enter(_saveLock);
			var path = Path.Combine(File.FullName);
			_root.Save(path);

			if (_packagesEdited)
			{
				await Process.Start(new ProcessStartInfo("dotnet", "restore")
				{
					WorkingDirectory = File.Directory!.FullName,
					CreateNoWindow = false, UseShellExecute = false,
				})!.WaitForExitAsync();
				_packagesEdited = false;
			}
			Monitor.Exit(_saveLock);
		}, "Save project", onAbort: _ => Monitor.Exit(_saveLock));
	}

	public override string ToString()
	{
		var text = new StringWriter();
		_root.Save(text);
		return text.ToString();
	}
}