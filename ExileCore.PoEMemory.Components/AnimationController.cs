using System;
using System.Linq;
using ExileCore.Shared.Cache;
using GameOffsets;

namespace ExileCore.PoEMemory.Components;

public class AnimationController : StructuredRemoteMemoryObject<AnimationControllerOffsets>
{
	private readonly CachedValue<Func<float, float>> _progressTransformFunc;

	private readonly CachedValue<float> _animationSpeed;

	private readonly CachedValue<SupportedAnimationList> _supportedAnimationList;

	private ActiveAnimationData ActiveAnimationData => base.M.ReadStdVector<long>(base.Structure.ActiveAnimationsArrayPtr).Select(base.ReadObject<ActiveAnimationData>).LastOrDefault((ActiveAnimationData x) => x.AnimationId == CurrentAnimationId);

	public float MaxRawAnimationProgress => base.Structure.MaxAnimationProgress - base.Structure.MaxAnimationProgressOffset;

	public float RawNextAnimationPoint => base.Structure.NextAnimationPoint;

	public float RawAnimationProgress => base.Structure.AnimationProgress;

	public float RawAnimationSpeed => base.Structure.AnimationSpeedMultiplier1 * base.Structure.AnimationSpeedMultiplier2;

	public float TransformedMaxRawAnimationProgress => TransformProgress(MaxRawAnimationProgress);

	public float TransformedRawNextAnimationPoint => TransformProgress(RawNextAnimationPoint);

	public float TransformedRawAnimationProgress => TransformProgress(RawAnimationProgress);

	public float AnimationSpeed => _animationSpeed.Value;

	public SupportedAnimationList SupportedAnimationList => _supportedAnimationList.Value;

	public int CurrentAnimationId => base.Structure.AnimationInActorId;

	public int CurrentAnimationStage => base.Structure.CurrentAnimationStage;

	public AnimationStageList CurrentAnimation
	{
		get
		{
			if (CurrentAnimationId < 0 || CurrentAnimationId >= SupportedAnimationList.Animations.Count)
			{
				throw new ArgumentOutOfRangeException("CurrentAnimationId", CurrentAnimationId, $"There's only {SupportedAnimationList.Animations.Count} animations");
			}
			return SupportedAnimationList.Animations[CurrentAnimationId];
		}
	}

	public float NextAnimationPoint => TransformedRawNextAnimationPoint / TransformedMaxRawAnimationProgress;

	public float AnimationProgress => TransformedRawAnimationProgress / TransformedMaxRawAnimationProgress;

	public TimeSpan AnimationCompletesIn
	{
		get
		{
			float num = (TransformedMaxRawAnimationProgress - TransformedRawAnimationProgress) / AnimationSpeed;
			return TimeSpan.FromSeconds((!float.IsNaN(num) && !float.IsInfinity(num)) ? num : 1f);
		}
	}

	public TimeSpan AnimationActiveFor
	{
		get
		{
			float num = TransformedRawAnimationProgress / AnimationSpeed;
			return TimeSpan.FromSeconds((!float.IsNaN(num) && !float.IsInfinity(num)) ? num : 0f);
		}
	}

	public AnimationController()
	{
		_progressTransformFunc = KeyTrackingCache.Create(() => ActiveAnimationData?.TransformRawProgressFunc ?? ((Func<float, float>)((float f) => f)), () => (CurrentAnimationId, CurrentAnimationStage));
		_animationSpeed = KeyTrackingCache.Create(() => ActiveAnimationData?.AnimationSpeed ?? RawAnimationSpeed, () => (CurrentAnimationId, CurrentAnimationStage));
		_supportedAnimationList = new TimeCache<SupportedAnimationList>(() => GetObject<SupportedAnimationList>(base.Structure.ActorAnimationArrayPtr), 1000L);
	}

	public float TransformProgress(float progress)
	{
		return _progressTransformFunc.Value(progress);
	}
}
