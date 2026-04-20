# Role
あなたは C# と .NET エコシステムに精通した、Jellyfin および Emby プラグイン開発のシニアアーキテクトです。

# Project Goal
メディアの `OfficialRating` を自動変換・ロックするプラグイン「Rating Standardizer」を開発します。
単一のコードベースから、Jellyfin用とEmby用の2つのプラグインをビルドできる「マルチプロジェクト（クリーンアーキテクチャ）構成」を構築することが目標です。

# Solution Structure
以下の3つのプロジェクトを含むソリューション `RatingStandardizer.sln` を作成してください。

## 1. RatingStandardizer.Core
- ターゲット: `netstandard2.0`
- 役割: Jellyfin/Emby APIに一切依存しない純粋なドメインロジック。
- 実装内容:
  - `RatingMapping` モデル (OriginalRating, TargetRating)
  - 日本の映倫基準（G, PG12, R15+, R18+, NR）への変換を行う `RatingConverter` クラス。
  - プラグインの有効/無効、マッピングリストを保持する設定用インターフェース。

## 2. RatingStandardizer.Jellyfin
- ターゲット: `net9.0`
- 参照: `RatingStandardizer.Core` プロジェクト
- 依存NuGet: `Jellyfin.Controller` (v10.11.8互換)
- 実装内容:
  - `IPlugin`, `IServerEntryPoint` の実装。
  - Jellyfinの `ILibraryManager.ItemAdded/ItemUpdated` イベントの購読。
  - Coreの `RatingConverter` を呼び出し、Jellyfinのアイテムを更新＆ロック（`LockedFields` の更新）する処理。
  - Jellyfin用の設定画面 (HTML/JS) の提供。

## 3. RatingStandardizer.Emby
- ターゲット: `net8.0` (Emby 4.9.3 の .NET 8 環境に合わせるため)
- 参照: `RatingStandardizer.Core` プロジェクト
- 依存NuGet: `MediaBrowser.Server.Core` (Version 4.9.3.x 等、対象バージョンに適合するもの)
- 実装内容:
  - Embyのプラグインエントリーポイントの実装。
  - Embyの `ILibraryManager` を使ったイベント購読とメタデータ更新処理。
  - Emby用の設定ページ (Embyの独自UIフレームワークに準拠) の提供。

# Implementation Rules
- 必ず最初に `dotnet new sln` と各プロジェクトを作成し、互いの参照（`dotnet add reference`）を設定するスクリプトを生成・実行してください。
- 各プラットフォームの `ILibraryManager` の仕様の違い（名前空間の違いや、ロック機能のプロパティ名の違いなど）に注意して実装を分けてください。