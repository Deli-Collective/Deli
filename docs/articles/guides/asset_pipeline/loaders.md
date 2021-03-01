# Asset Loaders
Asset loaders (or just "loaders") are delegates which process handles defined in mod manifests. They are the end of the asset pipeline.

> [!TIP]
> A completed asset loader can be found in [the example
> mod](https://github.com/Deli-Collective/Deli.ExampleMod/blob/master/Deli.ExampleMod/src/AssetLoaders.cs).

## Creating a (Immediate) Loader
Unlike readers, almost all loaders are non-static methods because they need to mutate the state of the application. For example:

```c#
using BepInEx.Logging;
using Deli.Setup;
using Deli.VFS;

public class LoggerLoader
{
    private readonly ManualLogSource _logger;
    private int _loaded;
    
    public LoggerLoader(ManualLogSource logger)
    {
        _logger = logger;
    }

    public void Load(SetupStage stage, Mod mod, IHandle handle)
    {
        _logger.LogInfo($"{mod} gave me {handle} to load. I have now loaded {++_loaded} handles.");
    }
} 
```

## Creating a Delayed Loader
The delayed asset pipeline has the same problem as the async
Delayed loaders much simpler than delayed readers. Just return IEnumerator:

```c#
using System.Collections;
using BepInEx.Logging;
using Deli.Runtime;
using Deli.VFS;
using UnityEngine;

public class LoggerLoader
{
    private readonly ManualLogSource _logger;
    
    public LoggerLoader(ManualLogSource logger)
    {
        _logger = logger;
    }

    public IEnumerator Load(RuntimeStage stage, Mod mod, IHandle handle)
    {
        _logger.LogInfo($"I am about to do a long operation to load {handle}");
        
        // Despite waiting 10s, this does not freeze the game for 10s
        yield return new WaitForSeconds(10f);
        
        _logger.LogInfo($"I have finished the long operation to load {handle}");
    }
}
```