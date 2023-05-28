using System;
using ExileCore.Shared.Cache;
using GameOffsets;

namespace ExileCore.PoEMemory.Components;

public class Buff : RemoteMemoryObject
{
	private readonly Lazy<string> _name;

	private BuffOffsets? _offsets;

	private readonly CachedValue<BuffOffsets> _cachedValue;

	private readonly CachedValue<BuffDat> _cachedValueBuffData;

	public BuffOffsets BuffOffsets
	{
		get
		{
			BuffOffsets? buffOffsets = (_offsets = _offsets ?? base.M.Read<BuffOffsets>(base.Address));
			return buffOffsets.Value;
		}
	}

	public string Name => _name.Value;

	[Obsolete("Use BuffCharges instead")]
	public byte Charges => (byte)BuffOffsets.Charges;

	public ushort BuffCharges => BuffOffsets.Charges;

	public ushort BuffStacks => base.M.Read<ushort>(base.M.Read<long>(base.Address + 248));

	public string DisplayName => base.M.ReadStringU(_cachedValueBuffData.Value.DisplayName);

	public string Description => base.M.ReadStringU(_cachedValueBuffData.Value.Description);

	public bool IsInvisible => base.M.Read<bool>(_cachedValueBuffData.Value.IsInvisible);

	public bool IsRemovable => base.M.Read<bool>(_cachedValueBuffData.Value.IsRemovable);

	public float MaxTime => BuffOffsets.MaxTime;

	public float Timer => BuffOffsets.Timer;

	public Buff()
	{
		_cachedValue = new FramesCache<BuffOffsets>(() => base.M.Read<BuffOffsets>(base.Address), 3u);
		_cachedValueBuffData = new FramesCache<BuffDat>(() => base.M.Read<BuffDat>(_cachedValue.Value.BuffDatPtr), 3u);
		_name = new Lazy<string>(delegate
		{
			string text = $"{"Buff"}{_cachedValueBuffData.Value.Name}";
			int num = 0;
			string text2;
			do
			{
				text2 = RemoteMemoryObject.Cache.StringCache.Read(text, () => base.M.ReadStringU(_cachedValueBuffData.Value.Name));
				if (text2 == string.Empty)
				{
					RemoteMemoryObject.Cache.StringCache.Remove(text);
				}
				num++;
			}
			while (text2 == string.Empty && num < 7);
			return text2;
		});
	}

	public override string ToString()
	{
		return $"{DisplayName}({Name}) - Charges: {BuffCharges} MaxTime: {MaxTime}, BuffStacks: {BuffStacks}";
	}
}
