using System;
using System.Numerics;
using ExileCore.RenderQ;
using ExileCore.Shared.AtlasHelper;
using ExileCore.Shared.Enums;
using ExileCore.Shared.Helpers;
using ImGuiNET;
using SharpDX;

namespace ExileCore;

public class Graphics
{
	private record SetTextScaleDisposable(ImGuiRender Render, float OldScale) : IDisposable
	{
		public void Dispose()
		{
			Render.TextScale = OldScale;
		}
	}

	private static readonly RectangleF DefaultUV = new RectangleF(0f, 0f, 1f, 1f);

	private readonly CoreSettings _settings;

	private readonly ImGuiRender ImGuiRender;

	private readonly DX11 _lowLevel;

	[Obsolete]
	public DX11 LowLevel => _lowLevel;

	public FontContainer Font => ImGuiRender.CurrentFont;

	public FontContainer LastFont => ImGuiRender.CurrentFont;

	public Graphics(DX11 dx11, CoreSettings settings)
	{
		_lowLevel = dx11;
		_settings = settings;
		ImGuiRender = dx11.ImGuiRender;
	}

	public System.Numerics.Vector2 DrawText(string text, System.Numerics.Vector2 position, Color color, int height)
	{
		return DrawText(text, position, color, height, _settings.Font);
	}

	public System.Numerics.Vector2 DrawText(string text, System.Numerics.Vector2 position, Color color, FontAlign align)
	{
		return DrawText(text, position, color, _settings.FontSize, _settings.Font, align);
	}

	public System.Numerics.Vector2 DrawText(string text, System.Numerics.Vector2 position, Color color, string fontName, FontAlign align)
	{
		return ImGuiRender.DrawText(text, position, color, -1, fontName, align);
	}

	public System.Numerics.Vector2 DrawText(string text, System.Numerics.Vector2 position, Color color, int height, FontAlign align)
	{
		return ImGuiRender.DrawText(text, position, color, height, _settings.Font, align);
	}

	public System.Numerics.Vector2 DrawText(string text, System.Numerics.Vector2 position, Color color, int height, string fontName, FontAlign align = FontAlign.Left)
	{
		return ImGuiRender.DrawText(text, position, color, height, fontName, align);
	}

	public System.Numerics.Vector2 DrawText(string text, System.Numerics.Vector2 position, Color color)
	{
		return DrawText(text, position, color, _settings.FontSize, _settings.Font);
	}

	public System.Numerics.Vector2 DrawText(string text, System.Numerics.Vector2 position)
	{
		return DrawText(text, position, Color.White);
	}

	public System.Numerics.Vector2 DrawText(string text, System.Numerics.Vector2 position, FontAlign align)
	{
		return DrawText(text, position, Color.White, _settings.FontSize, align);
	}

	public IDisposable SetTextScale(float textScale)
	{
		float textScale2 = ImGuiRender.TextScale;
		ImGuiRender.TextScale *= textScale;
		return new SetTextScaleDisposable(ImGuiRender, textScale2);
	}

	public System.Numerics.Vector2 MeasureText(string text)
	{
		return ImGuiRender.MeasureText(text);
	}

	public System.Numerics.Vector2 MeasureText(string text, int height)
	{
		return ImGuiRender.MeasureText(text, height);
	}

	public void DrawLine(System.Numerics.Vector2 p1, System.Numerics.Vector2 p2, float borderWidth, Color color)
	{
		ImGuiRender.LowLevelApi.AddLine(p1, p2, color.ToImgui(), borderWidth);
	}

	public void DrawFrame(System.Numerics.Vector2 p1, System.Numerics.Vector2 p2, Color color, float rounding, int thickness, int flags)
	{
		ImGuiRender.LowLevelApi.AddRect(p1, p2, color.ToImgui(), rounding, (ImDrawFlags)flags, thickness);
	}

	public void DrawBox(System.Numerics.Vector2 p1, System.Numerics.Vector2 p2, Color color, float rounding = 0f)
	{
		ImGuiRender.LowLevelApi.AddRectFilled(p1, p2, color.ToImgui(), rounding);
	}

	public void DrawQuad(IntPtr textureId, System.Numerics.Vector2 a, System.Numerics.Vector2 b, System.Numerics.Vector2 c, System.Numerics.Vector2 d)
	{
		ImGuiRender.LowLevelApi.AddImageQuad(textureId, a, b, c, d);
	}

	public void DrawImage(string fileName, RectangleF rectangle)
	{
		DrawImage(fileName, rectangle, DefaultUV, Color.White);
	}

	public void DrawImage(string fileName, RectangleF rectangle, Color color)
	{
		DrawImage(fileName, rectangle, DefaultUV, color);
	}

	public void DrawImage(string fileName, RectangleF rectangle, RectangleF uv, Color color)
	{
		try
		{
			ImGuiRender.DrawImage(fileName, rectangle, uv, color);
		}
		catch (Exception ex)
		{
			DebugWindow.LogError(ex.ToString());
		}
	}

	public void DrawImage(string fileName, RectangleF rectangle, RectangleF uv)
	{
		DrawImage(fileName, rectangle, uv, Color.White);
	}

	public void DrawImage(AtlasTexture atlasTexture, RectangleF rectangle)
	{
		DrawImage(atlasTexture, rectangle, Color.White);
	}

	public void DrawImage(AtlasTexture atlasTexture, RectangleF rectangle, Color color)
	{
		DrawImage(atlasTexture.AtlasFileName, rectangle, atlasTexture.TextureUV, color);
	}

	public void DrawImageGui(string fileName, RectangleF rectangle, RectangleF uv)
	{
		ImGuiRender.DrawImage(fileName, rectangle, uv);
	}

	public void DrawImageGui(string fileName, System.Numerics.Vector2 TopLeft, System.Numerics.Vector2 BottomRight, System.Numerics.Vector2 TopLeft_UV, System.Numerics.Vector2 BottomRight_UV)
	{
		ImGuiRender.DrawImage(fileName, TopLeft, BottomRight, TopLeft_UV, BottomRight_UV);
	}

	public void DrawBox(RectangleF rect, Color color)
	{
		DrawBox(rect, color, 0f);
	}

	public void DrawBox(RectangleF rect, Color color, float rounding)
	{
		DrawBox(rect.TopLeft.ToVector2Num(), rect.BottomRight.ToVector2Num(), color, rounding);
	}

	public void DrawFrame(RectangleF rect, Color color, float rounding, int thickness, int flags)
	{
		DrawFrame(rect.TopLeft.ToVector2Num(), rect.BottomRight.ToVector2Num(), color, rounding, thickness, flags);
	}

	public void DrawFrame(RectangleF rect, Color color, int thickness)
	{
		DrawFrame(rect.TopLeft.ToVector2Num(), rect.BottomRight.ToVector2Num(), color, 0f, thickness, 0);
	}

	public void DrawFrame(System.Numerics.Vector2 p1, System.Numerics.Vector2 p2, Color color, int thickness)
	{
		DrawFrame(p1, p2, color, 0f, thickness, 0);
	}

	public bool InitImage(string name, bool textures = true)
	{
		string name2 = (textures ? ("textures/" + name) : name);
		return LowLevel.InitTexture(name2);
	}

	public IntPtr GetTextureId(string name)
	{
		return LowLevel.GetTexture(name);
	}

	public void DisposeTexture(string name)
	{
		LowLevel.DisposeTexture(name);
	}

	public IDisposable UseCurrentFont()
	{
		return ImGuiRender.UseCurrentFont();
	}

	[Obsolete]
	public System.Numerics.Vector2 DrawText(string text, SharpDX.Vector2 position, Color color, string fontName = null, FontAlign align = FontAlign.Left)
	{
		return ImGuiRender.DrawText(text, position.ToVector2Num(), color, -1, fontName, align);
	}

	[Obsolete]
	public System.Numerics.Vector2 DrawText(string text, SharpDX.Vector2 position, Color color)
	{
		return DrawText(text, position.ToVector2Num(), color, _settings.FontSize, _settings.Font);
	}

	[Obsolete]
	public System.Numerics.Vector2 DrawText(string text, SharpDX.Vector2 position, Color color, int height)
	{
		return DrawText(text, position.ToVector2Num(), color, height, _settings.Font);
	}

	[Obsolete]
	public System.Numerics.Vector2 DrawText(string text, SharpDX.Vector2 position, Color color, FontAlign align = FontAlign.Left)
	{
		return DrawText(text, position.ToVector2Num(), color, _settings.FontSize, _settings.Font, align);
	}

	[Obsolete]
	public System.Numerics.Vector2 DrawText(string text, SharpDX.Vector2 position, Color color, int height, FontAlign align = FontAlign.Left)
	{
		return DrawText(text, position.ToVector2Num(), color, height, _settings.Font, align);
	}

	[Obsolete]
	public System.Numerics.Vector2 DrawText(string text, SharpDX.Vector2 position, FontAlign align = FontAlign.Left)
	{
		return DrawText(text, position.ToVector2Num(), Color.White, _settings.FontSize, align);
	}

	[Obsolete]
	public void DrawLine(SharpDX.Vector2 p1, SharpDX.Vector2 p2, float borderWidth, Color color)
	{
		ImGuiRender.LowLevelApi.AddLine(p1.ToVector2Num(), p2.ToVector2Num(), color.ToImgui(), borderWidth);
	}

	[Obsolete]
	public void DrawBox(SharpDX.Vector2 p1, SharpDX.Vector2 p2, Color color, float rounding = 0f)
	{
		ImGuiRender.LowLevelApi.AddRectFilled(p1.ToVector2Num(), p2.ToVector2Num(), color.ToImgui(), rounding);
	}

	[Obsolete]
	public void DrawTexture(IntPtr user_texture_id, SharpDX.Vector2 a, SharpDX.Vector2 b, SharpDX.Vector2 c, SharpDX.Vector2 d)
	{
		ImGuiRender.LowLevelApi.AddImageQuad(user_texture_id, a.ToVector2Num(), b.ToVector2Num(), c.ToVector2Num(), d.ToVector2Num());
	}

	[Obsolete]
	public void DrawFrame(SharpDX.Vector2 p1, SharpDX.Vector2 p2, Color color, int thickness)
	{
		DrawFrame(p1.ToVector2Num(), p2.ToVector2Num(), color, 0f, thickness, 0);
	}
}
