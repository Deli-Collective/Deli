using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using BepInEx.Configuration;
using BepInEx.Logging;
using Deli.VFS;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Deli
{
    public class Mod
    {
		public Manifest Info { get; }

		public IDirectoryHandle Resources { get; }

		public ConfigFile Config { get; }

		public ManualLogSource Logger { get; }

		public Mod(Manifest info, IDirectoryHandle resources)
		{
			Info = info;
			Resources = resources;
			Config = new ConfigFile(DeliConstants.Filesystem.ConfigsDirectory + "/" + info.Guid + ".cfg", false);
			Logger = BepInEx.Logging.Logger.CreateLogSource(info.Name ?? info.Guid);
		}

		public override string ToString()
		{
			return Info.ToString();
		}

		public class Manifest
		{
			private static readonly Regex _guidFilter = new Regex(@"^[a-z0-9\._]+$");

#pragma warning disable CS8618
			[JsonProperty(Required = Required.Always)]
			public string Guid { get; private set; }
			[JsonProperty(Required = Required.Always)]
			public Version Version { get; private set; }
			[JsonProperty(Required = Required.Always)]
			public Version DeliVersion { get; private set; }
#pragma warning restore CS8618

			public string? Name { get; private set; }
			public string? Description { get; private set; }
			public string? IconPath { get; private set; }
			public string? SourceUrl { get; private set; }

			public Dictionary<string, Version>? Dependencies { get; private set; }
			public Dictionary<string, string>? Patchers { get; private set; }
			public Dictionary<string, string>? Assets { get; private set; }

			[OnDeserialized]
			private void Validate()
			{
				// Make sure GUID is normalized
				if (!_guidFilter.IsMatch(Guid))
				{
					throw new FormatException("GUID should be lowercase alphanumeric, with '.' allowed.");
				}
			}

			public override string ToString()
			{
				return $"[{Name ?? Guid} {Version}]";
			}
		}
    }
}
