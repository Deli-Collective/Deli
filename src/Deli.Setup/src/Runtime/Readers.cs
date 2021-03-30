using System.IO;
using BepInEx.Logging;
using Deli.Immediate;
using Deli.Runtime.Yielding;
using Deli.VFS;
using Deli.VFS.Disk;
using UnityEngine;

namespace Deli.Runtime
{
	internal static class Readers
	{
		public static void AddBuiltins(ReaderCollection readers)
		{
			readers.Add(BytesOf);
			readers.Add(AssetBundleOf);
			readers.Add(Texture2DOf);
		}

		private static ResultYieldInstruction<byte[]> BytesOf(IFileHandle file)
		{
			var stream = file.OpenRead();
			var buffer = new byte[stream.Length];

			return new AsyncYieldInstruction<Stream>(stream, (self, callback, state) => self.BeginRead(buffer, 0, buffer.Length, callback, state),
				(self, result) => self.EndRead(result)).CallbackWith(() =>
			{
				stream.Dispose();
				return buffer;
			});
		}

		private static ResultYieldInstruction<AssetBundle> AssetBundleOf(IFileHandle file)
		{
			if (file is IDiskHandle disk)
			{
				return new AsyncOperationYieldInstruction<AssetBundleCreateRequest, AssetBundle>(AssetBundle.LoadFromFileAsync(disk.PathOnDisk),
					operation => operation.assetBundle);
			}

			return BytesOf(file).ContinueWith(AssetBundle.LoadFromMemoryAsync, r => r.assetBundle);
		}

		private static ResultYieldInstruction<Texture2D> Texture2DOf(IFileHandle file)
		{
			return BytesOf(file).CallbackWith(bytes =>
			{
				var tex = new Texture2D(0, 0);
				tex.LoadImage(bytes);

				return tex;
			});
		}
	}
}
