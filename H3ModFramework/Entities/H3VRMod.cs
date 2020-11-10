using BepInEx.Logging;
using UnityEngine;

namespace H3ModFramework
{
    /// <summary>
    /// Base class for plugin mods
    /// </summary>
    public abstract class H3VRMod : MonoBehaviour
    {
        public ModInfo BaseMod;
        public ManualLogSource Logger;
    }
}