using BepInEx.Logging;
using UnityEngine;

namespace Deli
{
    /// <summary>
    /// Base class for plugin mods
    /// </summary>
    public abstract class DeliMod : MonoBehaviour
    {
        public ModInfo BaseMod;
        public ManualLogSource Logger;
    }
}