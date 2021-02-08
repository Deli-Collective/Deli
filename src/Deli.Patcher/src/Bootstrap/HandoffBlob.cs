using System.Collections.Generic;

namespace Deli.Bootstrap
{
	public readonly struct HandoffBlob
	{
		public Stage.Blob StageData { get; }
		public IEnumerable<Mod> Mods { get; }

		public HandoffBlob(Stage.Blob stageData, IEnumerable<Mod> mods)
		{
			StageData = stageData;
			Mods = mods;
		}
	}
}
