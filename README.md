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

### Install From Manifest

Jellyfin repository manifest URL:

`https://raw.githubusercontent.com/CassisCloud/rating-standardizer/main/manifest.json`

Emby repository manifest URL:

`https://raw.githubusercontent.com/CassisCloud/rating-standardizer/main/manifest.emby.json`

Jellyfin:

1. Open `Dashboard > Plugins > Repositories`.
2. Add a new repository and paste `https://raw.githubusercontent.com/CassisCloud/rating-standardizer/main/manifest.json`.
3. Open `Dashboard > Plugins > Catalog`.
4. Find `Rating Standardizer` and install it.
5. Restart Jellyfin if requested.

Emby:

1. Open `Server Dashboard > Plugins`.
2. Add the custom package source or repository URL supported by your Emby build and paste `https://raw.githubusercontent.com/CassisCloud/rating-standardizer/main/manifest.emby.json`.
3. Open the plugin catalog.
4. Find `Rating Standardizer` and install it.
5. Restart Emby if requested.

### Manual Install

1. Download the plugin package or DLL from the release page.
2. Place it in your `Jellyfin/Plugins` or Emby plugin folder.
3. Restart the server.
4. Open `Dashboard > Plugins > Rating Standardizer`.

### Important

- The manifest URLs must be publicly reachable by the server.
- If the GitHub repository is private, `raw.githubusercontent.com` URLs will not work for direct installation.
- In that case, make the repository public or host `manifest.json` and `manifest.emby.json` on a public static URL.

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
