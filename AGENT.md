# Role
あなたはJellyfinのC#/.NETプラグイン開発のエキスパートです。
Jellyfin.Controller および Jellyfin.Model ライブラリの仕様を熟知しています。

# Project Goal
Jellyfin内のメディア（映画、番組）の `OfficialRating` を、ユーザーが定義したマッピングルールに基づいて自動変換し、変更されないようにフィールドをロックするプラグイン「Rating Standardizer」を開発します。

# Tech Stack
- C# (.NET 9)  ※Jellyfin 10.11.x対応のため必ず net9.0 をターゲットにすること。
- Jellyfin Server API (v10.11.8 互換)
- HTML/JavaScript (プラグイン設定画面用)

# Architecture & Requirements

## 1. Configuration (`PluginConfiguration.cs`)
- `bool IsEnabled { get; set; }` プロパティを保持し、デフォルトを `true` にすること。
- `List<RatingMapping> Mappings` を保持すること。
- `RatingMapping` は `string OriginalRating` と `string TargetRating` を持つ。
- 日本などの主要なレーティング基準のプリセットを初期値として提供すること。

## 2. Web UI (`config.html`)
- プラグイン全体の有効/無効を切り替えるトグルスイッチ（またはチェックボックス）を最上部に配置すること。
- マッピングルールを動的に追加・削除・編集できるテーブルUIを実装すること。

## 3. Core Logic (`ServerEntryPoint.cs`)
- `IServerEntryPoint` を実装すること。
- `ILibraryManager` の `ItemAdded` と `ItemUpdated` イベントを購読すること。
- イベント発火時、**まず設定の `IsEnabled` が `true` かどうかを確認し、`false` の場合は即座に処理を抜ける（Returnする）こと。**
- `IsEnabled` が `true` の場合のみ、対象が `Video` や `Series` であり、かつ `OfficialRating` が設定されている場合に変換を試みること。

## 4. Item Update & Locking Protocol
- 変換が適用された場合、対象アイテムの `LockedFields` に `Jellyfin.Data.Enums.MetadataField.OfficialRating` を追加すること。
- データの保存は `ItemUpdateType.MetadataEdit` を使用して `ILibraryManager.UpdateItemAsync` を呼び出すこと。

## 5. Scheduled Task (`RatingStandardizerTask.cs`)
- `IScheduledTask` を実装し、既存のライブラリアイテムに対して一括適用するバッチ処理を実装すること。
- **タスク実行時も、最初に設定の `IsEnabled` を確認し、無効な場合はタスクをスキップして終了すること。**

# Rules
- 依存関係の注入 (DI) を正しく使用し、`ILogger<T>` で適切なログを出力すること。
- プラグインのGUIDは一意なものを生成して使用すること。

## 6. 設定ページのサイドバー表示
- `PluginPageInfo` の `EnableInMainMenu = true` を設定し、Jellyfin の Dashboard サイドバーに表示すること。
- `DisplayName` を設定し、ユーザーに分かりやすい名前で表示すること。
- 外部 Plugin Pages プラグインや手動 `config.json` は不要。
