using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Avalonia.Controls;

namespace BladeEngine.Editor.UI;

public class TaskViewer : UserControl, IDisposable
{
	private readonly StackPanel _stack;
	private readonly Dictionary<Guid, TaskEntry> _tasks;

	public TaskViewer()
	{
		AvaloniaXamlLoader.Load(this);
		_tasks = new Dictionary<Guid, TaskEntry>();
		_stack = this.FindControl<StackPanel>("Tasks");
		Concurrency.OnComplete += OnComplete;
		Concurrency.OnLaunch += OnLaunch;
		Concurrency.OnAbort += OnAbort;
	}

	private void OnLaunch(Guid id, string? name)
	{
		Dispatcher.UIThread.InvokeAsync(() =>
		{
			var entry = new TaskEntry(name ?? id.ToString());
			_stack.Children.Add(entry);
			_tasks.Add(id, entry);
		});
	}

	private void OnAbort(Guid id, string? name)
	{
		Dispatcher.UIThread.InvokeAsync(() =>
		{
			if(!_tasks.Remove(id, out var entry)) return;
			entry.Abort();
		});
	}
	
	private void OnComplete(Guid id, string? name)
	{
		Dispatcher.UIThread.InvokeAsync(() =>
		{
			if(!_tasks.Remove(id, out var entry)) return;
			entry.Complete();
			Dispatcher.UIThread.InvokeAsync(async () =>
			{
				await Task.Delay(2500);
				_stack.Children.Remove(entry);
			});
		});
	}

	public void Dispose()
	{
		Concurrency.OnAbort -= OnAbort;
		Concurrency.OnLaunch -= OnLaunch;
		Concurrency.OnComplete -= OnComplete;
	}

	private void Clear(object? sender, RoutedEventArgs e)
	{
		var remove = new List<TaskEntry>();
		// ReSharper disable once PossibleInvalidCastExceptionInForeachLoop
		foreach (TaskEntry task in _stack.Children) if(!task.Active) remove.Add(task);
		_stack.Children.RemoveAll(remove);
	}
}