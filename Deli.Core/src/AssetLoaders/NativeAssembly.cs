using System;
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
		private static extern IntPtr LoadLibrary(string path);

		private readonly DirectoryInfo _root;
		private readonly Dictionary<Mod, ModInfo> _dirs;

		// I would use the mod GUID over a ticket-system, but the mod GUID could contain invalid characters for the filesystem.
		private int _ticket;

		public NativeAssemblyAssetLoader()
		{
			// 0 B temp file on disk used to reserve handle
			var rootPath = Path.GetTempFileName();

			_dirs = new Dictionary<Mod, ModInfo>();
			_root = new DirectoryInfo(rootPath + ".d");

			_root.Create();
		}

		public void LoadAsset(IServiceKernel kernel, Mod mod, string path)
		{
			var raw = mod.Resources.Get<byte[]>(path).Expect("Failed to find native assembly at: " + path);

			var info = _dirs.GetOrInsertWith(mod, () => new ModInfo(_root, ref _ticket));
			info.LoadAssembly(raw, Path.GetFileName(path));
		}

		private class ModInfo
		{
			public readonly DirectoryInfo Assemblies;

			public int Ticket;

			public ModInfo(DirectoryInfo root, ref int ticket)
			{
				Assemblies = root.CreateSubdirectory(ticket++.ToString());
			}

			public void LoadAssembly(byte[] raw, string name)
			{
				// Path to DLL on disk
				var dest = Path.Combine(Assemblies.FullName, Ticket++ + "-" + name);

				// Write packed DLL to disk DLL
				File.WriteAllBytes(dest, raw);

				// Load disk DLL
				if (LoadLibrary(dest) == IntPtr.Zero)
				{
					var err = Marshal.GetLastWin32Error();
					throw new Win32Exception(err, "Failed to load native assembly at " + dest);
				}
			}
		}
	}
}