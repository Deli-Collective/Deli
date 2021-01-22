using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using BepInEx.Configuration;
using BepInEx.Logging;
using Deli.VFS;
using Valve.Newtonsoft.Json;

namespace Deli
{
    public class Mod
	{
		public Metadata Manifest { get; }

		public IDirectoryHandle Resources { get; }

		public ConfigFile Config { get; }

		public ManualLogSource Logger { get; }

		public Mod(Metadata manifest, IDirectoryHandle resources, ConfigFile config, ManualLogSource logger)
		{
			Manifest = manifest;
			Resources = resources;
			Config = config;
			Logger = logger;
		}

        public class Metadata
		{
			private static readonly Regex _guidFilter = new Regex(@"^[a-z0-9\.]+$");

			[JsonProperty(Required = Required.Always)]
			public string Guid { get; }
			[JsonProperty(Required = Required.Always)]
			public Version Version { get; }
			[JsonProperty(Required = Required.Always)]
			public Version DeliVersion { get; }

			public string? Name { get; }
			public string? Description { get; }
			public string? IconPath { get; }
			public string? SourceUrl { get; }

			public Dictionary<string, Version>? Dependencies { get; }
			public Dictionary<string, string>? Patchers { get; }
			public Dictionary<string, string>? Assets { get; }

			[JsonConstructor]
			public Metadata(string guid, Version version, Version deliVersion, Dictionary<string, Version>? dependencies, string? name, string? description, string? iconPath, string? sourceUrl, Dictionary<string, string>? patchers, Dictionary<string, string>? assets)
			{
				// Make sure GUID is normalized
				if (!_guidFilter.IsMatch(guid))
				{
					throw new FormatException("GUID should be lowercase alphanumeric, with '.' allowed.");
				}

				Guid = guid;
				Version = version;
				DeliVersion = deliVersion;

				Name = name;
				Description = description;
				IconPath = iconPath;
				SourceUrl = sourceUrl;

				Dependencies = dependencies;
				Patchers = patchers;
				Assets = assets;
			}
		}
    }
}
