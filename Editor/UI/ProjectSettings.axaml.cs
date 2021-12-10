using Avalonia;
using BladeEngine.Editor.UI.Models;
using NuGet.Protocol.Core.Types;
using Avalonia.Markup.Xaml;
using NuGet.Configuration;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using BladeEngine.Editor.NuGet;
using NuGet.Common;

namespace BladeEngine.Editor.UI;

public class ProjectSettings : Window
{
	private readonly ComboBox _architecture;
	private readonly StackPanel _packageList;
	private readonly TextBox _packageSearch;
	
	public ProjectSettings()
	{
		AvaloniaXamlLoader.Load(this);
		_architecture = this.FindControl<ComboBox>("Architecture");
		_packageList = this.FindControl<StackPanel>("PackageList");
		_packageSearch = this.FindControl<TextBox>("PackageSearch");
	}

	public ProjectSettings(ProjectModel project) : this()
	{
		DataContext = project;
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

	#region Nuget

	
	
	private async void InputElement_OnKeyUp(object? sender, KeyEventArgs e)
	{
		if(e.Key != Key.Return) return;
		
		_packageList.Children.Clear();
		_packageList.Children.Add(new TextBlock{Text = "Searching...", FontSize = 24, 
			TextAlignment = TextAlignment.Center, Margin = new Thickness(16)});
		
		var model = (ProjectModel) DataContext!;
		var results = await Nuget.Search(_packageSearch.Text);

		_packageList.Children.Clear();
		foreach (var package in results)
			_packageList.Children.Add(await PackageEntry.Create(model, package));
	}

	#endregion
}