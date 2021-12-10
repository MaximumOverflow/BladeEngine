using Avalonia.Markup.Xaml;
using Avalonia.Controls;
using BladeEngine.Core;
using BladeEngine.Editor.UI.Models;

namespace BladeEngine.Editor.UI;

public class ProjectSettings : Window
{
	private readonly ComboBox _architecture;
	
	public ProjectSettings()
	{
		AvaloniaXamlLoader.Load(this);
		_architecture = this.FindControl<ComboBox>("Architecture");
	}

	public ProjectSettings(ProjectModel project)
	{
		DataContext = project;
		AvaloniaXamlLoader.Load(this);
		
		_architecture = this.FindControl<ComboBox>("Architecture");
		foreach (ComboBoxItem i in _architecture.Items)
		{
			if ((string) i.Content != project.Architecture) continue;
			_architecture.SelectedItem = i;
			break;
		}
	}

	private void SelectArchitecture(object? sender, SelectionChangedEventArgs e)
	{
		if(DataContext is null) return;
		var model = (ProjectModel) DataContext!;
		var arch = (string) ((ComboBoxItem) _architecture.SelectedItem!).Content;
		model.Architecture = arch!;
	}
}