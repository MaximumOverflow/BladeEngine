using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Controls;

namespace BladeEngine.Editor.UI;

public class WarningDialog : Window
{
	public WarningDialog()
	{
		AvaloniaXamlLoader.Load(this);
	}
	
	public WarningDialog(string message) : this()
	{
		this.FindControl<TextBlock>("Text").Text = message;
	}

	private void Close(object? sender, RoutedEventArgs e)
	{
		Close();
	}
}