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
        public Manifest Info { get; }

        public IResourceIO Resources { get; }

        public ConfigFile Config { get; }

        public ManualLogSource Log { get; }

        public Mod(Manifest info, IResourceIO resources, ConfigFile config, ManualLogSource log)
        {
            Info = info;
            Resources = resources;
            Config = config;
            Log = log;
        }

        public override string ToString()
        {
            return Info.ToString();
        }

        [JsonObject(ItemRequired = Required.Always)]
        public readonly struct Manifest
        {
            public string Guid { get; }
            public Version Version { get; }
            public Dictionary<string, Version> Dependencies { get; }

            [JsonProperty(Required = Required.Default)]
            public Option<string> Name { get; }
            [JsonProperty(Required = Required.Default)]
            public Option<string[]> Authors { get; }

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

            public override string ToString()
            {
                return $"{Guid} @ {Version}";
            }
        }
    }
}