using System;
using System.Collections.Generic;
using System.Text;
using Atlas;
using BepInEx.Configuration;
using BepInEx.Logging;
using Valve.Newtonsoft.Json;

namespace Deli
{
    public readonly struct Mod
    {
        /// <summary>
        ///     Information about the mod
        /// </summary>
        public Manifest Info { get; }

        /// <summary>
        ///     The assets for the mod
        /// </summary>
        public IResourceIO Resources { get; }

        /// <summary>
        ///     The configuration for the mod
        /// </summary>
        public ConfigFile Config { get; }

        /// <summary>
        ///     The log to be used by the mod
        /// </summary>
        public ManualLogSource Log { get; }

        public Mod(Manifest info, IResourceIO resources, ConfigFile config, ManualLogSource log)
        {
            Info = info;
            Resources = resources;
            Config = config;
            Log = log;
        }

        /// <summary>
        ///     A simple printout of this mod's identity. Use <seealso cref="Info"/> in conjunction with <see cref="Manifest.ToPrettyString"/> to get a more complete printout.
        /// </summary>
        public override string ToString()
        {
            return Info.ToString();
        }

        [JsonObject(ItemRequired = Required.Always)]
        public readonly struct Manifest
        {
            /// <summary>
            ///     The globally unique identitifer of this mod. This cannot conflict with any other mods.
            /// </summary>
            public string Guid { get; }

            /// <summary>
            ///     The current version of this mod.
            /// </summary>
            public Version Version { get; }

            /// <summary>
            ///     The GUIDs and corresponding versions of mods that this mod requires.
            /// </summary>
            public Dictionary<string, Version> Dependencies { get; }


            /// <summary>
            ///     The user-friendly name for this mod.
            /// </summary>
            [JsonProperty(Required = Required.Default)]
            public Option<string> Name { get; }

            /// <summary>
            ///     The creators of this mod.
            /// </summary>
            /// <value></value>
            [JsonProperty(Required = Required.Default)]
            public Option<string[]> Authors { get; }


            /// <summary>
            ///     The asset paths and corresponding asset loaders that this mod contains.
            /// </summary>
            public Dictionary<string, string> Assets { get; }

            [JsonConstructor]
            public Manifest(string guid, Version version, Option<string> name, Option<string[]> authors, Dictionary<string, Version> dependencies, Dictionary<string, string> assets)
            {
                Guid = guid;
                Version = version;
                Dependencies = dependencies;

                Name = name;
                Authors = authors;
                
                Assets = assets;
            }

            /// <summary>
            ///     A pretty-printout of the mods identity. Examples:
            ///     <code>deli.example @ 1.0.0.0</code>
            ///     <code>Example Mod (deli.example @ 1.0.0.0)</code>
            ///     <code>Example Mod (deli.example @ 1.0.0.0) by Developer A</code>
            ///     <code>Example Mod (deli.example @ 1.0.0.0) by Developer A and Developer B</code>
            /// </summary>
            public string ToPrettyString()
            {
                var builder = new StringBuilder();
                
                var hasName = Name.MatchSome(out var name);
                if (hasName)
                {
                    builder.Append(name).Append(" (");
                }

                builder.Append(Guid).Append(" @ ").Append(Version);

                if (hasName)
                {
                    builder.Append(')');
                }

                if (Authors.MatchSome(out var authors))
                {
                    builder.Append(' ');

                    var iLast = authors.Length - 1;
                    for (var i = 0; i < iLast; ++i)
                    {
                        builder.Append(authors[i]).Append(", ");
                    }

                    if (authors.Length > 1)
                    {
                        builder.Append("and ");
                    }
                    
                    builder.Append(authors[iLast]);
                }

                return builder.ToString();
            }

            /// <summary>
            ///     A simple prinout of this mod's identity. Example:
            ///     <code>deli.example @ 1.0.0.0</code>
            /// </summary>
            public override string ToString()
            {
                return $"[{Guid} {Version}]";
            }
        }
    }
}