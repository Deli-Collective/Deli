using System;
using System.Collections.Generic;
using System.IO;
using BepInEx.Configuration;
using Ionic.Zip;
using Valve.Newtonsoft.Json;

namespace H3ModFramework
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ModInfo
    {
        private readonly Dictionary<string, byte[]> _loadedByteResources = new Dictionary<string, byte[]>();
        private readonly Dictionary<string, object> _loadedObjectResources = new Dictionary<string, object>();

        public ZipFile Archive;
        public string Location;
        [JsonProperty] public string Author;
        public ConfigFile Config;
        [JsonProperty] public string[] Dependencies;
        [JsonProperty] public string Guid;
        [JsonProperty] public ModuleInfo[] Modules;
        [JsonProperty] public string Name;
        public Version Version;
        [JsonProperty("Version")] public string VersionString;
        public bool IsArchive;

        /// <summary>
        ///     Fetches data from the mod's archive at the specified path
        /// </summary>
        /// <param name="path">Path to the data</param>
        /// <param name="cache">Optionally cache the result for future fetches</param>
        /// <returns>A byte array of the raw data at the given path</returns>
        public byte[] GetResource(string path, bool cache = true)
        {
            // If we already have it cached, return it
            if (_loadedByteResources.TryGetValue(path, out var result))
                return result;

            // If this mod is an archive and the file exists fetch it like that
            if (IsArchive && Archive.ContainsEntry(path))
                using (var memoryStream = new MemoryStream())
                {
                    Archive[path].Extract(memoryStream);
                    var bytes = memoryStream.ToArray();
                    if (cache) _loadedByteResources[path] = bytes;
                    return bytes;
                }

            // If this mod is an uncompressed folder fetch it like this
            if (!IsArchive && File.Exists(Path.Combine(Location, path)))
            {
                var bytes = File.ReadAllBytes(Path.Combine(Location, path));
                if (cache) _loadedByteResources[path] = bytes;
                return bytes;
            }

            // If it's not found
            H3ModFramework.PublicLogger.LogWarning($"Resource {path} requested in mod {Guid} but it doesn't exist!");
            return new byte[0];
        }

        /// <summary>
        ///     Fetches data from the specified path and automatically converts it to the given type
        /// </summary>
        /// <param name="path">Path to the data</param>
        /// <param name="cache">Optionally cache the result for future fetches</param>
        /// <typeparam name="T">Type to convert data to</typeparam>
        /// <returns>The converted data at the given path</returns>
        public T GetResource<T>(string path, bool cache = true)
        {
            // Check if it's already cached
            if (_loadedObjectResources.TryGetValue(path, out var cached))
                return (T) cached;

            // Try and load the bytes for the resource.
            var bytes = GetResource(path, false);
            // If it doesn't exist, return the default value for T
            if (bytes.Length == 0) return default;

            // Check if we have a type loader for the type
            if (ResourceTypeLoader.RegisteredTypeLoaders.TryGetValue(typeof(T), out var method))
            {
                // Invoke the type loader with the bytes from before
                var result = (T) method.Invoke(null, new object[] {bytes});
                if (cache) _loadedObjectResources[path] = result;
                return result;
            }

            H3ModFramework.PublicLogger.LogError($"Resource {path} in {Guid} was requested with type {nameof(T)} but no TypeLoader exists for this type!");
            return default;
        }

        /// <summary>
        ///     Constructs a ModInfo class from the archive at a given path
        /// </summary>
        /// <param name="path">Path to the archive</param>
        /// <returns>Instantiated ModInfo class</returns>
        public static ModInfo FromArchive(string path)
        {
            // Try and load the archive
            ZipFile archive;
            try
            {
                archive = ZipFile.Read(path);
            }
            catch (Exception e)
            {
                // This method should only be passed zip files, so we should provide a stack trace if this errors.
                H3ModFramework.PublicLogger.LogError($"Could not load mod archive ({path})\n{e}");
                return null;
            }

            // Try to locate the manifest file
            if (!archive.ContainsEntry(Constants.ManifestFileName))
            {
                H3ModFramework.PublicLogger.LogError($"Could not load {path} as it is not a valid mod.");
                return null;
            }

            // If we have a metadata file then we can go ahead and read the metadata
            using (var memoryStream = new MemoryStream())
            {
                archive[Constants.ManifestFileName].Extract(memoryStream);
                memoryStream.Position = 0;
                var mod = JsonConvert.DeserializeObject<ModInfo>(new StreamReader(memoryStream).ReadToEnd());
                mod.IsArchive = true;
                mod.Archive = archive;
                mod.Version = new Version(mod.VersionString);
                mod.Config = new ConfigFile(Path.Combine(Constants.ConfigDirectory, $"{mod.Guid}.cfg"), true);
                mod.Location = path;
                return mod;
            }
        }

        /// <summary>
        ///     Constructs a ModInfo class from a folder
        /// </summary>
        /// <param name="path">Path to the folder</param>
        /// <returns>Constructed ModInfo class</returns>
        public static ModInfo FromManifest(string path)
        {
            if (!File.Exists(path))
            {
                H3ModFramework.PublicLogger.LogError($"Could not load {path} as a mod, it is missing a manifest.");
                return null;
            }

            var mod = JsonConvert.DeserializeObject<ModInfo>(File.ReadAllText(path));
            mod.IsArchive = false;
            mod.Version = new Version(mod.VersionString);
            mod.Config = new ConfigFile(Path.Combine(Constants.ConfigDirectory, $"{mod.Guid}.cfg"), true);
            mod.Location = Path.GetDirectoryName(path);
            return mod;
        }

        public override string ToString()
        {
            return $"{Guid}@{VersionString}";
        }

        [JsonObject(MemberSerialization.OptIn)]
        public struct ModuleInfo
        {
            [JsonProperty] public string Loader;
            [JsonProperty] public string Path;

            public override string ToString() => $"{Loader}:{Path}";
        }
    }
}