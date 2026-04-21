# Rating Standardizer

[English version](README.md)

Rating Standardizer は、設定したマッピングルールに基づいてメディアのレーティングを標準化する Jellyfin / Emby 向けプラグインです。

## 機能

- アイテム追加・更新時にレーティングを自動標準化
- スケジュールタスクによる一括処理に対応
- 日本、米国、ヨーロッパ、オーストラリア向けのプリセットを内蔵
- カスタムマッピングに対応
- 対象ライブラリを限定して処理可能

## デフォルト設定

- プラグイン状態: 有効
- スケジュール: 毎週月曜 03:00（サーバーローカル時刻）
- Target Libraries: オフ
- プリセット: Custom

## インストール

### マニフェストからインストール

Jellyfin 用リポジトリマニフェスト URL:

`https://raw.githubusercontent.com/CassisCloud/rating-standardizer/main/manifest.json`

Emby 用リポジトリマニフェスト URL:

`https://raw.githubusercontent.com/CassisCloud/rating-standardizer/main/manifest.emby.json`

Jellyfin:

1. `Dashboard > Plugins > Repositories` を開きます。
2. 新しいリポジトリを追加し、`https://raw.githubusercontent.com/CassisCloud/rating-standardizer/main/manifest.json` を登録します。
3. `Dashboard > Plugins > Catalog` を開きます。
4. `Rating Standardizer` を見つけてインストールします。
5. 必要に応じて Jellyfin を再起動します。

Emby:

1. `Server Dashboard > Plugins` を開きます。
2. お使いの Emby ビルドで利用できるカスタムパッケージソースまたはリポジトリ URL に `https://raw.githubusercontent.com/CassisCloud/rating-standardizer/main/manifest.emby.json` を登録します。
3. プラグインカタログを開きます。
4. `Rating Standardizer` を見つけてインストールします。
5. 必要に応じて Emby を再起動します。

### 手動インストール

1. リリースページからプラグイン ZIP または DLL をダウンロードします。
2. `Jellyfin/Plugins` または Emby のプラグインフォルダに配置します。
3. サーバーを再起動します。
4. `Dashboard > Plugins > Rating Standardizer` を開きます。

### 重要

- サーバーから manifest URL に直接アクセスできる必要があります。
- GitHub リポジトリが private の場合、`raw.githubusercontent.com` の URL では直接インストールできません。
- その場合はリポジトリを public にするか、`manifest.json` と `manifest.emby.json` を公開された静的 URL に配置してください。

## サイドバー表示

本プラグインは、Jellyfin / Emby 標準のプラグイン設定画面としてダッシュボードのサイドバーに表示されます。

- 追加プラグインは不要です
- 表示位置: `Dashboard > Rating Standardizer`
- メニューに表示されない場合はサーバーを再起動してください

## ビルド

```bash
dotnet build RatingStandardizer.sln
```

出力例:

- Jellyfin: `RatingStandardizer.Jellyfin/bin/Debug/net9.0/Jellyfin.Plugin.RatingStandardizer.dll`
- Emby: `RatingStandardizer.Emby/bin/Debug/net8.0/Emby.Plugin.RatingStandardizer.dll`
