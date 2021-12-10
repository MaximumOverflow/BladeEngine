using ReactiveUI;

namespace BladeEngine.Editor.UI.Models;

public class EditorModel : ReactiveObject
{
	private SceneModel? _sceneModel;
	private ProjectModel? _projectModel;

	public SceneModel? SceneModel
	{
		get => _sceneModel;
		set => this.RaiseAndSetIfChanged(ref _sceneModel, value);
	}
	
	public ProjectModel? ProjectModel
	{
		get => _projectModel;
		set => this.RaiseAndSetIfChanged(ref _projectModel, value);
	}
}