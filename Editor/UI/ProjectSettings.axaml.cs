using BladeEngine.Editor.UI.Models;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia;

namespace BladeEngine.Editor.UI;

public class ProjectSettings : Window
{
	private readonly CheckBox _installed;
	private readonly TextBox _packageSearch;
	private readonly StackPanel _packageList;

	public ProjectSettings()
	{
		AvaloniaXamlLoader.Load(this);
		_installed = this.FindControl<CheckBox>("Installed");
		_packageList = this.FindControl<StackPanel>("PackageList");
		_packageSearch = this.FindControl<TextBox>("PackageSearch");
	}

	public ProjectSettings(ProjectModel project) : this()
	{
		DataContext = project;
		SearchPackage(null, new KeyEventArgs{Key = Key.Enter});
	}

	#region Nuget

	private void ToggleInstalledPackages(object? sender, RoutedEventArgs e)
	{
		if (string.IsNullOrWhiteSpace(_packageSearch.Text) && _installed.IsChecked!.Value)
			ShowAllInstalledPackages();
		else 
			SearchPackage(null, new KeyEventArgs {Key = Key.Return});
	}

	private void ShowAllInstalledPackages()
	{
		_packageList.Children.Clear();
		var model = (ProjectModel) DataContext!;
		foreach (var package in model.Project.Packages.Keys)
		{
			NuGet.FetchInfo(package, info =>
			{
				if(!info.HasValue) return;
				Dispatcher.UIThread.InvokeAsync(() => _packageList.Children.Add(new PackageEntry(model, info.Value)));
			}).ConfigureAwait(false);
		}
	}
	
	private async void SearchPackage(object? sender, KeyEventArgs e)
	{
		if(e.Key != Key.Return) return;

		_packageList.Children.Clear();
		_packageList.Children.Add(new TextBlock{Text = "Searching...", FontSize = 24, 
			TextAlignment = TextAlignment.Center, Margin = new Thickness(16)});
		
		var model = (ProjectModel) DataContext!;
		var results = (await NuGet.Search(_packageSearch.Text)).ToList();
		if (_installed.IsChecked!.Value) results.RemoveAll(p => !model.Project.Packages.ContainsKey(p.Title));

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
		
		async Task Save()
		{
			await model.Project.Save();
		}

		Save().ConfigureAwait(false);
		return false;
	}
}