using ReactiveUI;

namespace BladeEngine.Editor.UI.Models;

public sealed class ProjectModel : ReactiveObject
{
	public readonly Project Project;
	
	public ProjectModel(Project project)
	{
		Project = project;
	}

	public string Name
	{
		get => Project.Name;
		set { Project.Name = value; this.RaisePropertyChanged(nameof(Name)); }
	}
	
	public string Architecture
	{
		get => Project.Properties["Architecture"].Value;
		set
		{
			Project.Properties["Architecture"].Value = value;
			this.RaisePropertyChanged(nameof(Architecture));
		}
	}

	public static implicit operator Project(ProjectModel m) => m.Project;
}