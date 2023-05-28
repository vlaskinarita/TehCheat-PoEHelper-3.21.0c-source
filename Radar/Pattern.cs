using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ExileCore.Shared.Interfaces;

namespace Radar;

public class Pattern : IPattern
{
	private string _mask;

	public string Name { get; }

	public byte[] Bytes { get; }

	public bool[] Mask { get; }

	public int StartOffset => 0;

	public int PatternOffset { get; }

	string IPattern.Mask => _mask ?? (_mask = new string(Mask.Select((bool x) => (!x) ? '?' : 'x').ToArray()));

	public Pattern(string pattern, string name)
	{
		List<string> arr = pattern.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
		int patternOffset = arr.FindIndex((string x) => x == "^");
		if (patternOffset == -1)
		{
			patternOffset = 0;
		}
		else
		{
			arr.RemoveAt(patternOffset);
		}
		PatternOffset = patternOffset;
		Bytes = arr.Select((string x) => (byte)((!(x == "??")) ? byte.Parse(x, NumberStyles.HexNumber) : 0)).ToArray();
		Mask = arr.Select((string x) => x != "??").ToArray();
		Name = name;
		while (!Mask[0])
		{
			int patternOffset2 = PatternOffset;
			PatternOffset = patternOffset2 - 1;
			Mask = Mask.Skip(1).ToArray();
			Bytes = Bytes.Skip(1).ToArray();
		}
		while (!Mask[^1])
		{
			Mask = Mask.SkipLast(1).ToArray();
			Bytes = Bytes.SkipLast(1).ToArray();
		}
	}
}
