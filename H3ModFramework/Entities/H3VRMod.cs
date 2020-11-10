using BepInEx.Logging;
using UnityEngine;

namespace H3ModFramework
{
    public abstract class H3VRMod : MonoBehaviour
    {
        public ManualLogSource Logger;
        public ModInfo BaseMod;
    }
}