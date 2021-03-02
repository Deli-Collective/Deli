# Version Checkers
Version checkers allows Deli to automatically check for newer versions of mods, given that each mod supplies a source URL.

> [!NOTE]
> A source URL is a URL to where the mod originated from. While the name contains "source" in it, it does not necessarily mean the URL to
> the source code. The URL may very well be a download to a release page, such as BoneTome.

## Creating a Version Checker
Aside from coroutines, version checkers are simple. They are given a path (`https://example.com/[path]`) and are expected to produce a version.

Here's a simple example that reads a plaintext webpage for a version:
```c#
using Deli.Runtime.Yielding;
using SemVer;
using UnityEngine;

private static ResultYieldInstruction<SemVersion?> PlaintextVersionOf(string path)
{
    const string pathPrefix = "https://example.com/";

    var request = new UnityWebRequest(pathPrefix + path, UnityWebRequest.kHttpVerbGET)
    {
        downloadHandler = new DownloadHandlerBuffer()
    };
    
    return new AsyncOperationYieldInstruction(request.Send()).CallbackWith(request =>
    {
        return SemVersion.Parse(request.downloadHandler.text);
    });
}
```

> [!TIP]
> If you expect a path of a certain format, validate it! Deli passes the raw path that is given by mods, which could be complete gibberish.

## Inserting a Version Checker
Version checkers are the simplest item to insert into Deli. They are indexed by the domain name of source URLs that they are responsible
for, e.g. `github.com` is responsible for GitHub source URLs. 

Example:
```c#
using Deli.Runtime;
using Deli.Runtime.Yielding;
using SemVer;

private ResultYieldInstruction<SemVersion?> VersionOf(string path)
{
    ...
}

private void OnRuntime(RuntimeStage stage)
{
    stage.VersionCheckers["example.com"] = VersionOf;
} 
```

> [!WARNING]
> If a version checker already exists for that domain, an exception will be thrown. This is done to avoid silent conflicts.