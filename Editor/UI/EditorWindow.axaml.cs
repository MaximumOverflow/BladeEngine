using Window = Avalonia.Controls.Window;
using BladeEngine.Editor.UI.Models;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Controls;
using Avalonia;
using BladeEngine.Editor.UI.Rendering;

namespace BladeEngine.Editor.UI;

public sealed class EditorWindow : Window
{
	private readonly RenderControl _render;
	
	public EditorWindow()
	{
		// OpenGL.Initialize(new WindowOptions{Size = new Vector2D<int>(800, 600), Title = "GL", IsVisible = false});
		DataContext = new EditorModel();
		AvaloniaXamlLoader.Load(this);

		_render = this.FindControl<RenderControl>("RenderControl");

		#if DEBUG
		this.AttachDevTools();
		#endif
	}
	
	private async void NewProject(object? sender, RoutedEventArgs e)
	{
		//TODO Ask if user wants to save the current project
		var project = await new NewProjectDialog().Open(this);
		if(project is null) return;
		
		((EditorModel) DataContext!).ProjectModel = new ProjectModel(project);
		Title = $"{project.Name} - Blade - {Graphics.CurrentApi}";
	}

	private async void OpenProject(object? sender, RoutedEventArgs e)
	{
		var path = await new OpenFileDialog{AllowMultiple = false, Filters = new()
		{
			new FileDialogFilter{Extensions = new(){"csproj"}, Name = "Blade Project File"}
		}}.ShowAsync(this);
		if(path?.Length != 1) return;

		var project = new Project(new FileInfo(path[0]));
		((EditorModel) DataContext!).ProjectModel = new ProjectModel(project);
		Title = $"{project.Name} - Blade - {Graphics.CurrentApi}";
		Debug.Log("Opened project '{0}'", path);
	}
	
	private void SaveProject(object? sender, RoutedEventArgs? e)
	{
		var project = ((EditorModel) DataContext!).ProjectModel!.Project;
		project.Save();
		Debug.Log("Project saved");
	}

	private void CloseProject(object? sender, RoutedEventArgs? e)
	{
		//TODO Ask if user wants to save the current project
		((EditorModel) DataContext!).ProjectModel = null;
		Debug.Log("Project closed");
	}
	
	private async void OpenProjectSettings(object? sender, RoutedEventArgs e)
    {
    	var settings = new ProjectSettings(((EditorModel) DataContext!).ProjectModel!);
    	await settings.ShowDialog(this);
    }

	private void Exit(object? sender, RoutedEventArgs e)
	{
		CloseProject(null, null);
		Close();
	}

	private async void Build(object? sender, RoutedEventArgs e)
	{
		Debug.Log("Building project...");
		var project = ((EditorModel) DataContext!).ProjectModel!.Project;
		var result = await new BuildProcessWindow().Open(this, project);
		if(result != 0) Debug.LogError($"Project failed to build. Exit code: {result}");
		else Debug.Log("Project built successfully!");
	}
	
	private async void BuildAndRun(object? sender, RoutedEventArgs e)
	{
		
	}
}