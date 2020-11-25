# Deli
![Pre-release version](https://img.shields.io/github/v/release/nrgill28/Deli?include_prereleases&label=pre-release&style=flat-square) ![Discord](https://img.shields.io/discord/777351065950879744?label=&logo=discord&logoColor=ffffff&color=7389D8&labelColor=6A7EC2&link=https://discord.gg/ZSXUVxxWeD&style=flat-square)

Deli is a modding framework based on BepInEx which provides one standard uniform way of packaging and loading mods containing any number of assets of any type. Originally built for the popular VR firearms sandbox game _Hotdogs, Horseshoes & Hand Grenades_ but theoretically usable under any Unity game with BepInEx.

## Quick Links
- [Installing](https://github.com/nrgill28/Deli/wiki/Installation)
- [Project boards](https://github.com/nrgill28/Deli/projects)

## Features
- **Standardized way of loading mods**: Have a mod you want to install? Drop it in the mods folder and you're good to go.
- **Easy resource and dependency management**: Mods built for the framework can enjoy easy-to-use asset loading and dependency management built into the framework.
- **Access to internal services kernel**: The kernel is a container for a bunch of objects that can be used globaly. Stick some stuff in there to let other mods have it in a more proper way

## Advantages
The main advantages to using this framework in addition to BepInEx are as follows:
- Each Deli mod can include any number of assets of any type. You can bundle your code plugins with some default example content or even create a mega mod pack with lots of content of a certain theme. 
- Code plugins can define asset loaders that will be used to automatically load assets of a certain type from other mods. This is where the framework becomes game-independent, as anyone can write code mods to load any game-specific content.
