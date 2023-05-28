using System.Collections.Generic;
using System.Numerics;
using System.Text.RegularExpressions;
using ExileCore.PoEMemory.Components;
using GameOffsets.Native;

namespace Radar;

public static class Extensions
{
	public static Vector3 GridPos(this Render render)
	{
		return render.PosNum / 10.869565f;
	}

	public static Vector2 ToSdx(this Vector2 v)
	{
		return new Vector2(v.X, v.Y);
	}

	public static Vector2i Truncate(this Vector2 v)
	{
		return new Vector2i((int)v.X, (int)v.Y);
	}

	public static IEnumerable<T> GetEveryNth<T>(this IEnumerable<T> source, int n)
	{
		int j = 0;
		foreach (T item in source)
		{
			if (j == 0)
			{
				yield return item;
			}
			j++;
			j %= n;
		}
	}

	public static bool Like(this string str, string pattern)
	{
		return new Regex("^" + Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".") + "$", RegexOptions.IgnoreCase | RegexOptions.Singleline).IsMatch(str);
	}
}
