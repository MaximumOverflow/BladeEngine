namespace BladeEngine.Core.Rendering.Common;

public abstract class Shader
{
	public abstract string Source { get; }
	public abstract IReadOnlyDictionary<string, (int, DataType)> Parameters { get; }
	
	public abstract void Use();
	public abstract bool SetParameter<T>(string name, T value);
	
	public static Shader? FromFile(string path)
	{
		if (!File.Exists(path))
		{
			Debug.LogError($"Could not load shader, \"{path}\" does not exist.");
			return null;
		}

		switch (Graphics.CurrentApi)
		{
			case Graphics.Api.None:
				Debug.LogError("Could not load shader, no graphics api is currently being used.");
				return null;

			default:
				Debug.LogError($"Could not load shader, {Graphics.CurrentApi} is not currently supported.");
				return null;
		}
	}
	
	public static Shader? FromSource(string source)
	{
		switch (Graphics.CurrentApi)
		{
			case Graphics.Api.None:
				Debug.LogError("Could not load shader, no graphics api is currently being used.");
				return null;

			default:
				Debug.LogError($"Could not load shader, {Graphics.CurrentApi} is not currently supported.");
				return null;
		}
	}
}