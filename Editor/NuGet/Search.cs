using NuGet.Protocol.Core.Types;
using NuGet.Configuration;
using NuGet.Common;

namespace BladeEngine.Editor.NuGet;

public static partial class NuGet
{
	// private static readonly PackageMetadataResource MetadataResource = CreateMetadataResource();

	private static Task<PackageSearchResource> CreateMetadataResource()
	{
		var providers = new List<Lazy<INuGetResourceProvider>>();
		providers.AddRange(Repository.Provider.GetCoreV3());
		var source = new PackageSource("https://api.nuget.org/v3/index.json");
		var repository = new SourceRepository(source, providers);
		return repository.GetResourceAsync<PackageSearchResource>();
	}

	public static async Task<IEnumerable<NugetSearchResult>> Search(string search)
	{
		Debug.Log($"Searching NuGet for '{search}'.");

		try
		{
			var repo = await CreateMetadataResource();
			var results = await repo.SearchAsync(search, new SearchFilter(false), 
				0, 32, NullLogger.Instance, CancellationToken.None);

			Debug.Log($"NuGet search for '{search}' complete.");

			
			var packages = new List<NugetSearchResult>();
			foreach (var package in results)
			{
				var versions = (await package.GetVersionsAsync()).Select(v => v.Version.ToString()).ToList();
				versions.Reverse();
				
				packages.Add(new NugetSearchResult
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
			return Array.Empty<NugetSearchResult>();
		}
	}
}

public readonly record struct NugetSearchResult
(
	string Title,
	string Owners,
	string Authors,
	string Description,
	long? DownloadCount,
	IReadOnlyList<string> PackageVersions,
	string? IconUrl, string? LicenseUrl, string? ReadmeUrl, string? ProjectUrl, string? DetailsUrl
);