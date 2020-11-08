using System;
using System.Collections.Generic;
using System.IO;
using Ionic.Zip;
using Valve.Newtonsoft.Json;

namespace H3ModLoader
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ModInfo
    {
        // Mod info
        [JsonProperty] public string Guid;
        [JsonProperty] public string Name;
        [JsonProperty] public string Author;
        [JsonProperty] public string[] Dependencies;

        // Loader info
        [JsonProperty] public string Loader;
        [JsonProperty] public string LoaderVersion;
        [JsonProperty] public string GameVersion;

        // Resources
        public ZipFile Archive;
        private Dictionary<string, byte[]> _loadedByteResources = new Dictionary<string, byte[]>();
        private Dictionary<string, object> _loadedObjectResources = new Dictionary<string, object>();

        /// <summary>
        /// Fetches data from the mod's archive at the specified path
        /// </summary>
        /// <param name="path">Path to the data</param>
        /// <param name="cache">Optionally cache the result for future fetches</param>
        /// <returns>A byte array of the raw data at the given path</returns>
        public byte[] GetResource(string path, bool cache = true)
        {
            if (_loadedByteResources.ContainsKey(path))
                return _loadedByteResources[path];
            if (Archive.ContainsEntry(path))
            {
                using (var memoryStream = new MemoryStream())
                {
                    Archive[path].Extract(memoryStream);
                    var bytes = memoryStream.ToArray();
                    if (cache) _loadedByteResources[path] = bytes;
                    return bytes;
                }
            }

            H3ModLoader.PublicLogger.LogWarning($"Resource {path} requested in mod {Guid} but it doesn't exist!");
            return new byte[0];
        }

        /// <summary>
        /// Fetches data from the specified path and automatically converts it to the given type
        /// </summary>
        /// <param name="path">Path to the data</param>
        /// <param name="cache">Optionally cache the result for future fetches</param>
        /// <typeparam name="T">Type to convert data to</typeparam>
        /// <returns>The converted data at the given path</returns>
        public T GetResource<T>(string path, bool cache = true)
        {
            // Check if it's already cached
            if (_loadedObjectResources.ContainsKey(path))
                return (T) _loadedObjectResources[path];
            
            // Try and load the bytes for the resource.
            var bytes = GetResource(path, false);
            // If it doesn't exist, return the default value for T
            if (bytes.Length == 0) return default;

            // Check if we have a type loader for this type
            if (!TypeLoaders.RegisteredTypeLoaders.ContainsKey(typeof(T)))
            {
                H3ModLoader.PublicLogger.LogError($"Resource {path} in {Guid} was requested with type {nameof(T)} but no TypeLoader exists for this type!");
                return default;
            }

            // Invoke the type loader with the bytes from before
            var result = (T) TypeLoaders.RegisteredTypeLoaders[typeof(T)].Invoke(null, new object[] {bytes});
            if (cache) _loadedObjectResources[path] = result;
            return result;
        }

        /// <summary>
        /// Constructs a ModInfo class from the archive at a given path
        /// </summary>
        /// <param name="path">Path to the archive</param>
        /// <returns>Instantiated ModInfo class</returns>
        public static ModInfo FromFile(string path)
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
                H3ModLoader.PublicLogger.LogError($"Could not load mod archive ({path})\n" + e.StackTrace);
                return null;
            }

            // Try to locate the metadata file
            if (!archive.ContainsEntry(Constants.ArchiveMetaFilePath))
            {
                H3ModLoader.PublicLogger.LogError($"Could not load {path} as it is not a valid mod.");
                return null;
            }

            // If we have a metadata file then we can go ahead and read the metadata
            using (var memoryStream = new MemoryStream())
            {
                archive[Constants.ArchiveMetaFilePath].Extract(memoryStream);
                memoryStream.Position = 0;
                var mod = JsonConvert.DeserializeObject<ModInfo>(new StreamReader(memoryStream).ReadToEnd());
                mod.Archive = archive;
                return mod;
            }
        }
    }
}