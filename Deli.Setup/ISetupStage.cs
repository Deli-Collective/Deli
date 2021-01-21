namespace Deli
{
	public interface ISetupStage
	{
		void AddAssetLoader(string name, ICoroutineAssetLoader loader);

		void AddResourceReader<T>(ICoroutineResourceReader<T> reader);
	}
}
