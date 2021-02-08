using Deli.VFS;

namespace Deli.Immediate
{
	/// <summary>
	///		A deserializer that operates using a single method call.
	/// </summary>
	/// <typeparam name="T">The type to deserialize to.</typeparam>
	public delegate T ImmediateReader<out T>(IFileHandle file) where T : notnull;

	/// <summary>
	///		An asset loader that completes in a singular method call.
	/// </summary>
	/// <typeparam name="TStage">The type of <see cref="Stage"/> that this asset loader supports.</typeparam>
	public delegate void ImmediateAssetLoader<in TStage>(TStage stage, Mod mod, IHandle handle) where TStage : Stage;
}
