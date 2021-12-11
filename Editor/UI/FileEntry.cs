using Material.Icons;

namespace BladeEngine.Editor.UI;

public class FileEntry
{
	public readonly FileSystemInfo Info;

	public string Name => Info.Name;
	
	public bool Editable { get; }
	public MaterialIconKind Icon { get; private init; }
	private const MaterialIconKind ProjectIcon = MaterialIconKind.SettingsApplications;

	public FileEntry(FileInfo info)
	{
		Info = info;
		Icon = GetIcon(Info.Extension);
		Editable = Icon != ProjectIcon;
	}
	
	public FileEntry(DirectoryInfo info)
	{
		Info = info;
		Icon = MaterialIconKind.Folder;
	}

	public static FileEntry UpperDirectory(DirectoryInfo current)
	{
		return new FileEntry(current.Parent!) {Icon = MaterialIconKind.FolderUpload};
	}

	private static MaterialIconKind GetIcon(string extension)
	{
		switch (extension)
		{
			case ".cs": return MaterialIconKind.FileCode;
			case ".dll": return MaterialIconKind.Bookshelf;
			case ".exe": return MaterialIconKind.Application;
			case ".csproj": return ProjectIcon;
			
			case ".txt": 
			case ".json": return MaterialIconKind.FileDocument;
			
			default: return MaterialIconKind.HelpBox;
		}
	}
}