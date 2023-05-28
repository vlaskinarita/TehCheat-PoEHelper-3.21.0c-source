using System;
using ExileCore.Shared.Enums;

namespace ExileCore.Shared.SomeMagic;

public struct MemoryBasicInformation
{
	public IntPtr BaseAddress;

	public IntPtr AllocationBase;

	public MemoryProtectionType AllocationProtect;

	public IntPtr RegionSize;

	public MemoryAllocationState State;

	public MemoryProtectionType Protect;

	public MemoryAllocationType Type;
}
