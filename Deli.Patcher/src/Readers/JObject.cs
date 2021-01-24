using System.IO;
using Deli.VFS;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Deli.Patcher.Readers
{
	public class JObjectImmediateReader : IImmediateReader<JObject>
	{
		public JObject Read(IFileHandle handle)
		{
			using var raw = handle.OpenRead();
			using var text = new StreamReader(raw);
			using var json = new JsonTextReader(text);

			return JObject.Load(json);
		}
	}
}
