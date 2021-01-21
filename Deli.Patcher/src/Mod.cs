using System;
using System.Collections.Generic;

namespace Deli
{
    public class Mod
	{
        public string Guid {get;set;}
        public Version Version {get;set;}
        public string? Name {get;set;}
        public Dictionary<string, Version>? Dependencies {get;set;}
        public Dictionary<string, string>? Assets {get;set;}
        public Dictionary<string, string>? Patchers {get;set;}
        public string? IconPath {get;set;}
        public string? SourceUrl {get;set;}
    }
}
