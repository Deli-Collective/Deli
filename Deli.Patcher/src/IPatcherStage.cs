using System;

namespace Deli.Patcher
{
	public interface IPatcherStage : IStage
	{
		IDisposable AddAssetLoader(string name, IImmediateAssetLoader loader);

		void AddPatcher(string fileName, IPatcher patcher);
	}
}
