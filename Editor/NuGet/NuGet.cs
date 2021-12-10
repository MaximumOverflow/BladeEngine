using Avalonia.Media.Imaging;

namespace BladeEngine.Editor.NuGet;

public static partial class NuGet
{
	static NuGet()
	{
		Directory.CreateDirectory("Nuget/Cache/Icons/");
	}

	public static async Task<Bitmap?> FetchIcon(NugetSearchResult package)
	{
		var iconPath = $"Nuget/Cache/Icons/{package.Title}.png";
		if (File.Exists(iconPath)) return new Bitmap(iconPath);
		if (string.IsNullOrEmpty(package.IconUrl)) return null;
		Debug.Log($"Caching icon for {package.Title} from {package.IconUrl}");
		using var client = new HttpClient();
		var response = await client.GetAsync(package.IconUrl);
		var data = await response.Content.ReadAsByteArrayAsync();
		await File.WriteAllBytesAsync(iconPath, data);
		return new Bitmap(iconPath);
	}

	public static async Task<Bitmap?> FetchIcon(NugetSearchResult package, Action<Bitmap?> callback)
	{
		var icon = await FetchIcon(package);
		callback.Invoke(icon);
		return icon;
	}
}