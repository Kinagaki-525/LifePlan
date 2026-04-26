# AGENTS.md

このリポジトリで作業する Codex は、以下の方針に従ってください。

## Scope

- このファイルは `C:\Users\PC_User\source\repos\rennsyu` 配下の作業に適用する
- 対象は `rennsyu` の ASP.NET Core MVC アプリ実装と関連する UI 修正
- 実装時は常にアーキテクチャの整合性を意識し、責務の置き場所を検討してから変更する

## Recommended Architecture

このリポジトリでは、次の構成を基準に考える。

```text
MyApp
├─ Controllers
├─ Views
├─ ViewModels
├─ Application
│  ├─ Services
│  └─ Interfaces
├─ Domain
│  ├─ Entities
│  ├─ Services
│  ├─ Logic
│  └─ Rules
├─ Infrastructure
│  ├─ Data
│  └─ Repositories
└─ Program.cs
```

## Project Context

- 技術スタックは `C#`, `ASP.NET Core MVC`, `Razor Views`, `Bootstrap`
- 既存の MVC 構成と命名規則を優先する
- 画面文言や説明は日本語を優先してよい
- 不要に概念を増やしすぎない

## General Rules

- 変更前に既存ファイル構成と関連箇所を確認する
- 必要最小限の変更で目的を達成する
- 既存フォルダ構成をむやみに変更しない
- 影響範囲が広い変更は、実装前に短く方針を説明する
- 小さな修正や明確な追加は、そのまま実装まで進めてよい
- 変更後は可能ならビルド確認を行う
- 実装だけでなく、責務の分離や依存方向が妥当かも確認する

## Layer Responsibilities

### Controllers

- HTTP リクエストを受け取り、`Application/Services` を呼び出す
- 画面遷移、入力受付、レスポンス返却に集中する
- 業務ロジックや永続化処理を直接持ち込まない
- `Infrastructure` を直接呼ばない

### Views

- Razor による表示責務のみを持つ
- `Views/<Controller名>` の既存構成に合わせる
- 既存の `Views/Shared/_Layout.cshtml` に合う構成を優先する

### ViewModels

- View に渡す画面専用データを置く
- `Domain/Entities` と役割を混在させない
- 画面表示都合のプロパティは `ViewModels` 側で扱う
- `Models` という曖昧な置き場は増やさず、画面向けは `ViewModels` に寄せる

### Application/Services

- 画面や機能単位の処理の流れを組み立てる
- `Domain/Services` や `Domain/Logic` を呼び出して処理を進める
- Repository 抽象を通してデータ取得や保存を行う
- 業務判断そのものはできるだけ `Domain` に寄せる

### Application/Interfaces

- Repository など、Application 層が利用する抽象を置く
- 実装詳細ではなく、アプリケーションから見た必要契約を表現する

### Domain/Entities

- 業務データの中心を表す
- Entity 自身に自然に属する振る舞いはここで表現する
- 画面表示専用の都合を持ち込まない

### Domain/Services

- Entity 単体では表現しづらい業務ロジックを置く
- 業務判断や複数概念にまたがるドメイン処理を担当する
- Application の手順制御と混同しない

### Domain/Logic

- 純粋関数として表現できる計算や判定を置く
- 副作用を持たない処理を優先する
- DB アクセス、外部サービス呼び出し、HTTP 依存は持ち込まない
- 入力と出力が明確で、単体テストしやすい形を優先する

### Domain/Rules

- 業務上の制約、判定ルール、ルール表現を置く
- `Logic` や `Entities` に置きづらい業務ルールを整理して配置する

### Infrastructure/Data

- DB に近い技術実装を置く
- `DbContext`, Entity Framework の設定, Migration, Seed などをここで扱う

### Infrastructure/Repositories

- `Application/Interfaces` の実装を置く
- `Infrastructure/Data` を使って取得・保存処理を実装する
- Controller や View から直接参照させない

## Dependency Direction

- `Controllers` は `Application` を呼ぶ
- `Application` は `Domain` と `Application/Interfaces` に依存する
- `Infrastructure` は `Application/Interfaces` を実装する
- `Views` は `ViewModels` を使う
- `Controllers` から `Infrastructure` を直接参照しない
- `Views` に `Domain/Entities` を直接渡さない

## DTO and Mapper Policy

- 最初から `DTOs` や `Mappers` を過剰に増やさない
- 画面向けデータはまず `ViewModels` を優先する
- Application 層の入出力整理が必要になった時点で `DTOs` を追加してよい
- 型変換が複数箇所に散り始めた時点で `Mappers` を追加してよい
- 必要になるまでは Service 内の変換で十分

## CSS and UI

- まず `wwwroot/css/site.css` の既存方針に合わせる
- 既存 UI に沿った軽微なスタイル調整を優先する
- 不要に複雑なセレクタや過剰な詳細度は避ける
- デザイン変更は既存レイアウトとの整合性を保つ

## Avoid

- 不要な大規模リファクタ
- 関係のないファイルへの横断的な変更
- 依存関係の追加を前提とした提案や実装
- 既存命名規則を無視したファイル名やアクション名の追加
- 確認なしの破壊的変更
- 責務が曖昧なままクラスやフォルダを追加すること

## Response Format

回答は次の順で簡潔にまとめる。

1. 何を変更したか
2. なぜ変更したか
3. どのファイルを触ったか
4. 確認方法