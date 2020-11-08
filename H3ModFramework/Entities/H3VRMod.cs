using System;
using BepInEx.Logging;

namespace H3ModFramework
{
    public abstract class H3VRMod
    {
        protected ManualLogSource Logger { get; private set; }
        
        public H3VRMod(ManualLogSource logSource)
        {
            Logger = logSource;
        }
        
        
        /// <summary>
        /// This is called by the default Mod Loader after all mods have been constructed
        /// </summary>
        public virtual void Start()
        {
        }
    }
}