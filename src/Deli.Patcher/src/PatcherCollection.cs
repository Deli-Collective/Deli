using System;

namespace Deli.Patcher
{
	/// <summary>
	///		A collection of patchers, each keyed by the name of the DLL it is patching and the mod that is providing it
	/// </summary>
	public sealed class PatcherCollection : NestedServiceCollection<string, Mod, Patcher>
	{
		// Just an adjustment of signature, to make the XML docs more helpful
		/// <summary>
		///		Gets or sets a patcher
		/// </summary>
		/// <param name="target">The DLL to patch</param>
		/// <param name="source">The mod that is providing the patcher</param>
		public override Patcher this[string target, Mod source]
		{
			get => base[target, source];
			set => base[target, source] = value;
		}

		/// <summary>
		///		Sets a patcher if it was not present, otherwise adds a patcher to the existing patcher
		/// </summary>
		/// <param name="target">The DLL to patch</param>
		/// <param name="source">The mod that is providing the patcher</param>
		/// <param name="patcher">The patcher itself</param>
		public void SetOrAdd(string target, Mod source, Patcher patcher)
		{
			if (TryGet(target, source, out var existing))
			{
				patcher = (Patcher) Delegate.Combine(existing, patcher);
			}

			this[target, source] = patcher;
		}
	}
}
