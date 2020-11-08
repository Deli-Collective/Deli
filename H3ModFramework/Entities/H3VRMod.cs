namespace H3ModFramework
{
    public abstract class H3VRMod
    {
        /// <summary>
        /// This is called by the default Mod Loader to initialize this mod. The order mods are initialized in is not guaranteed to be constant.
        /// </summary>
        public virtual void Awake()
        {
        }

        /// <summary>
        /// This is called by the default Mod Loader after all mods have had their Awake() method called
        /// </summary>
        public virtual void Start()
        {
        }
    }
}