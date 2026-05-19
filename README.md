# Rating Standardizer

[日本語版はこちら](README.ja.md)

Rating Standardizer is a Jellyfin / Emby plugin that normalizes mixed regional media ratings to an internal minimum-age model, then renders that age through a selected display preset.

## Features

- Automatically standardizes ratings when items are added or updated
- Supports batch processing through a scheduled task
- Normalizes inputs such as `TV-14`, `PG-13`, `MA15+`, `FSK-16`, `R15+`, `BR-14`, and `18+` to an internal age
- Includes output presets for Age-based, My Simple Ratings, Japan/Eirin, United States TV, United States Film, Europe Generic, UK/BBFC, Germany/FSK, and Australia
- Supports custom input rules and custom output presets
- Stores original rating history in dependency-free sharded JSON files and optionally mirrors it to tags as `OriginalRating:<value>`
- Can be limited to selected libraries only
- Supports per-library overrides for output preset, ambiguous mapping, unknown rating behavior, and original rating storage
- Downloads/uploads JSON backups containing plugin configuration and rating history
- Exports/imports rating history only for migration between Jellyfin and Emby installs

## Default Settings

- Plugin state: enabled
- Scheduled task: every Monday at 03:00 (server local time)
- Target Libraries: off
- Output preset: Age-based
- Ambiguous mapping: Conservative
- Unknown rating behavior: Keep original
- Original rating storage: Plugin history store + Tags

## How It Works

Ratings are processed in two separate steps:

```text
Input rating -> minimum age -> output preset
TV-MA        -> 18+         -> TV-MA / 18+ / R18+ / MA15+
PG-13        -> 13+         -> PG-13 / 14+ / PG12
FSK-16       -> 16+         -> FSK-16 / 16+ / R15+
```

The internal age model avoids locking the plugin to one country's rating system. This is safer for Jellyfin and Emby libraries where metadata from multiple countries can be mixed.

## Built-In Output Presets

- Age-based
- My Simple Ratings
- Japan / Eirin
- United States / TV
- United States / Film
- Europe / Generic
- Europe / UK BBFC
- Europe / Germany FSK
- Australia

## Ambiguous Ratings

Some ratings have different meanings depending on region. The default `Conservative` mode favors safer parental-control behavior.

- `TV-MA`: `18+` in Conservative, `17+` in Nearest
- `R`: `18+` in Conservative, `17+` in Nearest
- Australia `15+`: `MA15+` in Conservative

## Custom Rules And Presets

Custom input rules define how raw metadata should be interpreted. Example: treat `KR-15`, `KMRB-15`, and `15세이상관람가` as `15+`.

Alias rules match one or more exact rating strings. Regex rules can extract the age from a capture group:

```json
[
  {
    "id": "custom_br_age",
    "name": "Brazil age notation",
    "matchType": "Regex",
    "pattern": "^BR-(\\d{1,2})$",
    "minimumAgeFromGroup": 1,
    "minimumAge": 0,
    "system": "CUSTOM_BR",
    "priority": 1000,
    "enabled": true
  }
]
```

The settings page validates custom rules and presets before saving, including alias presence, regex syntax, age bounds, render labels, and duplicate aliases. It also supports JSON import/export for custom rules and custom presets.

Custom output presets define how internal ages should be displayed. Example:

```json
[
  {
    "id": "custom_simple",
    "name": "My Simple Ratings",
    "render": [
      { "min": 0, "max": 11, "label": "ALL" },
      { "min": 12, "max": 14, "label": "TEEN" },
      { "min": 15, "max": 17, "label": "MATURE" },
      { "min": 18, "max": 99, "label": "ADULT" }
    ]
  }
]
```

The settings page includes a test conversion tool so you can verify the matched rule, internal age, selected output preset, and final rating before running a batch update.

## Library Overrides

Target Libraries decides whether a library is processed. Library Overrides only change how a processed library is converted.

Per-library overrides can change:

- Output preset
- Ambiguous mapping policy
- Unknown rating behavior
- Original rating storage mode

If Target Libraries is enabled and a library is not selected, that library is skipped even when it has an override configured.

## Original Rating History

The plugin stores original rating history in dependency-free sharded JSON files. Tags are only a mirror for visibility when enabled.

Storage modes:

- Plugin history store + Tags
- Plugin history store only
- Tags only
- Disabled

The history store keeps item id, path, name, production year, provider ids, original official rating, original custom rating, matched rule, normalized age, final rating, preset, mapping mode, plugin version, and update time.

History records are split across 256 shard files under `DataPath/RatingStandardizer/history/records`, with indexes under `DataPath/RatingStandardizer/history/indexes`. A single item update only rewrites its shard and indexes. Original ratings are preserved on later updates when a record already exists.

The Data Migration section can download/upload a backup JSON containing both plugin configuration and rating history. It can also export/import history only. Restoring a backup updates plugin configuration and imports history; existing history with the same item id is updated from the imported data.

## Custom Rule Fields

The settings page shows only the common fields:

- Enabled: turns the rule on or off.
- Name: a human-readable label.
- Type: `Alias` for exact strings, or `Regex` for a regular expression.
- Match value: aliases separated by commas, or the regex pattern.
- Treat as: the internal minimum age.
- Age group: for Regex rules only, the capture group number that contains the age. For example, `^BR-(\d{1,2})$` uses group `1` to read `14` from `BR-14`.
- Priority: higher custom priority wins when multiple rules match.

Optional advanced fields can be shown with `Show advanced rule options`:

- Conservative: overrides `Treat as` when the global policy is `Conservative`. Use it to make ambiguous ratings safer, such as `R -> 18+`.
- Lenient: overrides `Treat as` when the global policy is `Lenient`. Use it when you want a relaxed interpretation.
- Restriction: records whether the source rating is advisory, restricted, adult, or unknown. This is stored in configuration/history and is useful for future filtering or diagnostics, but it does not change the output label by itself.

Advanced fields are optional. If left empty, the plugin uses `Treat as` for all mapping policies and `unknown` for restriction.

## Installation

### Jellyfin

Jellyfin repository manifest URL:

`https://raw.githubusercontent.com/CassisCloud/RatingStandardizer/main/manifest.json`

1. Open `Dashboard > Plugins > Repositories`.
2. Add a new repository and paste `https://raw.githubusercontent.com/CassisCloud/RatingStandardizer/main/manifest.json`.
3. Open `Dashboard > Plugins > Catalog`.
4. Find `Rating Standardizer` and install it.
5. Restart Jellyfin if requested.

### Emby

Emby does not install this plugin from a Jellyfin-style repository manifest. Install it manually:

1. Download the Emby plugin package or DLL from the release page.
2. Place it in your Emby server plugin folder.
3. Restart the server.
4. Open `Dashboard > Plugins > Rating Standardizer`.

## Build

```bash
dotnet build RatingStandardizer.sln
```

Example output:

- Jellyfin: `RatingStandardizer.Jellyfin/bin/Debug/net9.0/Jellyfin.Plugin.RatingStandardizer.dll`
- Emby: `RatingStandardizer.Emby/bin/Debug/net8.0/Emby.Plugin.RatingStandardizer.dll`
