# Rating Standardizer

[English version](README.md)

Rating Standardizer は、国や地域が混在したメディアレーティングを内部の最低年齢モデルへ正規化し、選択した表示プリセットで出力する Jellyfin / Emby 向けプラグインです。

## 機能

- アイテム追加・更新時にレーティングを自動標準化
- スケジュールタスクによる一括処理に対応
- `TV-14`、`PG-13`、`MA15+`、`FSK-16`、`R15+`、`BR-14`、`18+` などを内部年齢へ正規化
- Age-based、My Simple Ratings、日本/映倫、米国TV、米国映画、Europe Generic、UK/BBFC、Germany/FSK、Australia の表示プリセットを内蔵
- カスタム入力ルールとカスタム表示プリセットに対応
- 元レーティング履歴を依存関係不要の分割 JSON に保存し、必要に応じて `OriginalRating:<value>` タグへミラー保存
- 対象ライブラリを限定して処理可能
- ライブラリ別に表示プリセット、曖昧なレーティング、不明レーティング、元レーティング保存先を上書き可能
- 設定と履歴を JSON バックアップとしてダウンロード/アップロード可能
- 履歴のみの export/import に対応し、Jellyfin/Emby 間の移行にも利用可能

## デフォルト設定

- プラグイン状態: 有効
- スケジュール: 毎週月曜 03:00（サーバーローカル時刻）
- Target Libraries: オフ
- 表示プリセット: Age-based
- 曖昧なレーティング: Conservative
- 不明レーティング: Keep original
- 元レーティング保存: Plugin history store + Tags

## 仕組み

処理は「入力レーティングの正規化」と「表示プリセットでの出力」に分かれています。

```text
入力レーティング -> 内部年齢 -> 表示プリセット
TV-MA            -> 18+      -> TV-MA / 18+ / R18+ / MA15+
PG-13            -> 13+      -> PG-13 / 14+ / PG12
FSK-16           -> 16+      -> FSK-16 / 16+ / R15+
```

内部表現を年齢に寄せることで、Jellyfin / Emby のメタデータに複数国の制度が混ざっても、1つの国の制度に固定せず安全に変換できます。

## 内蔵表示プリセット

- Age-based
- My Simple Ratings
- Japan / Eirin
- United States / TV
- United States / Film
- Europe / Generic
- Europe / UK BBFC
- Europe / Germany FSK
- Australia

## 曖昧なレーティング

地域によって意味がずれるレーティングは、デフォルトの `Conservative` で保護者制限向けに安全側へ寄せます。

- `TV-MA`: Conservative では `18+`、Nearest では `17+`
- `R`: Conservative では `18+`、Nearest では `17+`
- Australia の `15+`: Conservative では `MA15+`

## カスタムルールとカスタムプリセット

カスタム入力ルールでは、メタデータ上の文字列を何歳以上として扱うかを定義できます。例: `KR-15`、`KMRB-15`、`15세이상관람가` を `15+` として扱う。

Alias ルールは指定した文字列に一致します。Regex ルールでは capture group から年齢を抽出できます。

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

設定画面では保存前に、alias の有無、regex 構文、年齢範囲、表示ラベル、alias 重複などを検証します。カスタムルールとカスタムプリセットは JSON で import / export できます。

カスタム表示プリセットでは、内部年齢をどのラベルで表示するかを定義できます。例:

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

設定画面にはテスト変換機能があり、適用前に一致したルール、内部年齢、選択プリセット、最終レーティングを確認できます。

## ライブラリ別上書き

Target Libraries は、そのライブラリを処理対象にするかどうかを決めます。Library Overrides は、処理対象になったライブラリの変換方法だけを変更します。

ライブラリ別に上書きできる項目:

- 表示プリセット
- 曖昧なレーティングの扱い
- 不明レーティングの扱い
- 元レーティング保存モード

Target Libraries が有効で対象外になっているライブラリは、Library Override が設定されていてもスキップされます。

## 元レーティング履歴

元レーティング履歴は、依存関係不要の分割 JSON に保存します。Tags は有効化した場合の可視化用ミラーです。

保存モード:

- Plugin history store + Tags
- Plugin history store only
- Tags only
- Disabled

履歴には item id、path、name、production year、provider ids、元 official rating、元 custom rating、一致したルール、内部年齢、最終レーティング、プリセット、変換ポリシー、プラグインバージョン、更新時刻を保存します。

履歴は `DataPath/RatingStandardizer/history/records` 以下の 256 個の shard JSON に分割して保存し、`DataPath/RatingStandardizer/history/indexes` 以下に索引 JSON を保存します。1件の更新では対象 shard と索引だけを書き換えます。既に履歴がある場合、元レーティングは後続更新でなるべく上書きしません。

設定画面の Data Migration から、設定と履歴をまとめたバックアップ JSON をダウンロード/アップロードできます。履歴のみの export/import も可能です。バックアップの復元はプラグイン設定を更新し、同じ item id の履歴はインポート内容で更新されます。

## Custom Rule の項目

設定画面では、よく使う項目だけを表示します。

- Enabled: ルールの有効/無効。
- Name: ルールの表示名。
- Type: 完全一致の `Alias`、または正規表現の `Regex`。
- Match value: Alias の場合はカンマ区切りの入力文字列、Regex の場合は正規表現。
- Treat as: 内部的に何歳以上として扱うか。
- Age group: Regex ルール専用の項目です。年齢が入っている capture group 番号を指定します。例: `^BR-(\d{1,2})$` なら group `1` から `BR-14` の `14` を読み取ります。
- Priority: 複数ルールに一致した場合の優先度。高い方が優先されます。

任意の詳細項目は `Show advanced rule options` で表示できます。

- Conservative: 全体ポリシーが `Conservative` の場合に `Treat as` を上書きします。例: `R -> 18+` のように安全側へ寄せたい場合に使います。
- Lenient: 全体ポリシーが `Lenient` の場合に `Treat as` を上書きします。緩めの解釈にしたい場合に使います。
- Restriction: 元レーティングが advisory、restricted、adult、unknown のどれに近いかを記録します。設定や履歴に残り、将来のフィルタや診断に使えますが、それ自体で出力ラベルは変わりません。

詳細項目は必須ではありません。空欄の場合、すべての変換ポリシーで `Treat as` が使われ、Restriction は `unknown` として扱われます。

## インストール

### Jellyfin

Jellyfin 用リポジトリマニフェスト URL:

`https://raw.githubusercontent.com/CassisCloud/RatingStandardizer/main/manifest.json`

1. `Dashboard > Plugins > Repositories` を開きます。
2. 新しいリポジトリを追加し、`https://raw.githubusercontent.com/CassisCloud/RatingStandardizer/main/manifest.json` を登録します。
3. `Dashboard > Plugins > Catalog` を開きます。
4. `Rating Standardizer` を見つけてインストールします。
5. 必要に応じて Jellyfin を再起動します。

### Emby

Emby は Jellyfin のようにリポジトリマニフェスト URL を登録してこのプラグインをインストールする方式ではありません。手動でインストールしてください。

1. リリースページから Emby 用プラグイン ZIP または DLL をダウンロードします。
2. Emby サーバーのプラグインフォルダに配置します。
3. サーバーを再起動します。
4. `Dashboard > Plugins > Rating Standardizer` を開きます。

## ビルド

```bash
dotnet build RatingStandardizer.sln
```

出力例:

- Jellyfin: `RatingStandardizer.Jellyfin/bin/Debug/net9.0/Jellyfin.Plugin.RatingStandardizer.dll`
- Emby: `RatingStandardizer.Emby/bin/Debug/net8.0/Emby.Plugin.RatingStandardizer.dll`
