using System.IO;
using BepInEx.Logging;
using Deli.Runtime.Yielding;
using Deli.VFS;
using UnityEngine;

namespace Deli.Runtime
{
	internal static class Readers
	{
		public static DelayedReaderCollection DefaultCollection(ManualLogSource logger) => new(logger)
		{
			BytesOf,
			AssetBundleOf,
			Texture2DOf
		};

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
