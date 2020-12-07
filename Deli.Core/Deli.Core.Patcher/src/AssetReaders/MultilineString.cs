using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Deli.Core
{
	[QuickUnnamedBind]
	public class MultilineStringAssetReader : IAssetReader<IEnumerable<string>>
	{
		public IEnumerable<string> ReadAsset(byte[] raw)
		{
			using var memory = new MemoryStream(raw);
			using var text = new StreamReader(memory, Encoding.UTF8);

			while (!text.EndOfStream)
			{
				yield return text.ReadLine();
			}
		}
	}
}
