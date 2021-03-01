# Delayed vs Immediate Pipeline
This article is very small, but covers an important difference that readers and loaders possess.

## Definitions
### What is Immediate?
The immediate asset pipeline is the asset pipeline that must be performed in a single, blocking operation.

### What is Delayed?
The delayed asset pipeline is the asset pipeline that can be performed immediately, but may also be spread out over multiple frames.
 
## Advantages
### Why use Immediate?
Immediate is the easiest to write for. It also is required for certain assets, which can only be read in a synchronous operation. Sometimes
these assets can be made into a delayed asset by reading the bytes asynchronously, but some like assemblies cannot be delayed like this
because they might be read directly from a file on disk.

### Why use Delayed?
Every immediate operation, Deli or not, causes a hitch during runtime or increases the startup time of the game. To mitigate this, Unity
has coroutines. Deli integrates this via the delayed asset pipeline. In this pipeline, an operation is completed over the course of a
coroutine. This allows long operations to be performed with smooth framerate and fast startup times.