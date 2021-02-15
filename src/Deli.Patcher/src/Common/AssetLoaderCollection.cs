using System;
using System.Text.RegularExpressions;

namespace Deli
{
	// Fixes static member in generic class warning
	// Have 1 regex instance rather than however many loaders there are
	internal static class AssetLoaderCollection
	{
		public static readonly Regex NameFilter = new(@"^[a-z0-9\._]+$");
	}

	/// <summary>
	///		A collection of asset loaders, each keyed by the mod that is providing it and its name
	/// </summary>
	/// <typeparam name="TLoader">The type of asset loader</typeparam>
	public sealed class AssetLoaderCollection<TLoader> : NestedServiceCollection<Mod, string, TLoader> where TLoader : Delegate
	{
		/// <summary>
		///		Gets or sets an asset loader
		/// </summary>
		/// <param name="source">The mod that is providing the asset loader</param>
		/// <param name="name">The name of the asset loader</param>
		/// <exception cref="ArgumentException">The name does not conform to the requirements: lowercase alphanumeric, '.' and '_' allowed</exception>
		public override TLoader this[Mod source, string name]
		{
			get => base[source, name];
			set
			{
				if (!AssetLoaderCollection.NameFilter.IsMatch(name))
				{
					throw new ArgumentException("Name must be lowercase alphanumeric, permitting '.' and '_' otherwise", nameof(name));
				}

				base[source, name] = value;
			}
		}
	}
}
