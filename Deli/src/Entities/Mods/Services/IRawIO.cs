using ADepIn;

namespace Deli
{
	/// <summary>
	///		Represents a direct handle to paths.
	/// </summary>
	public interface IRawIO : IFindableIO
	{
		/// <summary>
		///		Returns Some with data if the data exists at that path, otherwise None.
		/// </summary>
		Option<byte[]> this[string path] { get; }
	}
}
