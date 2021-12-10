using NuGet.Common;
using NuGet.Configuration;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace BladeEngine.Editor.NuGet;

public static partial class Nuget
{
	// private static readonly PackageMetadataResource MetadataResource = CreateMetadataResource();

	private static Task<PackageMetadataResource> CreateMetadataResource()
	{
		var providers = new List<Lazy<INuGetResourceProvider>>();
		providers.AddRange(Repository.Provider.GetCoreV3());
		var source = new PackageSource("https://api.nuget.org/v3/index.json");
		var repository = new SourceRepository(source, providers);
		return repository.GetResourceAsync<PackageMetadataResource>();
	}

	public static async Task<IEnumerable<NugetSearchResult>> Search(string search)
	{
		Debug.Log($"Searching NuGet for '{search}'.");
		var dict = new Dictionary<string, (IPackageSearchMetadata, List<NuGetVersion>)>();

		try
		{
			var repo = await CreateMetadataResource();
			var task = await repo.GetMetadataAsync(search, false, false, 
				NullSourceCacheContext.Instance, NullLogger.Instance, CancellationToken.None);
			
			Debug.Log($"NuGet search for '{search}' complete.");
			
			foreach (var metadata in task)
			{
				if (!dict.ContainsKey(metadata.Title))
				{
					var package = metadata;
					var versions = new List<NuGetVersion>{metadata.Identity.Version};
					dict.Add(metadata.Title, (package, versions));
				}
				else
				{
					var data = dict[metadata.Title];
					if (metadata.Identity.Version.CompareTo(data.Item1.Identity.Version) > 0)
						data.Item1 = metadata;
					data.Item2.Add(metadata.Identity.Version);
					dict[metadata.Title] = data;
				}
			}
			
			Debug.Log($"Found {dict.Count} {(dict.Count == 1 ? "result" : "results")} for '{search}'.");
			
			var packages = new List<NugetSearchResult>();
			foreach (var (package, versions) in dict.Values)
			{
				versions.Sort((a, b) => -a.CompareTo(b));
				var ver = versions.Select(v => v.ToString()).ToArray();
			
				packages.Add(new NugetSearchResult
				(
					package.Title, package.Owners, package.Authors, package.Description, package.DownloadCount, ver,
					package.IconUrl?.ToString(), package.LicenseUrl?.ToString(), package.ReadmeUrl?.ToString(), 
					package.ProjectUrl?.ToString(), package.PackageDetailsUrl?.ToString()
				));
			}
			
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