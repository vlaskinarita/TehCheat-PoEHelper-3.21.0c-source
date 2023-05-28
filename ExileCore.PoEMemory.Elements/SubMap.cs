using System;
using System.Numerics;
using SharpDX;

namespace ExileCore.PoEMemory.Elements;

public class SubMap : Element
{
	[Obsolete]
	public SharpDX.Vector2 Shift => base.M.Read<SharpDX.Vector2>(base.Address + 616);

	public System.Numerics.Vector2 ShiftNum => base.M.Read<System.Numerics.Vector2>(base.Address + 616);

	[Obsolete]
	public SharpDX.Vector2 DefaultShift => base.M.Read<SharpDX.Vector2>(base.Address + 624);

	public System.Numerics.Vector2 DefaultShiftNum => base.M.Read<System.Numerics.Vector2>(base.Address + 624);

	public float Zoom => base.M.Read<float>(base.Address + 684);
}
