# Modules Plugins
Modules are plugins that can be used in the patcher stage, but offer significantly less utility. You should always default to behaviours,
unless you absolutely need to patch a DLL before it is loaded.

## Creating a Module
Creating a module is slightly more complicated than a behaviour, but still easy. All you need to do is inherit from a
@Deli.Patcher.DeliModule and implement the constructor.

```c#
using Deli.Patcher;

public class MyModule : DeliModule
{
    public MyModule(Mod mod) : base(mod)
    {
    }
}
```

> [!WARNING]
> Do not change the constructor signature (parameters) or your module will fail to load. 

## Supported Stages
For all intents and purposes, the only stage supported is @Deli.Patcher.PatcherStage.

To be more specific, the only *strongly typed* stage supported is @Deli.Patcher.PatcherStage. However, modules can subscribe to a weakly typed
@Deli.Patcher.DeliModule.StageEvents.Other event, which is invoked for the remaining stages. However, this subscription provides nothing
more than a @Deli.Stage. If you need a module (for patching) and a behaviour, it is recommended to create an assembly for each and package
them in the same mod.