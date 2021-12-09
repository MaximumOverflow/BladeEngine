using BladeEngine.Extensions;
using System.Diagnostics;
using Serilog.Events;
using Serilog;

namespace BladeEngine.Core;

public static class Debug
{
	static Debug()
	{
		try
		{
			Directory.CreateDirectory("logs");
			File.Delete("logs/log.txt");

			#if DEBUG
			Serilog.Log.Logger = new LoggerConfiguration()
				.WriteTo.File("logs/log.txt", LogEventLevel.Debug)
				.WriteTo.Console()
				.CreateLogger();
			#else
			Serilog.Log.Logger = new LoggerConfiguration()
				.WriteTo.File("logs/log.txt", LogEventLevel.Error)
				.CreateLogger();
			#endif
		}
		catch (Exception e)
		{
			Console.WriteLine(e);
		}
	}

	public static event Action<string?>? OnLog; 

	[Conditional("DEBUG")]
	public static void Log(object obj)
	{
		var fmt = obj.ToString();
		Serilog.Log.Information(fmt);
		OnLog?.Invoke(fmt);
	}

	[Conditional("DEBUG")]
	public static void Log(string fmt, params object?[] args)
	{
		Serilog.Log.Information(fmt, args);
		OnLog?.Invoke(string.Format(fmt, args));
	}

	[Conditional("DEBUG")]
	public static void LogWarning(object obj)
	{
		var fmt = obj.ToString();
		Serilog.Log.Warning(fmt);
		OnLog?.Invoke(fmt);
	}

	[Conditional("DEBUG")]
	public static void LogWarning(string fmt, params object?[] args)
	{
		Serilog.Log.Warning(fmt, args);
		OnLog?.Invoke(string.Format(fmt, args));
	}

	public static void LogError(object obj)
	{
		var fmt = obj.ToString();
		var trace = obj is Exception e ? e.StackTrace : string.Join('\n', Environment.StackTrace.Split('\n').Skip(2));
		Serilog.Log.Error($"{obj}\n{trace}");
		OnLog?.Invoke(fmt);
	}
	
	public static void LogError(string fmt, params object?[] args)
	{
		var trace = string.Join('\n', Environment.StackTrace.Split('\n').Skip(2));
		Serilog.Log.Error($"{fmt}\n{trace}", args);
		OnLog?.Invoke(string.Format(fmt, args));
	}

	public static void LogFatal(object obj)
	{
		var fmt = obj.ToString();
		var trace = obj is Exception e ? e.StackTrace : string.Join('\n', Environment.StackTrace.Split('\n').Skip(2));
		Serilog.Log.Fatal($"{obj}\n{trace}");
		OnLog?.Invoke(fmt);
	}

	public static void LogFatal(string fmt, params object?[] args)
	{
		var trace = string.Join('\n', Environment.StackTrace.Split('\n').Skip(2));
		Serilog.Log.Fatal($"{fmt}\n{trace}", args);
		OnLog?.Invoke(string.Format(fmt, args));
	}
}