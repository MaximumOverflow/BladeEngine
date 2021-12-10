using BladeEngine.Editor.UI.Models;
using BladeEngine.Editor.NuGet;
using Avalonia.Media.Imaging;
using Avalonia.Markup.Xaml;
using Avalonia.Controls;

namespace BladeEngine.Editor.UI;

public class PackageEntry : UserControl
{
	private readonly Image _image;
	private readonly TextBlock _title;
	private readonly ComboBox _versions;

	public PackageEntry()
	{
		AvaloniaXamlLoader.Load(this);
		_image = this.FindControl<Image>("Image");
		_title = this.FindControl<TextBlock>("Title");
		_versions = this.FindControl<ComboBox>("Versions");
	}

	private PackageEntry(ProjectModel model, NugetSearchResult metadata) : this()
	{
		DataContext = model;
		_title.Text = metadata.Title;
		_versions.Items = metadata.PackageVersions;
		_versions.SelectedIndex = 0;
		var path = $"Nuget/Cache/Icons/{metadata.Title}.png";
		if(File.Exists(path)) _image.Source = new Bitmap(path);
	}

	public static async Task<PackageEntry> Create(ProjectModel model, NugetSearchResult metadata)
	{
		var iconPath = $"Nuget/Cache/Icons/{metadata.Title}.png";
		if (!File.Exists(iconPath) && !string.IsNullOrEmpty(metadata.IconUrl))
		{
			Directory.CreateDirectory("Nuget/Cache/Icons/");
			Debug.Log($"Caching icon for {metadata.Title} from {metadata.IconUrl}");
			using var client = new HttpClient();
			var response = await client.GetAsync(metadata.IconUrl);
			var data = await response.Content.ReadAsByteArrayAsync();
			await File.WriteAllBytesAsync(iconPath, data);
		}

		return new PackageEntry(model, metadata);
	}
}