# H3ModFramework
H3ModFramework is a framework for modularizing mods into archives containing one or more modules. These modules can be anything - provided you have the module loader to load them! The module loaders are themselves also modules. This allows for complete organization and modularity for a number of reasons:
- Each mod isn't limited to just one type of thing. A single mod archive can contain any number of modules, whether it be a .NET Assembly, a custom weapon, custom Take and Hold character or sosig template. Anything can be included.
- Mod archives can include their own dependencies if needed. For example if you've written some custom code which requires an additional external library it can be included inside the one mod archive along with the rest of your data.
- Because the module loaders are themselves also modules this allows for infinite kinds of content that can be loaded using this framework. This framework only ships with one module loader - the Assembly loader - which allows you to load custom assemblies containing more module loaders while leaving the framework to do all the hard work of managing dependency load orders and common tasks.

## Examples
Lets say you've made a custom character for Take and Hold and to compliment it you've made some custom enemy templates as well. Normally you'd have to place the files individually inside their respective folders. With this mod framework you can package them all up into a single archive, and if you wanted, even include some custom weaponry!

## Features
- **Extreme modularity**: all module loaders are user-defined, giving external libraries control over how they want to load their own data.
- **Resource management**: Built into the framework is modular resource management. You can fetch and cache content from your mod archives as raw byte arrays, or use the fancy included Type Loaders (which are also modular!) which provide an easy-to-use intermediate for fetching and caching the data as any type
- **Built-in dependency and version management**: Let the framework handle the discovery of mods. All you have to do is tell the framework which mods you're expecting to be there and it will do the rest.
- **Compatible with legacy formats**: (Provided someone writes a module loader for it!)
