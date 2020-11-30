using ADepIn;

namespace Deli
{
	/// <summary>
	///		Represents an object that performs an action on mod assets
	/// </summary>
	public interface IAssetLoader
	{
		/// <summary>
		/// 	Loads an asset from a mod asset table
		/// </summary>
		/// <param name="kernel">The Deli service kernel</param>
		/// <param name="mod">The mod that contains the asset</param>
		/// <param name="path">The path to the asset</param>
		void LoadAsset(IServiceKernel kernel, Mod mod, string path);
	}
}
