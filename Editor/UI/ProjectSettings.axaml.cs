using BladeEngine.Editor.UI.Models;
using BladeEngine.Editor.NuGet;
using Avalonia.Markup.Xaml;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia;

namespace BladeEngine.Editor.UI;

public class ProjectSettings : Window
{
	private readonly TextBox _packageSearch;
	private readonly StackPanel _packageList;

	public ProjectSettings()
	{
		AvaloniaXamlLoader.Load(this);
		_packageList = this.FindControl<StackPanel>("PackageList");
		_packageSearch = this.FindControl<TextBox>("PackageSearch");
	}

	public ProjectSettings(ProjectModel project) : this()
	{
		DataContext = project;
	}

	#region Nuget

	private async void InputElement_OnKeyUp(object? sender, KeyEventArgs e)
	{
		if(e.Key != Key.Return) return;
		
		_packageList.Children.Clear();
		_packageList.Children.Add(new TextBlock{Text = "Searching...", FontSize = 24, 
			TextAlignment = TextAlignment.Center, Margin = new Thickness(16)});
		
		var model = (ProjectModel) DataContext!;
		var results = await NuGet.NuGet.Search(_packageSearch.Text);

		_packageList.Children.Clear();
		foreach (var package in results)
			_packageList.Children.Add(new PackageEntry(model, package));
	}

	#endregion

	protected override bool HandleClosing()
	{
		var model = (ProjectModel) DataContext!;
		if (!model.Validate())
		{
			new WarningDialog("Some project settings are invalid. Please check your project.").ShowDialog(this);
			return true;
		}

		if (base.HandleClosing())
			return true;
		
		model.Project.Save();
		return false;
	}
	
}