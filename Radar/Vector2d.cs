using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace Radar;

public readonly record struct Vector2d
{
	public double Length => Math.Sqrt(X * X + Y * Y);

	public readonly double X;

	public readonly double Y;

	public Vector2d(double X, double Y)
	{
		this.X = X;
		this.Y = Y;
	}

	public static Vector2d operator -(Vector2d v1, Vector2d v2)
	{
		return new Vector2d(v1.X - v2.X, v1.Y - v2.Y);
	}

	public static Vector2d operator +(Vector2d v1, Vector2d v2)
	{
		return new Vector2d(v1.X + v2.X, v1.Y + v2.Y);
	}

	public static Vector2d operator /(Vector2d v, double d)
	{
		return new Vector2d(v.X / d, v.Y / d);
	}

	[CompilerGenerated]
	private bool PrintMembers(StringBuilder builder)
	{
		builder.Append("X = ");
		builder.Append(X.ToString());
		builder.Append(", Y = ");
		builder.Append(Y.ToString());
		builder.Append(", Length = ");
		builder.Append(Length.ToString());
		return true;
	}

	[CompilerGenerated]
	public void Deconstruct(out double X, out double Y)
	{
		X = this.X;
		Y = this.Y;
	}
}
