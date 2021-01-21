namespace Deli
{
	public interface IPatcherStage
	{
		void AddPatcher(string fileName, IPatcher patcher);
	}
}
