# Deli Developer Documentation
Welcome! If you are looking for documentation on Deli, made to be read by developers, you are in the right place! If you are instead looking
for user documentation, such as installation instructions, see the [GitHub wiki](https://github.com/Deli-Collective/Deli/wiki).

Aside from this homepage, this website is split into 2 parts: articles and API documentation.  
Articles are hand-written documents. They should be especially helpful in getting the big picture.  
API documentation is a collection of auto-generated pages, each detailing a type. If you need XML docs, you can find them there.

## Why Use Deli
The fundamental purpose of Deli is to centralize the loading of arbitrary data, so that all mods are in a single format. The secondary goal
is to abstract and simplify IO to the point where most mods are no longer code, but data that other mods operate on.  

What does that mean in practice? Consider *Hotdogs, Horseshoes & Hand Grenades* (H3VR), a VR sandbox shooter.

### The Necessity
Before Deli, H3VR had 5 different mod formats:
- BepInEx plugins: arbitrary code
- LSIIC virtual objects: new guns, magazines, and attachments
- Sideloader mods: asset replacement via XUnity.ResourceRedirector
- TNHTweaker characters: new ways to play Take and Hold, a rogue-like gamemode within the game
- WurstMod maps: new maps to play Take and Hold on, and even new maps which contain new gamemodes entirely

That is quite the variety, and new users knew that. Without Deli, each format had different installation instructions, no baseline metadata,
and each mod had to do all of the IO that was needed. Consequently, there were redundancies in all of the frameworks and it was difficult to
create and maintain them due to the added complexity.

### The Effect
After Deli was introduced, the following formats shifted or began to embed themselves in Deli mods:
- BepInEx plugins
- TNHTweaker characters
- WurstMod maps

The other formats have not been embedded in Deli yet, because of constraints in Deli's v0.2 design. Though, these are still planned to be
converted.

For users this was amazing: you download a `.deli` file, drop it in the `mods` folder, and the mod is installed. If it requires other mods,
Deli errors out and tells you. No special instructions.  
For developers it was amazing: less code to write and maintain, and less problems with installation.

## What Deli is Not
Deli is not, and will not, replace BepInEx. Deli mods can contain arbitrary code akin to BepInEx patchers/plugins, but Deli still utilizes
many features of BepInEx. It would be a waste of effort to reimplement and maintain all that BepInEx provides.
