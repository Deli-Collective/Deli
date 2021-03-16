# Deli
![latest version](https://img.shields.io/github/v/release/Deli-Collective/Deli?label=latest&style=flat-square)
[![Discord](https://img.shields.io/discord/777351065950879744?label=&logo=discord&logoColor=ffffff&color=7389D8&labelColor=6A7EC2&style=flat-square)](https://discord.gg/g8xeFyt42j)

Deli is a modding framework which provides a uniform way of packaging and loading mods, each containing any type or number of
assets. While it was originally made for the popular VR firearms sandbox game _Hotdogs, Horseshoes & Hand Grenades_, its dependency on
BepInEx means it theoretically works for any Unity game.

## Quick Links
- [Installing](https://github.com/Deli-Collective/Deli/wiki/Installation)
- [Milestones](https://github.com/Deli-Collective/Deli/milestones) (i.e. plans for future updates)
- [Join us on Discord for discussions, updates and support](https://discord.gg/g8xeFyt42j)

## Features
- **Standardized mod installation**: Have a mod you want to install? Drop it in the mods folder and you're good to go.
- **Automatic version checking**: Given that each mod provides enough info, Deli automatically checks for mod updates.
- **Simple dependency management**: Basic dependency management is built into the framework. If a dependency is not present, an error is given and the dependant mod does not load.
- **Easy access to assets**: Each mod is given a filesystem tree to access its assets from, allowing mods to dynamically load content themselves.

## Documentation
User documentation for the latest release can be found at [the Deli wiki](https://github.com/Deli-Collective/Deli/wiki).
This wiki can be updated by community members, and may be out of date shortly after a release.   
Developer documentation for the latest pre-release can be found at [the Deli website](https://deli-collective.github.io/Deli).
This website is tied directly to the repository, and should be up to date with the latest release by the time of release. If for you wish
to view the documentation for earlier versions of Deli, you must clone the repo, download and extract the DocFX binaries to `docs/bin/`,
and serve the website yourself.
