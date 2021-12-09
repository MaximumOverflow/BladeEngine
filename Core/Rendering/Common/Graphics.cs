namespace BladeEngine.Core.Rendering.Common;

public static class Graphics
{
	public enum Api { None, OpenGL, Vulkan }
	internal static GraphicsApi Current { get; set; } = null!;
	public static Api CurrentApi { get; internal set; } = Api.None;

	#region Bindings

	// ReSharper disable once ConstantConditionalAccessQualifier
	public static bool IsRendering => Current?.Running ?? false;
	
	public static bool Initialize(GraphicsSettings settings)
	{
		switch (settings.Api)
		{
			case Api.None: return false;
			case Api.OpenGL: return OpenGL.OpenGL.Initialize(settings);
			default: throw new NotImplementedException();
		}
	}

	public static bool Terminate()
	{
		switch (CurrentApi)
		{
			case Api.None: return true;
			case Api.OpenGL: return OpenGL.OpenGL.Terminate();
			default: throw new NotImplementedException();
		}
	}
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Clear() => Current.Clear();
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void SwapBuffers() => Current.SwapBuffers();

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void SetClearColor(float r, float g, float b) 
		=> Current.SetClearColor(r, g, b);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void SetRenderingResolution(uint width, uint height)
		=> Current.SetRenderingResolution(width, height);
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void GetRenderingResolution(out uint width, out uint height)
		=> Current.GetRenderingResolution(out width, out height);
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void FlushRenderBuffer() 
		=> Current.FlushRenderBuffer();
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void DrawMesh(Mesh mesh, Material material, in Matrix4x4 transform) 
		=> Current.DrawMesh(mesh, material, transform);
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void DrawMesh(Mesh mesh, Material material, IEnumerable<Matrix4x4> transforms) 
		=> Current.DrawMesh(mesh, material, transforms);
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ReadPixels(Span<byte> bytes) 
		=> Current.ReadPixels(bytes);
	
	#endregion

	#region Utilities

	public static Matrix4x4 DefaultProjection { get; set; } = Matrix4x4.Identity;

	#endregion
}

public abstract class GraphicsApi
{
	public abstract bool Running { get; }
	
	public abstract void Clear();
	public abstract void SwapBuffers();
	public abstract void SetClearColor(float r, float g, float b);
	public abstract void SetRenderingResolution(uint width, uint height);
	public abstract void GetRenderingResolution(out uint width, out uint height);

	public abstract void ReadPixels(Span<byte> bytes);
	public abstract void FlushRenderBuffer();
	public abstract void DrawMesh(Mesh mesh, Material material, in Matrix4x4 transform);
	public abstract void DrawMesh(Mesh mesh, Material material, IEnumerable<Matrix4x4> transforms);
}