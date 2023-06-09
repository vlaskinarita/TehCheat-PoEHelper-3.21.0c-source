using System.Runtime.InteropServices;
using GameOffsets.Native;

namespace GameOffsets;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct EntityOffsets
{
	[FieldOffset(8)]
	public ObjectHeaderOffsets Head;

	[FieldOffset(8)]
	public long EntityDetailsPtr;

	[FieldOffset(16)]
	public StdVector ComponentList;

	public override string ToString()
	{
		return $"Head: {Head} ComponentList:{ComponentList}";
	}
}
