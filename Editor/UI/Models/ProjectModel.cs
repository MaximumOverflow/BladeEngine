using BladeEngine.Extensions;
using ReactiveUI;

namespace BladeEngine.Editor.UI.Models;

public sealed class ProjectModel : ReactiveObject
{
	public readonly Project Project;

	public ProjectModel(Project project)
	{
		Project = project;
	}
	
	public string AssemblyName
	{
		get => Project.AssemblyName;
		set { Project.AssemblyName = value; this.RaisePropertyChanged(nameof(AssemblyName)); }
	}
	
	public string RootNamespace
	{
		get => Project.RootNamespace;
		set { Project.RootNamespace = value; this.RaisePropertyChanged(nameof(RootNamespace)); }
	}

	public Architecture Architecture
	{
		get => Project.Architecture;
		set { Project.Architecture = value; this.RaisePropertyChanged(nameof(Architecture)); }
	}
	
	public OperatingSystem OperatingSystem
	{
		get => Project.OperatingSystem;
		set { Project.OperatingSystem = value; this.RaisePropertyChanged(nameof(OperatingSystem)); }
	}
	
	public BuildType BuildType
	{
		get => Project.BuildType;
		set { Project.BuildType = value; this.RaisePropertyChanged(nameof(BuildType)); }
	}

	public bool Validate()
	{
		if (string.IsNullOrWhiteSpace(AssemblyName) || string.IsNullOrWhiteSpace(RootNamespace))
			return false;
		
		if (!Project.NamespaceRegex.IsExactMatch(AssemblyName) || !Project.NamespaceRegex.IsExactMatch(RootNamespace))
			return false;

		return true;
	}
	
	public static implicit operator Project(ProjectModel m) => m.Project;
}