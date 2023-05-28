using System;
using System.Collections.Generic;

namespace ExileCore.Shared.SomeMagic;

public class Pointer : IDisposable
{
	public IntPtr BaseAddress { get; }

	public List<int> Offsets { get; } = new List<int>();


	public Pointer(IntPtr baseAddress, params int[] offsets)
	{
		BaseAddress = baseAddress;
		foreach (int item in offsets)
		{
			Offsets.Add(item);
		}
	}

	public void Dispose()
	{
		GC.SuppressFinalize(this);
	}

	~Pointer()
	{
		Dispose();
	}
}
