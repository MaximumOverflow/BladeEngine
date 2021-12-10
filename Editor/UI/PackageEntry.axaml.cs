using BladeEngine.Editor.UI.Models;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Avalonia.Controls;
using NuGet.Versioning;

namespace BladeEngine.Editor.UI;

public class PackageEntry : UserControl
{
	private readonly Image _image;
	private readonly Button _button;
	private readonly TextBlock _title;
	private readonly ComboBox _versions;

	public PackageEntry()
	{
		AvaloniaXamlLoader.Load(this);
		_image = this.FindControl<Image>("Image");
		_button = this.FindControl<Button>("Button");
		_title = this.FindControl<TextBlock>("Title");
		_versions = this.FindControl<ComboBox>("Versions");
	}

	public PackageEntry(ProjectModel model, NugetPackageInfo package) : this()
	{
		DataContext = model;
		_title.Text = package.Title;
		_versions.Items = package.PackageVersions;
		_versions.SelectedIndex = 0;
		
		UpdateButtonText();
		NuGet.FetchIcon(package, img => Dispatcher.UIThread.InvokeAsync(() => _image.Source = img)).ConfigureAwait(false);
	}

	public void UpdateButtonText()
	{
		var project = ((ProjectModel) DataContext!).Project;
		if (project.Packages.TryGetValue(_title.Text, out var version))
		{
			var selectedVersion = _versions.SelectedItem!.ToString();
			_button.Content = selectedVersion == version?.ToString() ? "Remove" : "Update";
		}
		else _button.Content = "Install";
	}

	private void HandleButtonAction(object? sender, RoutedEventArgs e)
	{
		var project = ((ProjectModel) DataContext!).Project;
		if (project.Packages.TryGetValue(_title.Text, out var version))
		{
			var selectedVersion = _versions.SelectedItem!.ToString();
			if (selectedVersion == version?.ToString())
			{
				project.RemovePackage(_title.Text);
				_button.Content = "Install";
			}
			else
			{
				project.RemovePackage(_title.Text);
				project.AddPackage(_title.Text, new NuGetVersion(selectedVersion));
				_button.Content = "Remove";
			}
		}
		else
		{
			project.AddPackage(_title.Text, new NuGetVersion(_versions.SelectedItem!.ToString()));
			_button.Content = "Remove";
		}
	}

	private void OnVersionChanged(object? sender, SelectionChangedEventArgs e)
	{
		var project = ((ProjectModel) DataContext!).Project;
		if (!project.Packages.TryGetValue(_title.Text, out var version)) return;
		UpdateButtonText();
	}
}