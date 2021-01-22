using System;

namespace Deli.Setup
{
	public interface ISetupStage : IStage
	{

		CoroutineReaderCollection CoroutineReaders { get; }

		IDisposable AddAssetLoader(string name, ICoroutineAssetLoader loader);

		ICoroutineReader<T> GetReader<T>();
	}
}
