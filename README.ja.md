# Rating Standardizer

[English version](README.md)

Rating Standardizer は、設定したマッピングルールに基づいてメディアのレーティングを標準化する Jellyfin / Emby 向けプラグインです。

## 機能

- アイテム追加・更新時にレーティングを自動標準化
- スケジュールタスクによる一括処理に対応
- 日本、米国、ヨーロッパ、オーストラリア向けのプリセットを内蔵
- カスタムマッピングに対応
- 対象ライブラリを限定して処理可能

## インストール

### Jellyfin

Jellyfin 用リポジトリマニフェスト URL:

`https://raw.githubusercontent.com/CassisCloud/RatingStandardizer/main/manifest.json`

1. `Dashboard > Plugins > Repositories` を開きます。
2. 新しいリポジトリを追加し、`https://raw.githubusercontent.com/CassisCloud/RatingStandardizer/main/manifest.json` を登録します。
3. `Dashboard > Plugins > Catalog` を開きます。
4. `Rating Standardizer` を見つけてインストールします。
5. 必要に応じて Jellyfin を再起動します。

Jellyfin のリリース成果物は、リポジトリ経由インストール向けの zip です。

### Emby

Emby は手動でインストールしてください。

1. Emby Server を停止します。
2. 最新リリースから `Emby.Plugin.RatingStandardizer.dll` をダウンロードします。
3. Emby Server の data folder 配下にある `plugins` ディレクトリを開きます。
4. 既存バージョンを更新する場合は、現在の `Emby.Plugin.RatingStandardizer.dll` を先にリネームして退避します。
5. 新しい `Emby.Plugin.RatingStandardizer.dll` を `plugins` ディレクトリにコピーします。
6. Emby Server を起動します。
7. `Server Dashboard > Plugins` を開き、`Rating Standardizer` が表示されることを確認します。

補足:

- Linux や NAS では、配置した DLL の所有者と権限を既存の Emby plugin DLL と揃えてください。
- 手動インストールされた Emby plugin に対して、カタログ用の画像やプレビュー画像が表示されることは保証できません。
- Emby Server の data folder の場所は、Emby 公式ドキュメントを参照してください。

### 重要

- Jellyfin の manifest URL は、サーバーから直接アクセスできる必要があります。
- リポジトリが private の場合、`raw.githubusercontent.com` の URL では Jellyfin に直接追加できません。
- その場合はリポジトリを public にするか、`manifest.json` を公開された静的 URL に配置してください。

## リリース成果物

- Jellyfin: `ratingstandardizer-jellyfin_<version>.zip`
- Emby: `Emby.Plugin.RatingStandardizer.dll`

## ビルド

```bash
dotnet build RatingStandardizer.sln
```

出力例:

- Jellyfin: `RatingStandardizer.Jellyfin/bin/Debug/net9.0/Jellyfin.Plugin.RatingStandardizer.dll`
- Emby: `RatingStandardizer.Emby/bin/Debug/net8.0/Emby.Plugin.RatingStandardizer.dll`
