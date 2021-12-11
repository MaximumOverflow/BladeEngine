using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Avalonia.Controls;
using Avalonia;

namespace BladeEngine.Editor.UI;

public class BuildProcessWindow : Window
{
	private readonly TextBlock _text, _title;
	private readonly ScrollViewer _scroll;
	
	public BuildProcessWindow()
	{
		AvaloniaXamlLoader.Load(this);
		_text = this.FindControl<TextBlock>("Text");
		_title = this.FindControl<TextBlock>("Title");
		_scroll = this.FindControl<ScrollViewer>("Scroll");
		
		#if DEBUG
		this.AttachDevTools();
		#endif
	}

	public async Task<int> Open(Window window, Project project)
	{
		Title = $"Building {project.AssemblyName}...";

		ShowDialog(window);
		var exe = new Executable(project);
		var result = await exe.Build(s => Dispatcher.UIThread.InvokeAsync(() =>
		{
			_text.Text += s + '\n';
			_scroll.ScrollToEnd();
		}));

		_title.Text = result == 0 ? "Build completed" : "Build failed";
		return result;
	}
}