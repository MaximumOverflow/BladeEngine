namespace BladeEngine.Extensions;

public static class EnumerableExtensions
{
	public static T[] Skip<T>(this T[] list, int count)
	{
		if (count >= list.Length) return Array.Empty<T>();
		if (count <= 0)
		{
			var copy = new T[list.Length];
			list.AsSpan().CopyTo(copy);
			return copy;
		}

		var array = new T[list.Length - count];
		list.AsSpan(count).CopyTo(array);
		return array;
	}
}