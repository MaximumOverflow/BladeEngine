using System.Collections.Concurrent;

namespace BladeEngine.Editor;

public static class Concurrency
{
	public delegate void EventDelegate(Guid id, string? name);
	public static event EventDelegate? OnLaunch, OnComplete, OnAbort;
	private static readonly ConcurrentDictionary<Guid, Task> Tasks = new();

	public static Task ScheduleTask(Action action, string? name, Task? dependency = null, Action? onComplete = null, Action<Exception>? onAbort = null)
	{
		var guid = Guid.NewGuid();
		OnLaunch?.Invoke(guid, name);
		return Task.Run(async () =>
		{
			if (dependency is not null)
				await dependency;
			
			try
			{
				action.Invoke();
				Tasks.TryRemove(guid, out _);
			}
			catch (Exception e)
			{
				onAbort?.Invoke(e);
				OnAbort?.Invoke(guid, name);
				Tasks.TryRemove(guid, out _);
				return;
			}
			
			onComplete?.Invoke();
			OnComplete?.Invoke(guid, name);
		});
	}
	
	public static Task<T?> ScheduleTask<T>(Func<T> action, string? name, Task? dependency = null, Action? onComplete = null, Action<Exception>? onAbort = null)
	{
		var guid = Guid.NewGuid();
		OnLaunch?.Invoke(guid, name);
		return Task.Run(async () =>
		{
			if (dependency is not null)
				await dependency;
			
			T? result;
			
			try
			{
				result = action.Invoke();
				Tasks.TryRemove(guid, out _);
			}
			catch (Exception e)
			{
				onAbort?.Invoke(e);
				OnAbort?.Invoke(guid, name);
				Tasks.TryRemove(guid, out _);
				return default;
			}
			
			onComplete?.Invoke();
			OnComplete?.Invoke(guid, name);
			return result;
		});
	}
	
	public static Task<T?> ScheduleTask<T>(Func<Task<T?>> action, string? name, Task? dependency = null, Action? onComplete = null, Action<Exception>? onAbort = null)
	{
		var guid = Guid.NewGuid();
		OnLaunch?.Invoke(guid, name);
		return Task.Run(async () =>
		{
			if (dependency is not null)
				await dependency;

			T? result;
			
			try
			{
				result = await action.Invoke();
				Tasks.TryRemove(guid, out _);
			}
			catch (Exception e)
			{
				onAbort?.Invoke(e);
				OnAbort?.Invoke(guid, name);
				Tasks.TryRemove(guid, out _);
				return default;
			}
			
			onComplete?.Invoke();
			OnComplete?.Invoke(guid, name);
			return result;
		});
	}
}