using NuGet.Protocol.Core.Types;
using Avalonia.Media.Imaging;
using NuGet.Configuration;
using NuGet.Common;

namespace BladeEngine.Editor;

public readonly record struct NugetPackageInfo
(
	string Title,
	string Owners,
	string Authors,
	string Description,
	long? DownloadCount,
	IReadOnlyList<string> PackageVersions,
	string? IconUrl, string? LicenseUrl, string? ReadmeUrl, string? ProjectUrl, string? DetailsUrl
);

public static partial class NuGet
{
	private static readonly SourceRepository Repository;
	
	static NuGet()
	{
		Directory.CreateDirectory("Nuget/Cache/Icons/");
		var providers = new List<Lazy<INuGetResourceProvider>>();
		providers.AddRange(global::NuGet.Protocol.Core.Types.Repository.Provider.GetCoreV3());
		var source = new PackageSource("https://api.nuget.org/v3/index.json");
		Repository = new SourceRepository(source, providers);
	}
	
	public static async Task<NugetPackageInfo?> FetchInfo(string package)
	{
		var results = await Search(package, 1);
		var result = results.FirstOrDefault();
		return result.Title == package ? result : null;
	}

	public static async Task<Bitmap?> FetchIcon(NugetPackageInfo package)
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

	public static async Task<Bitmap?> FetchIcon(NugetPackageInfo package, Action<Bitmap?> callback)
	{
		var icon = await FetchIcon(package);
		callback.Invoke(icon);
		return icon;
	}
	
	public static async Task<IEnumerable<NugetPackageInfo>> Search(string search, int maxResults = 32)
	{
		Debug.Log($"Searching NuGet for '{search}'.");

		try
		{
			var repo = await Repository.GetResourceAsync<PackageSearchResource>();
			var results = await repo.SearchAsync(search, new SearchFilter(false), 
				0, maxResults, NullLogger.Instance, CancellationToken.None);

			Debug.Log($"NuGet search for '{search}' complete.");

			
			var packages = new List<NugetPackageInfo>();
			foreach (var package in results)
			{
				var versions = (await package.GetVersionsAsync()).Select(v => v.Version.ToString()).ToList();
				versions.Reverse();
				
				packages.Add(new NugetPackageInfo
				(
					package.Title, package.Owners, package.Authors, package.Description, package.DownloadCount, versions,
					package.IconUrl?.ToString(), package.LicenseUrl?.ToString(), package.ReadmeUrl?.ToString(), 
					package.ProjectUrl?.ToString(), package.PackageDetailsUrl?.ToString()
				));
			}
			
			Debug.Log($"Found {packages.Count} {(packages.Count == 1 ? "package" : "packages")} for '{search}'.");

			return packages;
		}
		catch (Exception e)
		{
			Debug.LogError(e);
			return Array.Empty<NugetPackageInfo>();
		}
	}
}