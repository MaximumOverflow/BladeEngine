using BladeEngine.Extensions;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia;

namespace BladeEngine.Editor.UI;

public class NewProjectDialog : Window
{
	private Project? _project;
	private readonly Button _create;
	private readonly TextBlock _errors;
	private readonly TextBox _directory, _name, _assembly, _namespace;
	
	public NewProjectDialog()
	{
		AvaloniaXamlLoader.Load(this);
		_name = this.FindControl<TextBox>("Name");
		_create = this.FindControl<Button>("Create");
		_errors = this.FindControl<TextBlock>("Errors");
		_assembly = this.FindControl<TextBox>("Assembly");
		_directory = this.FindControl<TextBox>("Directory");
		_namespace = this.FindControl<TextBox>("Namespace");
		_errors.Text = String.Empty;

		#if DEBUG
		this.AttachDevTools();
		#endif
	}

	private void CheckInput(object? sender, KeyEventArgs e) => Check(null, null);

	private bool Check(object? sender, RoutedEventArgs? e)
	{
		var errors = "";
		var path = _directory.Text ?? String.Empty;
		var projPath = Path.Combine(path, _name.Text ?? string.Empty);
		
		if (!Directory.Exists(path)) errors += "Directory does not exist.\n";
		if (Directory.Exists(projPath)) errors += "Project directory already exists.\n";
		if (_name.Text?.Length == 0 || !Project.NameRegex.IsExactMatch(_name.Text)) errors += "Invalid name.\n";
		if (_assembly.Text?.Length == 0 || !Project.NamespaceRegex.IsExactMatch(_assembly.Text)) errors += "Invalid assembly name.\n";
		if (_namespace.Text?.Length == 0 || !Project.NamespaceRegex.IsExactMatch(_namespace.Text)) errors += "Invalid root namespace.\n";
		_create.IsEnabled = string.IsNullOrEmpty(errors);
		_errors.Text = errors;
		return _create.IsEnabled;
	}
	
	private async void Browse(object? sender, RoutedEventArgs e)
	{
		var dialog = new OpenFolderDialog();
		var path = await dialog.ShowAsync(this);
		if(path is null) return;
		
		var dir = new DirectoryInfo(path);
		_directory.Text = path;
		_name.Text = dir.Name;
		_assembly.Text = dir.Name;
		_namespace.Text = dir.Name;
		Check(null, null);
	}

	private void Create(object? sender, RoutedEventArgs e)
	{
		if(!Check(null, null)) return;
		Debug.Log($"Creating project '{_name.Text}'...");
		_project = Project.Create(_directory.Text, _name.Text, _assembly.Text, _namespace.Text);
		Debug.Log("Project created successfully!");
		Close();
	}

	private void Cancel(object? sender, RoutedEventArgs e)
	{
		Close();
	}
	
	public async Task<Project?> Open(Window window)
	{
		await ShowDialog(window);
		return _project;
	}
}