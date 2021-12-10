using BladeEngine.Editor.CodeGeneration;
using BladeEngine.Editor.UI.Rendering;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Controls;
using Avalonia;
using BladeEngine.Editor.UI.Models;
using Window = Avalonia.Controls.Window;

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
		
	}

	private async void OpenProject(object? sender, RoutedEventArgs e)
	{
		
	}
	
	private void SaveProject(object? sender, RoutedEventArgs? e)
	{
		
	}

	private void CloseProject(object? sender, RoutedEventArgs? e)
	{
		
	}
	
	private async void OpenProjectSettings(object? sender, RoutedEventArgs e)
    {
    	
    }

	private void Exit(object? sender, RoutedEventArgs e)
	{
		CloseProject(null, null);
		Close();
	}

	private async void Build(object? sender, RoutedEventArgs e)
	{
		
	}
	
	private async void BuildAndRun(object? sender, RoutedEventArgs e)
	{
		
	}
}