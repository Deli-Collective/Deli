# H3ModFramework
H3ModFramework is a framework for modularizing mods into archives containing one or more modules. These modules can be anything - provided you have the module loader to load them! The module loaders are themselves also modules. This allows for complete organization and modularity for a number of reasons:
- Each mod isn't limited to just one type of thing. A single mod archive can contain any number of modules, whether it be a .NET Assembly, a custom weapon, custom Take and Hold character or sosig template. Anything can be included.
- Mod archives can include their own dependencies if needed. For example if you've written some custom code which requires an additional external library it can be included inside the one mod archive along with the rest of your data.
- Because the module loaders are themselves also modules this allows for infinite kinds of content that can be loaded using this framework. This framework only ships with one module loader - the Assembly loader - which allows you to load custom assemblies containing more module loaders while leaving the framework to do all the hard work of managing dependency load orders and common tasks.

## Features
- **Extreme modularity**: all module loaders are user-defined, giving external libraries control over how they want to load their own data.
- **Resource management**: Built into the framework is modular resource management. You can fetch and cache content from your mod archives as raw byte arrays, or use the fancy included Type Loaders (which are also modular!) which provide an easy-to-use intermediate for fetching and caching the data as any type
- **Built-in dependency and version management**: Let the framework handle the discovery of mods. All you have to do is tell the framework which mods you're expecting to be there and it will do the rest.
- **Compatible with legacy formats**: (Provided someone writes a module loader for it!)

## Examples
So lets say you're making a sci-fi pack for H3. You have a couple of custom weapons with some custom scripts, a custom Take and Hold character and some custom sosig templates to go with it. Normally this would require you to place all these files in multiple folders in the game's root directory. With this modding framework and a module loader for each module this is completely mitigated. You distribute your pack as a single archive with the resources contained within and tell the framework where each resource is and what to load it with.

#### Example archive structure
```
- YourScifiPack.zip
  - metadata.json: info about your mod (Explained below)
  - scifiWeaponScripts.dll
  - Resources/
    - scifiCharacter.json
    - sosig_scifi.json
    - scifiWeapons.asset
```

#### Metadata.json
This file describes to the framework what your mod is and how it should be loaded.
(This is just an example. These dependencies and loaders don't exist. Yet.)
```JSON
{
  "Guid":"nrgill28.scifi_pack",
  "Name":"H3VR Scifi Pack",
  "Author":"nrgill28",
  "Dependencies":[
    "h3vr.sideloader",
    "tnh_tweaker"
  ],
  "Modules":[
    {
      "Loader":"Assembly",
      "FilePath":"scifiWeaponScripts.dll"
    },
    {
      "Loader":"TnHTweakerCharacter",
      "FilePath":"Resources/scifiCharacter.json"
    },
    {
      "Loader":"TnHTweakerSosig",
      "FilePath":"Resources/sosig_scifi.json"
    },
    {
      "Loader":"VirtualObject",
      "FilePath":"Resources/scifiWeapons.asset"
    }
  ]
}
```
