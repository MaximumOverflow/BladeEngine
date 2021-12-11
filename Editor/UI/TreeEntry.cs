namespace BladeEngine.Editor.UI;

public class TreeEntry
{
	public readonly DirectoryInfo Info;

	public string Name => Info.Name;
	public IReadOnlyList<TreeEntry> Children { get; }

	public TreeEntry(DirectoryInfo root)
	{
		Info = root;
		var sub = root.GetDirectories();
		var children = new List<TreeEntry>();
		foreach (var t in sub)
		{
			var dir = t;
			if((dir.Attributes & FileAttributes.Hidden) != 0 || dir.Name.StartsWith('.')) continue;
			children.Add(new TreeEntry(t));
		}
		
		Children = children;
	}
}