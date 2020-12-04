using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using ADepIn;

namespace Deli.Core
{
	[QuickNamedBind("assembly.native")]
	public class NativeAssemblyAssetLoader : IAssetLoader
	{
		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool SetDllDirectory(string path);

		private readonly DirectoryInfo _nativesRoot;
		private readonly Dictionary<Mod, DirectoryInfo> _modNativePairs;

		// I would use the mod GUID over a ticket-system, but the mod GUID could contain invalid characters for the filesystem.
		private int _ticket;

		public NativeAssemblyAssetLoader()
		{
			_nativesRoot = new DirectoryInfo(Path.GetTempFileName() + ".d");
			_modNativePairs = new Dictionary<Mod, DirectoryInfo>();

			_nativesRoot.Create();
		}

		public void LoadAsset(IServiceKernel kernel, Mod mod, string path)
		{
			var raw = mod.Resources.Get<byte[]>(path).Expect("Failed to find native assembly at: " + path);

			var modNatives = _modNativePairs.GetOrInsertWith(mod, () =>
			{
				// Create directory for natives
				var dir = _nativesRoot.CreateSubdirectory(_ticket++.ToString());
				var dirFull = dir.FullName;

				// Add to Window's native DLL search (this also causes DLL injection, but that's kinda our point).
				if (!SetDllDirectory(dirFull))
				{
					var err = Marshal.GetLastWin32Error();
					throw new Win32Exception(err, "Failed to add native DLL directory at " + dirFull);
				}

				return dir;
			});

			// Path to DLL on disk
			var dest = Path.Combine(modNatives.FullName, Path.GetFileName(path));

			// Write packed DLL to disk DLL
			File.WriteAllBytes(dest, raw);
		}
	}
}
