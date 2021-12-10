using System.Diagnostics.CodeAnalysis;

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
	private FileInfo _file;
	public readonly DirectoryInfo Directory;
	private readonly ProjectRootElement _root;
	private readonly Dictionary<string, ProjectItemElement> _items;
	private readonly Dictionary<string, ProjectPropertyElement> _properties;

	#region Properties

	public string Name { get; set; }

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
		set => _items[nameof(EngineAssemblyPath)].Metadata.First().Value = value;
	}

	#endregion

	public Project(FileInfo file)
	{
		if (!file.Exists) throw new FileNotFoundException("Could not open project.", file.FullName);
		
		_file = file;
		Directory = file.Directory!;
		Name = file.Name[..file.Name.LastIndexOf('.')];
		_root = ProjectRootElement.Open(file.FullName)!;
		
		_properties = new Dictionary<string, ProjectPropertyElement>();
		foreach (var property in _root.Properties)
			if(!string.IsNullOrEmpty(property.Label))
				_properties.Add(property.Label, property);

		_items = new Dictionary<string, ProjectItemElement>();
		foreach (var item in _root.Items)
			if(!string.IsNullOrEmpty(item.Label))
				_items.Add(item.Label, item);

		if (EngineAssemblyPath != EnvironmentVariables.EngineAssemblyPath)
		{
			Debug.LogWarning($"Project '{Name}' was using an outdated engine assembly path. The path has been updated.");
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
		packages.Label = "Packages";
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
		return new Project(file);
	}

	public void Save()
	{
		if (Directory.Name != Name)
		{
			var prev = new DirectoryInfo(Directory.FullName);
			Directory.MoveTo(Path.Combine(Directory.Parent!.FullName, Name));
			if(prev.Exists) prev.Delete();
			_file.Delete();
		}

		var path = Path.Combine(Directory.FullName, Name + ".csproj");
		_root.Save(path);
		_file = new FileInfo(path);
	}

	public override string ToString()
	{
		var text = new StringWriter();
		_root.Save(text);
		return text.ToString();
	}
}