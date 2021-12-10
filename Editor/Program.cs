using BladeEngine.Editor.UI;
using Avalonia.ReactiveUI;
using Avalonia;

namespace BladeEngine.Editor;

public static class Program
{
	[STAThread]
	public static void Main(string[] args)
	{
		try
		{
			Graphics.Initialize(new GraphicsSettings{Visible = false});
			Graphics.SetClearColor(35f / 256f, 40f / 256f, 51f / 256f);
			BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
			Graphics.Terminate();
		}
		catch (Exception e) { Debug.LogFatal(e); throw; }
	}

	public static AppBuilder BuildAvaloniaApp()
		=> AppBuilder.Configure<App>().UsePlatformDetect().UseReactiveUI().LogToTrace();
}
