namespace Deli
{
	/// <summary>
	///		A method which reads or mutates a stage
	/// </summary>
	/// <param name="stage">The stage to read or mutate</param>
	/// <typeparam name="TStage">The type of stage being ran</typeparam>
	public delegate void StageRunner<in TStage>(TStage stage) where TStage : Stage;
}
