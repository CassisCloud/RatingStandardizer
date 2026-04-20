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

1. リリースページからプラグイン DLL をダウンロードします。
2. `Jellyfin/Plugins` または Emby のプラグインフォルダに配置します。
3. サーバーを再起動します。
4. `Dashboard > Plugins > Rating Standardizer` を開きます。

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
