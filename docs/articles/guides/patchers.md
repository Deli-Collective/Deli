# Patchers
Patchers allow mods to edit or replace assemblies before they are loaded. This can be for a multitude of reasons, but a common one is to
add or remove members from a type (as this is not possible in runtime alternatives).

> [!IMPORTANT]
> Patchers' plugins must be <xref:Deli.Patcher.DeliModule>s; they cannot be @Deli.Setup.DeliBehaviour. Ensure you have the appropriate plugin to
> begin.

## Creating a Patcher
Patchers are simple. They only take a single, `ref` assembly parameter:

```c#
using Mono.Cecil;

public static void MyPatcher(ref AssemblyDefinition assembly)
{
    // Modify the assembly here using Mono.Cecil
}
```

Their implications are fairly complex though. If a patcher assembly references other assemblies, those assemblies become immune to
patchers. This is the reason why you cannot use @Deli.Setup.DeliBehaviour for patchers: they will prematurely load the core Unity assembly,
which BepInEx must patch.

> [!TIP]
> Do as little as possible in a patcher. That way, the most amount of assemblies can be patched.

## Inserting a Patcher
When inserting a patcher, you need to supply the mod that the patch originates from, and the name of the DLL file you wish to patch.

A demonstration of this would be:

```c#
using Mono.Cecil;

private void PatchTheGame(ref AssemblyDefinition assembly)
{
    ...
}

private void OnPatcher(PatcherStage stage)
{
    stage.Patchers[Source, "Assembly-CSharp.dll"] = PatchTheGame;
}
```