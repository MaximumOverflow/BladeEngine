using System.Text.RegularExpressions;

namespace BladeEngine.Extensions;

public static class RegexExtensions
{
	public static bool IsExactMatch(this Regex regex, string? text)
	{
		if (text is null) return false;
		return regex.Match(text).Value == text;
	}
}