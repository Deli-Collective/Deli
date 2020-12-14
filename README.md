# Deli
![Pre-release version](https://img.shields.io/github/v/release/Deli-Counter/Deli?include_prereleases&label=pre-release&style=flat-square) [![Discord](https://img.shields.io/discord/777351065950879744?label=&logo=discord&logoColor=ffffff&color=7389D8&labelColor=6A7EC2&style=flat-square)](https://discord.gg/g8xeFyt42j)

Deli is a modding framework based on BepInEx which provides one standard uniform way of packaging and loading mods containing any number of assets of any type. Originally built for the popular VR firearms sandbox game _Hotdogs, Horseshoes & Hand Grenades_ but theoretically usable under any Unity game with BepInEx.

## Quick Links
- [Installing](https://github.com/Deli-Counter/Deli/wiki/Installation)
- [Project boards](https://github.com/Deli-Counter/Deli/projects)
- [Join us on Discord for discussions, updates and support](https://discord.gg/g8xeFyt42j)

## Features
- **Standardized way of loading mods**: Have a mod you want to install? Drop it in the mods folder and you're good to go.
- **Easy resource and dependency management**: Mods built for the framework can enjoy easy-to-use asset loading and dependency management built into the framework.
- **Access to internal services kernel**: The kernel is a container for a bunch of objects that can be used globaly. Stick some stuff in there to let other mods have it in a more proper way

## Advantages
The main advantages to using this framework in addition to BepInEx are as follows:
- Each Deli mod can include any number of assets of any type, all within one mod file. It is possible to:
  - Bundle example assets with behaviours and modules (code assets)
  - Combine many assets of a certain theme to make a mega mod pack 
- Code assets can define asset loaders, which can be used by other mods to load their own assets. Consider:
  - Universal utility-like loaders, like the MonoMod patcher
  - Game-specific utility/mutator loaders (this allows the framework to be game independent)
