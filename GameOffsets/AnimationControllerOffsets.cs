using System.Runtime.InteropServices;
using GameOffsets.Native;

namespace GameOffsets;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct AnimationControllerOffsets
{
	[FieldOffset(24)]
	public NativePtrArray ActiveAnimationsArrayPtr;

	[FieldOffset(352)]
	public long ActorAnimationArrayPtr;

	[FieldOffset(372)]
	public int AnimationInActorId;

	[FieldOffset(388)]
	public float AnimationProgress;

	[FieldOffset(392)]
	public int CurrentAnimationStage;

	[FieldOffset(396)]
	public float NextAnimationPoint;

	[FieldOffset(400)]
	public float AnimationSpeedMultiplier1;

	[FieldOffset(440)]
	public float AnimationSpeedMultiplier2;

	[FieldOffset(408)]
	public float MaxAnimationProgressOffset;

	[FieldOffset(412)]
	public float MaxAnimationProgress;
}
