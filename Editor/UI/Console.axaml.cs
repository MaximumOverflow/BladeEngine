using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Input;

namespace BladeEngine.Editor.UI;

public class Console : UserControl
{
	private readonly TextBlock _text;
	private readonly ScrollViewer _scroll;
	private readonly AutoCompleteBox _command;
	private readonly Dictionary<string, ICommand> _commands = new();

	public Console()
	{
		AvaloniaXamlLoader.Load(this);
		_text = this.FindControl<TextBlock>("Text");
		_scroll = this.FindControl<ScrollViewer>("Scroll");
		_command = this.FindControl<AutoCompleteBox>("Command");
		
		//Warning This could lead to a memory leak when destroying the control
		Debug.OnLog += OnLog;
		
		RegisterCommands();
	}

	private void OnLog(string? text)
	{
		Dispatcher.UIThread.InvokeAsync(() => _text.Text += text + '\n');
	}

	private void InputElement_OnKeyDown(object? _, KeyEventArgs e)
	{
		var input = _command.Text;
		if (string.IsNullOrWhiteSpace(input)) return;

		var bits = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
		var args = bits.Length > 1 ? bits.AsSpan(1) : default;

		if (bits.Length == 1) _command.Items = _commands.Keys;
		if (!_commands.TryGetValue(bits[0], out var command))
		{
			if (e.Key is not (Key.Return or Key.Enter)) return;
			_text.Text += $"Unknown command '{bits[0]}'.\n";
			_command.Text = string.Empty;
			return;
		}

		if (bits.Length > 1)
		{
			_command.Items = command.Autocomplete(args);
			//TODO set the selector so it ignores the previous argument
		}
		
		if (e.Key is Key.Return or Key.Enter)
		{
			var result = command.Run(this, args);
			if (result is not null) _text.Text += $"Could not execute command '{command.Name}': {result}.\n";
			else _text.Text += input + '\n';
			_command.Text = string.Empty;
		}
	}

	#region Commands

	public interface ICommand
	{
		public string Name { get; }
		
		public string? Run(Console console, Span<string> args);
		
		public IEnumerable<object> Autocomplete(Span<string> args) 
			=> Array.Empty<object>();
	}
	
	private void RegisterCommands()
	{
		var types = GetType().GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Public);
		Debug.Log($"Trying to register {types.Length} console commands...");
		var count = types.Count(RegisterCommand);
		Debug.Log($"Registered {count} console commands...");
	}

	public bool RegisterCommand(Type type)
	{
		if (type.IsInterface || type.IsAbstract || !type.IsAssignableTo(typeof(ICommand)))
			return false;

		if (Activator.CreateInstance(type) is not ICommand command)
		{
			Debug.LogError($"Could not add command {type.FullName}. All commands must be default constructable.");
			return false;
		}

		var name = command.Name.ToLower();
		if (!_commands.TryAdd(name, command))
		{
			Debug.LogError($"Could not add command '{name}'. Command already exists.");
			return false;
		}

		return true;
	}

	private class Clear : ICommand
	{
		public string Name => "clear";
		
		public string? Run(Console console, Span<string> args)
		{
			if (!args.IsEmpty) return "invalid number of parameters";
			console._text.Text = "";
			return null;
		}
	}

	private class Set : ICommand
	{
		public string Name => "set";

		public string? Run(Console console, Span<string> args)
		{
			if (args.Length < 2) return "invalid number of parameters: the command requires at least 2 parameters";
			if (!_subCommands.TryGetValue(args[0], out var subCommand)) return $"unknown variable '{args[0]}'";
			return subCommand.Invoke(args[1..]);
		}

		public IEnumerable<object> Autocomplete(Span<string> args)
		{
			if (args.IsEmpty) return _subCommands.Keys;
			return Array.Empty<object>();
		}
		
		public delegate string? SubCommand(Span<string> args);
		private readonly Dictionary<string, SubCommand> _subCommands = new();
	}

	#endregion
}