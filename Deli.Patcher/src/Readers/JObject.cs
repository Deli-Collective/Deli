using System.IO;
using Deli.VFS;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Deli.Patcher.Readers
{
	/// <summary>
	///		An <see cref="IImmediateReader{T}"/> that reads <see cref="JObject"/>s.
	/// </summary>
	public sealed class JObjectImmediateReader : IImmediateReader<JObject>
	{
		/// <inheritdoc cref="IImmediateReader{T}.Read"/>
		public JObject Read(IFileHandle handle)
		{
			using var raw = handle.OpenRead();
			using var text = new StreamReader(raw);
			using var json = new JsonTextReader(text);

			return JObject.Load(json);
		}
	}
}
