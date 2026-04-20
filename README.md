# Rating Standardizer

[日本語版はこちら](README.ja.md)

Rating Standardizer is a Jellyfin / Emby plugin that normalizes media ratings based on configurable mapping rules.

## Features

- Automatically standardizes ratings when items are added or updated
- Supports batch processing through a scheduled task
- Includes built-in presets for Japan, United States, Europe, and Australia
- Supports fully custom mappings
- Can be limited to selected libraries only

## Default Settings

- Plugin state: enabled
- Scheduled task: every Monday at 03:00 (server local time)
- Target Libraries: off
- Preset: Custom

## Installation

1. Download the plugin DLL from the release page.
2. Place it in your `Jellyfin/Plugins` or Emby plugin folder.
3. Restart the server.
4. Open `Dashboard > Plugins > Rating Standardizer`.

## Sidebar Menu

The plugin uses the standard Jellyfin / Emby plugin configuration entry to appear in the dashboard sidebar.

- No extra plugin is required
- Location: `Dashboard > Rating Standardizer`
- If the menu does not appear, restart the server

## Build

```bash
dotnet build RatingStandardizer.sln
```

Example output:

- Jellyfin: `RatingStandardizer.Jellyfin/bin/Debug/net9.0/Jellyfin.Plugin.RatingStandardizer.dll`
- Emby: `RatingStandardizer.Emby/bin/Debug/net8.0/Emby.Plugin.RatingStandardizer.dll`
