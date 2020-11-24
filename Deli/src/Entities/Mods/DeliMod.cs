using UnityEngine;

namespace Deli
{
	/// <summary>
	///		Base class for plugin mods
	/// </summary>
	public abstract class DeliMod : MonoBehaviour
	{
		public Mod BaseMod;

		protected DeliMod()
		{
			BaseMod = Deli.Services.Get<Mod, DeliMod>(this).Expect("Could not acquire mod handle for " + GetType());
		}
	}
}
