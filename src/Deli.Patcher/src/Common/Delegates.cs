using Deli.VFS;

namespace Deli
{
	/// <summary>
	///		A method which reads or mutates a stage
	/// </summary>
	/// <param name="stage">The stage to read or mutate</param>
	/// <typeparam name="TStage">The type of stage being ran</typeparam>
	public delegate void StageRunner<in TStage>(TStage stage) where TStage : Stage;

	/// <summary>
	///		A deserializer that operates using a single method call.
	/// </summary>
	/// <typeparam name="TOut">The type to deserialize to.</typeparam>
	public delegate TOut Reader<out TOut>(IFileHandle file) where TOut : notnull;

	/// <summary>
	///		An asset loader.
	/// </summary>
	/// <typeparam name="TStage">The type of <see cref="Stage"/> that this asset loader supports.</typeparam>
	/// <typeparam name="TOut">The return value of the asset loader, or <see cref="Empty"/> if it has none.</typeparam>
	public delegate TOut AssetLoader<in TStage, out TOut>(TStage stage, Mod mod, IHandle handle) where TStage : Stage where TOut : notnull;
}
