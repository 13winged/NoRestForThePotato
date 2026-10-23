# Changelog

## [0.3.5] - 2026-10-23
### Fixed
- Fixed crashes on low-end PCs with Ultra Low preset
- Added try-catch blocks to all settings methods
- Added GC.Collect() after settings apply
- Cached Camera.allCameras array to prevent repeated allocations
- Added memory logging option (LogMemoryEnabled)

### Changed
- Ultra Low preset: RenderScale 0.3 -> 0.6 (fixes blur/crash)
- Ultra Low preset: TextureQuality 2 -> 1 (fixes missing textures)
- Ultra Low preset: TargetFrameRate 30 -> 60 (fixes CPU lag)
- Particles: now enabled with 0.3 multiplier instead of fully disabled

### Added
- Detailed potato_graphics.cfg.example with all parameters documented

## [0.3.4] - 2026-10-22
### Fixed
- Fixed frame freezes caused by repeated ApplySettings() calls
- OnSceneWasLoaded no longer triggers settings re-application
- Component cache built only once at mod startup

### Added
- Forward Rendering support
- Pixel light count limit
- Target frame rate cap

## [0.3.3] - 2026-10-22
### Added
- Streaming mipmaps support
- Async upload time slice
- LOD bias adjustment
- Component cache for performance

## [0.3.2] - 2026-10-22
### Added
- External config file (potato_graphics.cfg)
- Preset system (Ultra Low, Low, Medium, High, Default)
- Launcher with PowerShell script

## [0.3.1] - 2026-10-22
### Added
- Basic mod functionality
- GUI panel (F10)
- Shadow, texture, and effect controls