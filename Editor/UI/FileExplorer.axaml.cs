using BladeEngine.Editor.UI.Models;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Avalonia.Controls;
using Avalonia.Input;

namespace BladeEngine.Editor.UI;

public class FileExplorer : UserControl, IDisposable
{
	private Project _project;
	private ulong _lastClick;
	private FileEntry? _selected;
	private FileViewerModel _model;
	private FileSystemWatcher _watcher;

	public FileExplorer()
	{
		_model = null!;
		_project = null!;
		_watcher = null!;

		AvaloniaXamlLoader.Load(this);

		// ReSharper disable once ConditionIsAlwaysTrueOrFalse
		if (EditorWindow.Instance is not null)
		{
			EditorWindow.Instance.OnProjectOpened += OpenProject;
			EditorWindow.Instance.OnProjectClosed += CloseProject;
		}
	}

	private void OpenProject(Project project)
	{
		_project = project;
		var directory = project.File.Directory!;
		DataContext = _model = new FileViewerModel(directory);
		_model.CurrentDirectory = directory.FullName;
		_watcher = new FileSystemWatcher(directory.FullName)
		{
			IncludeSubdirectories = true, EnableRaisingEvents = true,
			NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.FileName | NotifyFilters.DirectoryName
		};
		_watcher.Created += OnFileChanged;
		_watcher.Deleted += OnFileChanged;
		_watcher.Renamed += OnFileChanged;
		ScheduleUpdate(true, true);
	}

	private void CloseProject()
	{
		var model = (FileViewerModel) DataContext!;
		_watcher.Created -= OnFileChanged;
		_watcher.Deleted -= OnFileChanged;
		_watcher.Renamed -= OnFileChanged;
		_watcher.Dispose();
		_project = null!;
		model.CurrentDirectory = "";
	}

	private void ScheduleUpdate(bool entries, bool tree) => Dispatcher.UIThread.InvokeAsync(() =>
	{
		if(tree) _model.UpdateTree();
		if(entries) _model.UpdateEntries();
	});
	
	private void OnFileChanged(object? sender, FileSystemEventArgs e)
	{
		if (Path.HasExtension(e.Name))
		{
			var info = new FileInfo(e.FullPath);
			if (info.Directory!.FullName == _model.CurrentDirectory) ScheduleUpdate(true, false);
		}
		else
		{
			var info = new DirectoryInfo(e.FullPath);
			ScheduleUpdate(info.Parent!.FullName == _model.CurrentDirectory, true);
		}
	}

	public void Dispose()
	{
		_watcher.Dispose();
		EditorWindow.Instance.OnProjectOpened -= OpenProject;
		EditorWindow.Instance.OnProjectClosed -= CloseProject;
	}
	
	private void OnEntryClicked(object? sender, PointerReleasedEventArgs e)
	{
		var item = (Control) sender!;
		var entry = (FileEntry) item.DataContext!;

		if (e.GetCurrentPoint(null).Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonReleased)
		{
			if (e.Timestamp - _lastClick < 400 && _selected == entry)
			{
				Open(entry.Info);
				_lastClick = 0;
			}
			else _lastClick = e.Timestamp;
			
			_selected = entry;
		}
	}
	
	private void OnTreeSelectionChanged(object? sender, SelectionChangedEventArgs e)
	{
		var tree = (TreeView) sender!;
		if (tree.SelectedItem is not Control item)
		{
			if(e.AddedItems.Count == 0) return;
			if(e.AddedItems[0] is not TreeEntry tEntry) return; 
			Open(tEntry.Info);
			return;
		}
		
		var entry = (TreeEntry) item.DataContext!;
		Open(entry.Info);
	}

	private void Open(object? sender, RoutedEventArgs e)
	{
		var item = (Control) sender!;
		Open(((FileEntry) item.DataContext!).Info);
	}

	private void Open(FileSystemInfo entry)
	{
		switch (entry)
		{
			case FileInfo file:
			{
				var path = FileAssociations.GetProgramPath(file.Extension);
				if (path is null)
				{
					Debug.LogWarning($"Could not open file '{file.FullName}'.");
					return;
				}
				Process.Start(new ProcessStartInfo
				{
					FileName = path, Arguments = file.FullName, 
					UseShellExecute = true
				});
				break;
			}

			case DirectoryInfo directory:
			{
				_model.CurrentDirectory = directory.FullName;
				break;
			}
		}
	}
	
	private void Copy(object? sender, RoutedEventArgs e)
	{
		
	}

	private void Rename(object? sender, RoutedEventArgs e)
	{
		
	}

	private void Delete(object? sender, RoutedEventArgs e)
	{
		var item = (Control) sender!;
		var entry = (FileEntry) item.DataContext!;
		entry.Info.Delete();
	}
}