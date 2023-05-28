using System.IO;
using SharpDX;

namespace ExileCore.Shared.AtlasHelper;

public class AtlasTexture
{
	public string TextureName { get; }

	public string AtlasFilePath { get; }

	public string AtlasFileName { get; }

	public RectangleF TextureUV { get; }

	internal AtlasTexture(string textureName, RectangleF textureUv, string atlasFilePath)
	{
		TextureName = textureName;
		TextureUV = textureUv;
		AtlasFilePath = atlasFilePath;
		AtlasFileName = Path.GetFileName(atlasFilePath);
	}
}
