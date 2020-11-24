using UnityEngine;

namespace Deli.Core
{
	[QuickUnnamedBind]
	public class TextureAssetReader : IAssetReader<Texture2D>
	{
		public Texture2D ReadAsset(byte[] raw)
		{
			var tex = new Texture2D(0, 0);
			tex.LoadImage(raw);
			return tex;
		}
	}
}
