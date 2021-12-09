using File = System.IO.File;

namespace BladeEngine.Core.Rendering.Common;

public abstract class Mesh : IDisposable
{
	public abstract uint[] Indices { get; set; }
	public abstract Vector3[] Normals { get; set; }
	public abstract Vector3[] Vertices { get; set; }

	public abstract void Update();
	public abstract void Dispose();

	public static Mesh? Create()
	{
		switch (Graphics.CurrentApi)
		{
			case Graphics.Api.None:
				Debug.LogError("Could not create mesh, no graphics api is currently being used.");
				return null;
			
			case Graphics.Api.OpenGL:
				return new OpenGL.Mesh();

			default:
				Debug.LogError($"Could not create mesh, {Graphics.CurrentApi} is not currently supported.");
				return null;
		}
	}

	public static Mesh? FromFile(string path)
	{
		if (!File.Exists(path))
		{
			Debug.LogError($"Could not load mesh, \"{path}\" does not exist.");
			return null;
		}

		switch (Graphics.CurrentApi)
		{
			case Graphics.Api.None:
				Debug.LogError("Could not load mesh, no graphics api is currently being used.");
				return null;
			
			case Graphics.Api.OpenGL:
				return OpenGL.Mesh.FromFile(path);

			default:
				Debug.LogError($"Could not load mesh, {Graphics.CurrentApi} is not currently supported.");
				return null;
		}
	}
}