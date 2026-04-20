# Rating Standardizer

Jellyfin プラグイン：メディアのレーティングを設定したマッピングに基づいて自動変換します。

## 機能

- メディア追加/更新時の自動レーティング変換
- バッチ処理による一括変換（スケジュールタスク）
- プリセット対応（日本・米国・オーストラリア・ヨーロッパ）
- カスタムマッピング対応

## インストール

1. リリースページから DLL をダウンロード
2. `Jellyfin/Plugins` フォルダに配置
3. Jellyfin を再起動
4. ダッシュボード → プラグイン → Rating Standardizer で設定

## サイドバー表示

本プラグインは Jellyfin 標準の `EnableInMainMenu` を使って、設定画面（Dashboard）のサイドバーに表示されます。

- 追加プラグインは不要です
- 表示位置: **Dashboard > Rating Standardizer**
- メニューに表示されない場合は Jellyfin を再起動してください

## ビルド

```bash
dotnet build RatingStandardizer.Jellyfin
```

出力: `bin/Debug/net9.0/Jellyfin.Plugin.RatingStandardizer.dll`
