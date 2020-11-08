using System;
using BepInEx.Logging;

namespace H3ModFramework
{
    public class H3VRMod
    {
        protected ManualLogSource Logger { get; private set; }
        protected ModInfo BaseMod { get; private set; }
        
        public H3VRMod(ModInfo mod, ManualLogSource logSource)
        {
            Logger = logSource;
            BaseMod = mod;
        }

        /// <summary>
        /// This is called by the default Mod Loader after all mods have been constructed
        /// </summary>
        public virtual void Start()
        {
        }
    }
}