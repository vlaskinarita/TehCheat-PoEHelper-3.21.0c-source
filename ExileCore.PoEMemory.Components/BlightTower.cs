using System.IO;

namespace ExileCore.PoEMemory.Components;

public class BlightTower : Component
{
	private string _id;

	private string _name;

	private string _icon;

	private string _iconFileName;

	private long? _datInfo;

	private long DatInfo
	{
		get
		{
			long? datInfo = _datInfo;
			if (!datInfo.HasValue)
			{
				long? num = (_datInfo = base.M.Read<long>(base.Address + 40));
				return num.Value;
			}
			return datInfo.GetValueOrDefault();
		}
	}

	public string Id => _id ?? (_id = base.M.ReadStringU(base.M.Read<long>(DatInfo)));

	public string Name => _name ?? (_name = base.M.ReadStringU(base.M.Read<long>(DatInfo + 8)));

	public string Icon => _icon ?? (_icon = base.M.ReadStringU(base.M.Read<long>(DatInfo + 24)));

	public string IconFileName => _iconFileName ?? (_iconFileName = Path.GetFileNameWithoutExtension(Icon));
}
