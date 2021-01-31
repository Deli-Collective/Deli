using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx.Logging;
using Deli.VFS;
using VDisk = Deli.VFS.Disk;
using VZip = Deli.VFS.Zip;
using Ionic.Zip;
using Filesystem = Deli.DeliConstants.Filesystem;

namespace Deli.Patcher.Bootstrap
{
	internal class Discovery
	{
		private readonly ManualLogSource _logger;
		private readonly DirectoryInfo _mods;
		private readonly ImmediateReader<Mod.Manifest> _manifestReader;

		public Discovery(ManualLogSource logger, ImmediateReader<Mod.Manifest> manifestReader)
		{
			_logger = logger;
			_manifestReader = manifestReader;
			_mods = Directory.CreateDirectory(Filesystem.ModsDirectory);
		}

		private Mod.Manifest CreateManifest(IDirectoryHandle resources)
		{
			if (resources[Filesystem.ManifestName] is not IFileHandle manifestFile)
			{
				throw new FileNotFoundException("Manifest file was not present.", Filesystem.ManifestName);
			}

			return _manifestReader(manifestFile);
		}

		private static IDirectoryHandle CreateZipResources(FileInfo file)
		{
			if (file.Length == 0)
			{
				// ZipFile already throws an exception, but it can't hurt to do this early with a concise message.
				throw new IOException("Zip file was 0 bytes long.");
			}

			var raw = file.OpenRead();
			var zip = ZipFile.Read(raw);
			var resources = VZip.RootDirectoryHandle.Create(zip);

			return resources;
		}

		private Mod CreateMod(IDirectoryHandle resources)
		{
			return new(CreateManifest(resources), resources);
		}

		private IEnumerable<Mod> DiscoverMods(DirectoryInfo dir)
		{
			var manifest = new FileInfo(Path.Combine(dir.FullName, Filesystem.ManifestName));
			if (manifest.Exists)
			{
				if (dir.FullName == Path.GetFullPath(Filesystem.ModsDirectory))
				{
					_logger.LogWarning("Ignoring misplaced manifest in the root of the mods directory. This manifest was probably not placed here intentionally and should be removed.");
				}
				else
				{
					Mod mod;
					try
					{
						var resources = new VDisk.RootDirectoryHandle(dir.FullName);
						mod = CreateMod(resources);
					}
					catch (Exception e)
					{
						_logger.LogError("Failed to create mod from directory mod at " + dir + Environment.NewLine + e);

						// Don't continue, as this directory was intended to be a mod.
						yield break;
					}

					yield return mod;
					yield break;
				}
			}

			foreach (var file in dir.GetFiles("*.deli"))
			{
				Mod mod;
				try
				{
					var resources = CreateZipResources(file);
					mod = CreateMod(resources);
				}
				catch (FileNotFoundException e)
				{
					if (e.FileName != Filesystem.ManifestName)
					{
						// Not our doing.
						throw;
					}

					_logger.LogError("A manifest file was not present in the zip mod at " + file);
					continue;
				}
				catch (Exception e)
				{
					_logger.LogError("Failed to create mod from zip mod at " + file + Environment.NewLine + e);
					continue;
				}

				yield return mod;
			}

			foreach (var submod in dir.GetDirectories().SelectMany(DiscoverMods))
			{
				yield return submod;
			}
		}

		public IEnumerable<Mod> Run()
		{
			foreach (var mod in DiscoverMods(_mods))
			{
				_logger.LogDebug($"Discovered mod: {mod} at {mod.Resources}");
				yield return mod;
			}
		}
	}
}
