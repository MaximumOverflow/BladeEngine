using System.Text.Json;

namespace BladeEngine.Editor;

public static class FileAssociations
{
	private static readonly string FilePath;
	private static readonly Dictionary<string, string> Associations;

	static FileAssociations()
	{
		FilePath = Path.Combine(EnvironmentVariables.SettingsDirPath, "FileAssociations.json");
		Associations = new Dictionary<string, string>();
		UpdateAssociations();
	}

	public static string? GetProgramPath(string extension)
		=> Associations.TryGetValue(extension, out var path) ? path : null;

	private static void UpdateAssociations()
	{
		Debug.Log("Updating file associations...");
		if(!File.Exists(FilePath)) File.WriteAllText(FilePath, "{\n}");

		var json = JsonDocument.Parse(File.ReadAllText(FilePath));
		foreach (var property in json.RootElement.EnumerateObject())
		{
			var extension = property.Name;
			var path = property.Value.GetString();

			if (path is null)
			{
				Debug.LogError($"Could not create file association for '{extension}'.");
				continue;
			}

			if (!extension.StartsWith('.'))
			{
				Debug.LogError($"Could not create file association for '{extension}'. '{extension}' is not a valid extension");
				continue;
			}
			
			Associations.TryAdd(property.Name, path);
		}
	}
}