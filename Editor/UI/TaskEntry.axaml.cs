using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Avalonia.Controls;
using Avalonia.Media;

namespace BladeEngine.Editor.UI;

public class TaskEntry : UserControl
{
	private readonly Stopwatch _stopwatch;
	private readonly TextBlock _name, _status, _elapsed;
	
	public bool Active { get; private set; } = true;
	public bool Aborted { get; private set; } = false;

	public TaskEntry()
	{
		AvaloniaXamlLoader.Load(this);
		_stopwatch = Stopwatch.StartNew();
		_name = this.FindControl<TextBlock>("Name");
		_status = this.FindControl<TextBlock>("Status");
		_elapsed = this.FindControl<TextBlock>("Elapsed");
	}

	public TaskEntry(string name) : this()
	{
		_name.Text = name;
		_status.Text = "Running...";
		_elapsed.Text = "00:00:00";
	}

	public void Complete()
	{
		var green = new SolidColorBrush(Colors.Green);
		_status.Text = "Complete";
		_name.Foreground = green;
		_status.Foreground = green;
		_elapsed.Foreground = green;
		_stopwatch.Stop();
		Active = false;
	}

	public void Abort()
	{
		var red = new SolidColorBrush(Colors.Red);
		_status.Text = "Aborted";
		_name.Foreground = red;
		_status.Foreground = red;
		_elapsed.Foreground = red;
		_stopwatch.Stop();
		Aborted = true;
		Active = false;
	}

	public override void Render(DrawingContext context)
	{
		base.Render(context);
		var e = _stopwatch.Elapsed;
		_elapsed.Text = $"{e.Hours:00}:{e.Minutes:00}:{e.Seconds:00}";
		if(Active) Dispatcher.UIThread.InvokeAsync(InvalidateVisual, DispatcherPriority.Background);
	}
}