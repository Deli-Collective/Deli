# Behaviour Plugins
Behaviours are plugins that, while unusable in the patcher stage, offer much more utility than a module.

> [!TIP]
> A completed behaviour can be found in [the example
> mod](https://github.com/Deli-Collective/DeliExampleMod/blob/master/Deli.ExampleMod/src/ExampleMod.cs).

## Creating a Behaviour
Creating a behaviour is as simple as can be. Just declare a class that inherits from @Deli.Setup.DeliBehaviour. For example:

```c#
using Deli.Setup;

public class MyBehaviour : DeliBehaviour
{
}
```

## Supported Stages
These are the stages that can be subscribed to by behaviours

- @Deli.Setup.SetupStage
- @Deli.Runtime.RuntimeStage