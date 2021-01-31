namespace Deli
{
	public delegate void StageRunner<in TStage>(TStage stage) where TStage : Stage;
}
