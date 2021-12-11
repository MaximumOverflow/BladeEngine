using System.Collections.ObjectModel;
using ReactiveUI;

namespace BladeEngine.Editor.UI.Models;

public sealed class FileViewerModel : ReactiveObject
{
	private readonly string _root;
	private string _currentDirectory;
	public ObservableCollection<FileEntry> Entries { get; }

	public FileViewerModel(DirectoryInfo root)
	{
		_root = root.FullName;
		_currentDirectory = _root;
		Tree = new ObservableCollection<TreeEntry>(new []{new TreeEntry(root)});
		Entries = new ObservableCollection<FileEntry>();
	}
	
	public ObservableCollection<TreeEntry> Tree { get; }
	
	public string CurrentDirectory
	{
		get => _currentDirectory;
		set
		{
			_currentDirectory = value;
			UpdateEntries();
		}
	}

	public void UpdateEntries()
	{
		Entries.Clear();
		if (!string.IsNullOrWhiteSpace(_currentDirectory))
		{
			var dirInfo = new DirectoryInfo(_currentDirectory);
			if(_currentDirectory != _root) Entries.Add(FileEntry.UpperDirectory(dirInfo));

			foreach (var directory in dirInfo.GetDirectories())
			{
				if((directory.Attributes & FileAttributes.Hidden) != 0 || directory.Name.StartsWith('.')) continue;
				Entries.Add(new FileEntry(directory));
			}

			foreach (var file in dirInfo.GetFiles())
			{
				if((file.Attributes & FileAttributes.Hidden) != 0 || file.Name.StartsWith('.')) continue;
				Entries.Add(new FileEntry(file));
			}
		}
		this.RaisePropertyChanged(nameof(CurrentDirectory));
	}

	public void UpdateTree()
	{
		Tree.Clear();
		Tree.Add(new TreeEntry(new DirectoryInfo(_root)));
		this.RaisePropertyChanged(nameof(Tree));
	}
}