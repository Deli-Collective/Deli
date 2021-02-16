---
title: "Deli API"
---

# Namespaces
Given that there are so many namespaces, it might help to understand what each is for.

---

## Stages
**This is the most important category**, and helps clarify the purpose of each stage. If you can only read one category, read this one.  

For ease of reference, each stage contains a list of details:
- Stage: the class of the stage
- Pipeline: the reader/loaders used by the stage
- Plugins: the types that can be used as entrypoints for custom code

### @Deli.Patcher
This stage allows for patching DLLs, similar to [BepInEx's preloader patchers](https://bepinex.github.io/bepinex_docs/master/articles/dev_guide/preloader_patchers.html).  

> [!WARNING]
> Reference as few assemblies as possible here. Doing so means that they cannot be patched.

Details:
- Stage: @Deli.Patcher.PatcherStage
- Pipeline: @Deli.Immediate
- Plugins: @Deli.Patcher.DeliModule

### @Deli.Setup
This stage is ran after all patching is done, but immediately before @Deli.Runtime. This is within the same timeframe as BepInEx plugin loading.

> [!NOTE]
> Any method patching or detouring should be done here. The delay of @Deli.Runtime might allow methods to be inlined, and referencing assemblies in @Deli.Patcher should be avoided.

Details:
- Stage: @Deli.Setup.SetupStage
- Pipeline: @Deli.Immediate
- Plugins: @Deli.Patcher.DeliModule, @Deli.Setup.DeliBehaviour

### @Deli.Runtime
This stage runs during the time that the game runs, hence the name "runtime." This is after BepInEx plugin loading.  
This namespace also contains the "delayed" pipeline. It is named "delayed" because the readers and loaders operate over multiple frames to provide delayed, non-blocking execution.

> [!TIP]
> Use this stage whenever possible. Using this stage gives a more fluent user experience because it adds no startup delay.
> Even if you need the abilities of @Deli.Patcher or @Deli.Setup, you can still allow content loaded from @Deli.Runtime.

Details:
- Stage: @Deli.Runtime.RuntimeStage
- Pipeline: @Deli.Runtime (with the assistance of @Deli.Runtime.Yielding )
- Plugins: @Deli.Patcher.DeliModule, @Deli.Setup.DeliBehaviour

---

## Virtual Filesystem
These namespaces represent the filesystems used by Deli.

### @Deli.VFS
Interfaces and extensions of for the **v**irtual **f**ile **s**ystem (VFS). Allows traversing and reading mod files and directories (hereafter: handles).

### @Deli.VFS.Disk
Disk implementations of @Deli.VFS interfaces. These implementations represent handles that are directly on disk, i.e. can be seen in the OS filesystem.  

> [!TIP]
> You can reload the handles in this VFS by casting the root handle to @Deli.VFS.Disk.IDiskHandle and calling @Deli.VFS.Disk.IDiskHandle.Refresh. Use it to prototype without restarting the game. 

### @Deli.VFS.Globbing
Globbing support for @Deli.VFS. A simple extension method is provided for most use cases, but @Deli.VFS.Globbing.GlobberFactory allows for configuring the possible globs, as well as creating a reusable glob method.

### @Deli.VFS.Zip
Zip implementations of @Deli.VFS interfaces. These implementations represent handles that are contained in a zip file.

---

## Other
But still important!

### @Deli
Items that all stages of Deli use.

### @Deli.Immediate
Items relating to the immediate reader/loader pipeline used by @Deli.Patcher and @Deli.Setup.  
This is named "immediate" because readers and loaders finish "immediately" after they begin, compared to @Deli.Runtime.

### @Deli.Runtime.Yielding
Subclasses and extension methods relating to UnityEngine.YieldInstruction, to make it easier to perform operations during @Deli.Runtime.

### @Semver
The [semver](https://www.nuget.org/packages/semver/) NuGet package. This package is not ready for use by Unity 5.6, so the core class has been embedded in this project with minor adjustments.