# Changelog
All notable changes to this project will be documented in this file.  
This file was added after [0.3.1] (2021-03-22), and as such will only reflect changes after that version.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic 
Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]
### Added
- LTS notice to `README.md`
- Backwards compatibility with mods that require `0.3.x`
### Changes
- Paths
  - Configs are now saved to `BepInEx/configs/Deli/`
  - The cache is now saved to `BepInEx/patchers/DeliTeam-Deli/cache/`
  - Zip mods are now loaded from `BepInEx/plugins/*/**/`, e.g. `BepInEx/plugins/MyName-MyMod/MyMod.deli`
  - Directory mods are now loaded from `BepInEx/plugins/*/*/**/`, e.g. `BepInEx/plugins/MyName-MyMod/DeliMod/manifest.json`
### Removed
- Discord invite from `README.md`

## [0.3.2]
### Fixed
- Globs no longer match handles with names that contain, but do not match, the glob [#28]
- `deli:monomod.hookgen` no longer throws exceptions when non-Deli MonoMod patches are present [#27]

[unreleased]: https://github.com/Deli-Collective/Deli/compare/v0.3.2...HEAD
[0.3.2]: https://github.com/Deli-Collective/Deli/compare/v0.3.1...v0.3.2
[0.3.1]: https://github.com/Deli-Collective/Deli/tree/v0.3.1
