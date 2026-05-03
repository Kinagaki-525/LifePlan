# LifePlan

ASP.NET Core MVC と Razor Views を使ったライフプランシミュレーターアプリです。

## 概要

このリポジトリは、家族構成、収入、支出、ライフイベントをもとに将来のキャッシュフローと貯蓄推移を確認するためのプロジェクトです。
既存の MVC 構成を尊重しつつ、責務が分かりやすい構成を保つことを目的としています。

## ディレクトリ方針

- `LifePlan/` : アプリケーション本体
- `docs/` : 設計メモ、画面仕様、作業メモなどのドキュメント

## 開発方針

- 既存の命名規則とディレクトリ構成を優先する
- 必要最小限の変更で目的を達成する
- 関係のないファイルへ波及する変更は避ける
- Service クラスを追加する場合は、対応する interface を `Application/Interfaces` または適切な Interfaces フォルダに定義する
- Service の interface / class / file 名は `{機能名}Service` を基本とする（例：`ILifePlanPageService.cs`, `LifePlanPageService.cs`）
- Service のメソッド名には `Service` を付けず、処理内容を動詞句で表す
- Controller や他レイヤーから Service を利用する場合は、原則として interface 経由で依存させる
- ViewModel、Domain 型、DTO の責務を混在させず、境界をまたぐ変換は Mapper に集約する
- 可能なら変更後にビルド確認を行う

## Codex / Copilot 向け作業ルール

Codex や GitHub Copilot でこのリポジトリを扱う場合は、`.github/copilot-introduction.md` を参照してください。

## ドキュメント

ドキュメントは `docs/` 配下に配置します。

例:

- 画面仕様
- 設計メモ
- 作業ログ
- 運用メモ

## セットアップ

必要に応じて追記してください。

## 実行方法

必要に応じて追記してください。

## 補足

詳細なメモや今後の方針は `docs/` 配下に整理してください。
