# Plugin Generalizations
For simplicity, both plugin types are kept as similar as possible. This lends to generalizations that can be made about all plugins.

## Abstracts
If you need a plugin to be inheritable, but not loaded by Deli, simply mark it as abstract.

## BepInEx-like Properties
Plugins have access to `Config` and `Logger` properties identical to BepInEx plugins. They also have access to an `Info` property, but it
is different from the `Info` property from BepInEx.

## Reading and Modifying Stages
Stages are an essential part of integrating your mod into the Deli framework. To use a stage, subscribe the corresponding event of the
`Stages` property. The method subscribed to it will be ran once.

> [!WARNING]
> Not all plugins can use the same stages. Consult the individual articles for which stages they can use.

For the setup stage, this would look like:
```c#
using Deli.Setup;

public class MyBehaviour : DeliBehaviour
{
    public MyBehaviour()
    {
        Stages.Setup += OnSetup;
    }
    
    private void OnSetup(SetupStage stage)
    {
        // Grab objects from the stage or insert them here
    }
}
```

While this example is the setup stage, the same principle applies to the runtime or patcher stage.

> [!WARNING]
> Although subscribing and unsubscribing can be done anywhere, attempting to do so after the stage has ran will result in an exception.