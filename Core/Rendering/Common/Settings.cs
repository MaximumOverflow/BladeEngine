using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace BladeEngine.Core.Rendering.Common;

public readonly struct GraphicsSettings
{
	public Graphics.Api Api { get; init; } = Graphics.Api.OpenGL;
	public int Width { get; init; } = 1280;
	public int Height { get; init; } = 720;
	public bool VSync { get; init; } = true;
	public bool Visible { get; init; } = true;
	public string Title { get; init; } = "Blade Window";

	public static implicit operator WindowOptions(in GraphicsSettings s) => new()
	{
		API = s.Api switch
		{
			Graphics.Api.OpenGL => GraphicsAPI.Default,
			Graphics.Api.Vulkan => GraphicsAPI.DefaultVulkan,
			_ => throw new ArgumentOutOfRangeException(nameof(s.Api))
		},
		Size = new Vector2D<int>(s.Width, s.Height),
		Title = s.Title, IsVisible = s.Visible, VSync = s.VSync,
	};
}