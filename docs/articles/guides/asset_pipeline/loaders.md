# Asset Loaders
Asset loaders (or just "loaders") are delegates which process handles defined in mod manifests. They are the end of the asset pipeline.

> [!TIP]
> A completed asset loader can be found in [the example
> mod](https://github.com/Deli-Collective/Deli.ExampleMod/blob/master/Deli.ExampleMod/src/AssetLoaders.cs).

## Creating a (Immediate) Loader
Unlike readers, almost all loaders are non-static methods because they need to mutate data. For example:

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
        if (handle is not IFileHandle file)
        {
            _logger.LogInfo($"{mod} gave me a directory to load.");    
        }
    
        _logger.LogInfo($"{mod} gave me file to load. I have now loaded {++_loaded} files.");
    }
} 
```

> [!TIP]
> Visit @Deli.VFS to see all the possible handle types. Mods can give your asset loader all kinds!

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

## Inserting a Loader
Akin to readers, loaders need to be directly added to Deli. Unlike readers, loaders are scoped by stage and mod. The stage can be a
singular stage, or the current stage and any that follow it (`Shared`).

The following adds a loader owned by the current mod that can only be used in the patcher stage:

```c#
using Deli;
using Deli.Patcher;
using Deli.VFS;

private void LoadAwesomeAssets(PatcherStage stage, Mod mod, IHandle handle)
{
    ...
}

private void OnPatcher(PatcherStage stage)
{
    // Pass 'Source' because that is the mod responsible for the loader.
    stage.PatcherAssetLoaders[Source, "awesome"] = LoadAwesomeAssets; 
}
```

> [!IMPORTANT]
> Loader names are regulated. They have the same requirements as a GUID: lowercase alphanumeric, permitting '.' and '_'. This is done for
> simplicity sake. Failing to meet these criteria will cause an exception. 