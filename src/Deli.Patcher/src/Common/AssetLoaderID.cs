using System;

namespace Deli
{
	/// <summary>
	///		Represents the identity of an asset loader
	/// </summary>
	public readonly struct AssetLoaderID
	{
		/// <summary>
		///		The GUID of the mod that the asset loader is from
		/// </summary>
		public string Mod { get; }
		/// <summary>
		///		The name of the asset loader itself
		/// </summary>
		public string Name { get; }

		/// <summary>
		///		Creates an instance of <see cref="AssetLoaderID"/>.
		/// </summary>
		/// <param name="mod">The GUID of the mod that the asset loader is from</param>
		/// <param name="name">The name of the asset loader itself</param>
		public AssetLoaderID(string mod, string name)
		{
			Mod = mod;
			Name = name;
		}

		/// <inheritdoc cref="ValueType.ToString"/>
		public override string ToString()
		{
			return Mod + ":" + Name;
		}
	}
}
