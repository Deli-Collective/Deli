namespace Deli
{
	/// <summary>
	///		A piece of code from a mod that runs at a certain stage and any subsequent stages.
	/// </summary>
	public interface IDeliPlugin
	{
		/// <summary>
		///		Invoked when a stage is in progress.
		/// </summary>
		/// <param name="stage">The current stage.</param>
		void Run(Stage stage);
	}
}
