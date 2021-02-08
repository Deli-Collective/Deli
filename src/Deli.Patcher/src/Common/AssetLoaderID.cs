namespace Deli
{
	public readonly struct AssetLoaderID
	{
		public string Mod { get; }
		public string Name { get; }

		public AssetLoaderID(string mod, string name)
		{
			Mod = mod;
			Name = name;
		}

		public override string ToString()
		{
			return Mod + ":" + Name;
		}
	}
}
