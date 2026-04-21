# Rating Standardizer

[日本語版はこちら](README.ja.md)

Rating Standardizer is a Jellyfin / Emby plugin that normalizes media ratings based on configurable mapping rules.

## Features

- Automatically standardizes ratings when items are added or updated
- Supports batch processing through a scheduled task
- Includes built-in presets for Japan, United States, Europe, and Australia
- Supports fully custom mappings
- Can be limited to selected libraries only

## Installation

### Jellyfin

Jellyfin repository manifest URL:

`https://raw.githubusercontent.com/CassisCloud/RatingStandardizer/main/manifest.json`

1. Open `Dashboard > Plugins > Repositories`.
2. Add a new repository and paste `https://raw.githubusercontent.com/CassisCloud/RatingStandardizer/main/manifest.json`.
3. Open `Dashboard > Plugins > Catalog`.
4. Find `Rating Standardizer` and install it.
5. Restart Jellyfin if requested.

The Jellyfin release package is a zip file intended for repository-based installation.

### Emby

Emby should be installed manually.

1. Stop Emby Server.
2. Download `Emby.Plugin.RatingStandardizer.dll` from the latest release.
3. Open your Emby Server data folder and locate the `plugins` directory.
4. If you are replacing an older version, rename the existing `Emby.Plugin.RatingStandardizer.dll` first so you can roll back if needed.
5. Copy the new `Emby.Plugin.RatingStandardizer.dll` into the `plugins` directory.
6. Start Emby Server again.
7. Open `Server Dashboard > Plugins` and confirm that `Rating Standardizer` appears.

Notes:

- On Linux or NAS systems, make sure the new DLL uses the same ownership and permissions as your other Emby plugin DLLs.
- Emby's manual plugin installation flow does not guarantee that catalog-style images or preview artwork will be shown for manually installed plugins.
- For the official Emby plugin documentation and server data folder details, refer to the Emby documentation site.

### Important

- The Jellyfin manifest URL must be publicly reachable by the server.
- If the repository is private, `raw.githubusercontent.com` URLs will not work for direct Jellyfin installation.
- In that case, make the repository public or host `manifest.json` on a public static URL.

## Release Assets

- Jellyfin: `ratingstandardizer-jellyfin_<version>.zip`
- Emby: `Emby.Plugin.RatingStandardizer.dll`

## Build

```bash
dotnet build RatingStandardizer.sln
```

Example output:

- Jellyfin: `RatingStandardizer.Jellyfin/bin/Debug/net9.0/Jellyfin.Plugin.RatingStandardizer.dll`
- Emby: `RatingStandardizer.Emby/bin/Debug/net8.0/Emby.Plugin.RatingStandardizer.dll`
