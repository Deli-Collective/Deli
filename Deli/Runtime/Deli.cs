using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Atlas;
using Atlas.Fluent;
using Atlas.Impl;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Ionic.Zip;
using UnityEngine;
using Valve.Newtonsoft.Json;
using Valve.Newtonsoft.Json.Serialization;

namespace Deli
{
    [BepInPlugin(Constants.Guid, Constants.Name, Constants.Version)]
    public class Deli : BaseUnityPlugin
    {
        private StandardServiceKernel _kernel;

        public IServiceResolver Services => _kernel;

        private void Bind()
        {
            // Cleanup
            _kernel.Bind<IList<IDisposable>>()
                .ToConstant(new List<IDisposable>());

            // Simple constants
            _kernel.Bind<Deli>()
                .ToConstant(this);
            _kernel.Bind<ManualLogSource>()
                .ToConstant(base.Logger);
            {
                var manager = new GameObject("Deli Manager");
                DontDestroyOnLoad(manager);
                
                _kernel.Bind<GameObject>()
                    .ToConstant(manager);
            }

            // JSON
            _kernel.Bind<NamingStrategy>()
                .ToConstant(new CamelCaseNamingStrategy());
            _kernel.Bind<IContractResolver>()
                .ToRecursiveNopMethod(x => new DefaultContractResolver
                {
                    NamingStrategy = x.Get<NamingStrategy>().Expect("JSON naming strategy not found")
                })
                .InSingletonNopScope();
            _kernel.Bind<IList<JsonConverter>>()
                .ToConstant(new List<JsonConverter>
                {
                    new OptionJsonConverter()
                });
            _kernel.Bind<JsonSerializerSettings>()
                .ToRecursiveNopMethod(x => new JsonSerializerSettings
                {
                    ContractResolver = x.Get<IContractResolver>().Expect("JSON contract resolver not found"),
                    Converters = x.Get<IList<JsonConverter>>().Expect("JSON converters not found")
                })
                .InSingletonNopScope();
            _kernel.Bind<JsonSerializer>()
                .ToRecursiveNopMethod(x =>
                {
                    var settings = x.Get<JsonSerializerSettings>().Expect("JSON settings not found.");

                    return JsonSerializer.Create(settings);
                })
                .InSingletonNopScope();

            // Basic impls
            _kernel.Bind<IAssetReader<Assembly>>()
                .ToConstant(new AssemblyAssetReader());
            _kernel.Bind<IAssetReader<Option<Mod.Manifest>>>()
                .ToRecursiveNopMethod(x => new JsonAssetReader<Mod.Manifest>(x))
                .InSingletonNopScope();

            // Associative services dictionaries
            _kernel.Bind<IDictionary<string, IAssetLoader>>()
                .ToConstant(new Dictionary<string, IAssetLoader>
            {
                ["assembly"] = new AssemblyModuleLoader()
            });
            _kernel.Bind<IDictionary<string, Mod>>()
                .ToConstant(new Dictionary<string, Mod>());

            // Enumerables
            _kernel.Bind<IEnumerable<IAssetLoader>>()
                .ToRecursiveMethod(x => x.Get<IDictionary<string, IAssetLoader>>().Map(v => (IEnumerable<IAssetLoader>) v.Values))
                .InTransientScope();
            _kernel.Bind<IEnumerable<Mod>>()
                .ToRecursiveMethod(x => x.Get<IDictionary<string, Mod>>().Map(v => (IEnumerable<Mod>) v.Values))
                .InTransientScope();

            // Contextual to dictionaries
            _kernel.Bind<IAssetLoader, string>()
                .ToWholeMethod((services, context) => services.Get<IDictionary<string, IAssetLoader>>()
                    .Map(x => x.OptionGetValue(context))
                    .Flatten())
                .InTransientScope();
            _kernel.Bind<Mod, string>()
                .ToWholeMethod((services, context) => services.Get<IDictionary<string, Mod>>()
                    .Map(x => x.OptionGetValue(context))
                    .Flatten())
                .InTransientScope();

            // Custom impls
            _kernel.Bind<ManualLogSource, string>()
                .ToContextualNopMethod(x => BepInEx.Logging.Logger.CreateLogSource(x))
                .InSingletonScope();
            _kernel.Bind<ConfigFile, string>()
                .ToContextualNopMethod(x => new ConfigFile(Path.Combine(Constants.ConfigDirectory, $"{x}.cfg"), true))
                .InSingletonScope();
        }

        private void Awake()
        {
            _kernel = new StandardServiceKernel();
            Bind();
            
            Initialize();
        }

        private void OnDestroy()
        {
            var disposables = Services.Get<IList<IDisposable>>().Expect("Could not find disposables.");

            foreach (var disposable in disposables)
            {
                disposable.Dispose();
            }
        }

        private Option<Mod> CreateMod(IRawIO raw)
        {
            var resources = new CachedResourceIO(new ResolverResourceIO(raw, Services));
            var infoParsed = resources.Get<Option<Mod.Manifest>>(Constants.ManifestFileName).Unwrap();

            if (!infoParsed.MatchSome(out var info))
            {
                Logger.LogWarning("Failed to parse manifest file");
            }
            else if (!Services.Get<ConfigFile, string>(info.Guid).MatchSome(out var config))
            {
                Logger.LogWarning("Failed to acquire config file for " + info);
            }
            else if (!Services.Get<ManualLogSource, string>(info.Name.UnwrapOr(info.Guid)).MatchSome(out var log))
            {
                Logger.LogWarning("Failed to log source for " + info);
            }
            else
            {
                return Option.Some(new Mod(info, resources, config, log));
            }

            return Option.None<Mod>();
        }

        /// <summary>
        ///     Enumerates the mods in the mods folder
        /// </summary>
        /// <returns>An enumerable of the mods in the mods folder</returns>
        private IEnumerable<Mod> DiscoverMods(DirectoryInfo dir)
        {
            var manifestPath = Path.Combine(dir.Name, Constants.ManifestFileName);
            if (File.Exists(manifestPath)) // Directory mod
            {
                var io = new DirectoryRawIO(dir);

                if (CreateMod(io).MatchSome(out var mod))
                {
                    yield return mod;
                }
                else
                {
                    Logger.LogWarning("Failed to create mod from directory: " + dir);
                }

                // Halt discovery in this directory
                // Used because non-Deli *.zip and manifest.json files would be misinterpretted.
                yield break;
            }

            foreach (var archiveFile in dir.GetFiles("*." + Constants.ModExtension))
            {
                // Zip mod

                var raw = archiveFile.OpenRead();
                _kernel.Get<IList<IDisposable>>().Unwrap().Add(raw);

                var zip = ZipFile.Read(raw);
                var io = new ArchiveRawIO(zip);
                
                if (!CreateMod(io).MatchSome(out var mod))
                {
                    Logger.LogWarning("Failed to create mod from archive: " + archiveFile);
                    continue;
                }

                yield return mod;
            }

            foreach (var mod in dir.GetDirectories().SelectMany(DiscoverMods))
            {
                yield return mod;
            }
        }

        private void Initialize()
        {
            EnsureDirectoriesExist();

            // Discover all the mods
            var modsDir = new DirectoryInfo(Constants.ModDirectory);
            var mods = DiscoverMods(modsDir).ToDictionary(x => x.Info.Guid, x => x);
            Logger.LogInfo($"Created {mods.Count} mods");

            // Make sure all dependencies are satisfied
            if (!CheckDependencies(mods))
            {
                Logger.LogError("One or more dependencies are not satisfied. Aborting initialization.");
                return;
            }

            // Load the mods
            try
            {
                // Sort the mods in the order they depend on each other
                var sorted = mods.Values.TSort(x => x.Info.Dependencies.Keys.Select(dep => mods[dep]), true);
                foreach (var mod in sorted) LoadMod(mod);
            }
            catch (Exception e)
            {
                Logger.LogError("Could not initialize mod framework.\n" + e);
            }
        }

        private bool CheckDependencies(Dictionary<string, Mod> mods)
        {
            foreach (var mod in mods.Values)
            foreach (var dep in mod.Info.Dependencies)
            {
                string DepToString()
                {
                    return $"{dep.Key} @ {dep.Value}";
                }

                // Try finding the installed dependency
                if (!mods.TryGetValue(dep.Key, out var resolved))
                {
                    Logger.LogError($"Mod {mod} depends on {DepToString()}, but it is not installed!");
                    return false;
                }

                // Check if the installed version satisfies the dependency request
                if (!resolved.Info.Version.Satisfies(dep.Value))
                {
                    Logger.LogError($"Mod {mod} depends on {DepToString()}, but version {resolved.Info.Version} is installed!");
                    return false;
                }
            }

            return true;
        }

        private static void EnsureDirectoriesExist()
        {
            Directory.CreateDirectory(Constants.ModDirectory);
            Directory.CreateDirectory(Constants.ConfigDirectory);
        }

        private void LoadMod(Mod mod)
        {
            // For each module inside the mod, load it
            foreach (var asset in mod.Info.Assets)
            {
                var assetPath = asset.Key;
                var assetLoader = asset.Value;

                if (!Services.Get<IAssetLoader, string>(assetLoader).MatchSome(out var loader))
                {
                    Logger.LogError($"Asset loader not found for {mod}: {assetLoader}");
                    continue;
                }

                loader.LoadAsset(_kernel, mod, assetPath);
            }

            // Add the ModInfo to the kernel.
            Services.Get<IDictionary<string, Mod>>().Unwrap().Add(mod.Info.Guid, mod);
        }
    }
}