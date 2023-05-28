using System.Runtime.InteropServices;
using GameOffsets.Native;

namespace GameOffsets;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct ActorAnimationListOffsets
{
	[FieldOffset(224)]
	public NativePtrArray AnimationList;
}
