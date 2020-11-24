using UnityEngine;

namespace Deli.Core
{
	[QuickUnnamedBind]
	public class AssetBundleAssetReader : IAssetReader<AssetBundle>
	{
		public AssetBundle ReadAsset(byte[] raw)
		{
			return AssetBundle.LoadFromMemory(raw);
		}
	}
}
