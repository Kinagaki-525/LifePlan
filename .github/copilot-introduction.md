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
│  ├─ Interfaces
│  ├─ Validators
│  ├─ Normalizers
│  └─ Results
├─ Domain
│  ├─ Entities
│  ├─ Services
│  ├─ Logic
│  ├─ ReferenceData
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

## Document Priority

- 実装方針に迷った場合は、`docs/implementation-plan.md` より `.github/copilot-introduction.md` の責務分離・依存方向ルールを優先する
- 実装計画と作業ルールが矛盾する場合は、作業ルールに合わせて実装し、必要に応じて実装計画側の更新を提案する

## Code Readability

- コードは処理のまとまりごとに適切な空行を入れ、読みやすさを保つ
- 変数宣言、入力検証、変換処理、主要な処理、戻り値の生成など、役割が変わる箇所では空行で区切る
- 空行を入れすぎて処理の流れが分断されないようにし、論理的なまとまりを優先する
- 1つのメソッド内に複数の責務が見える場合は、空行だけで整えるのではなく、メソッド分割や Mapper / Service / Domain への責務移動を検討する
- コメントで段落を作るのではなく、まず命名・空行・小さなメソッドで意図が伝わるコードを優先する

## Naming Rules

- Service の interface / class / file 名は `{機能名}Service` を基本とする
- Service interface 名は実装クラス名に `I` を付ける（例：`ILifePlanPageService` / `LifePlanPageService`）
- Service のメソッド名には `Service` を付けず、処理内容を動詞句で表す
- メソッド名は呼び出し側で自然に読める名前にする（例：`CreateInitialPage`, `CalculateLifePlan`, `ValidateInput`）

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
- Service クラスを追加する場合は、対応する interface を必ず定義する
- Controller や他レイヤーからは、具体 Service クラスではなく interface に依存させる
- `Domain/Services` や `Domain/Logic` を呼び出して処理を進める
- Repository 抽象を通してデータ取得や保存を行う
- 業務判断そのものはできるだけ `Domain` に寄せる

### Application/Interfaces

- Application Service や Repository など、Application 層が利用する抽象を置く
- 実装詳細ではなく、アプリケーションから見た必要契約を表現する
- Service interface 名は実装クラス名に `I` を付けた名前を基本とする（例：`ILifePlanPageService` / `LifePlanPageService`）

### Application/Validators

- ViewModel など Application 層が受け取った入力値のサーバー側検証を置く
- Controller や Service に検証条件を直接増やしすぎないための置き場とする
- Domain/Rules や Domain/ReferenceData を参照して、仕様上の入力制約を確認する
- 検証結果は Controller が `ModelState` へ反映できるキーとメッセージで返す
- 画面フロー制御や Domain Entity への変換は持たない

### Application/Normalizers

- ViewModel など Application 層が受け取った入力値を、Mapper に渡す前に正規化する
- 条件付き入力の採用・無効化、未入力時の関連項目クリア、手入力値とマスタ値の優先順位解決を担当する
- Domain Entity への型変換や単位変換は持たず、Mapper の前段処理に集中する
- 入力値の妥当性検証は持たず、Validator が検証済みの入力を前提に扱う
- View 表示用の選択肢や画面状態の補完は持たない

### Application/Results

- Application Service の処理結果を表す型を置く
- 再表示用 ViewModel、検証結果、Domain 変換結果など、Service の戻り値としてまとめたいデータを扱う
- 画面入力用の FormModel ではなく、Application 層のユースケース結果として命名・配置する
- 汎用的すぎる `Models` フォルダは避け、戻り値用途は `Results` を優先する

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
- ViewModel や画面表示用の型に依存しない
- 固定マスタや参考値そのものは原則として `Domain/ReferenceData` または適切な専用フォルダに置き、`Logic` には計算処理を置く

### Domain/ReferenceData

- 計算や選択肢生成で参照する固定データ、基準値、参考値を置く
- 教育費マスタ、年金参考値など、ユーザー入力ではなくアプリ側が持つ参照用データを扱う
- 計算処理そのものは持たず、必要な場合は `Domain/Logic` から参照されるデータとして表現する
- 画面表示専用の都合や ViewModel には依存しない
- クラス名に `ReferenceData` を機械的に付けず、対象と役割が分かる名前を優先する（例：`EducationCostMaster`, `EducationCostEntry`, `PensionReferenceEntry`）

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
- `Controllers` は原則として `Application/Interfaces` の Service interface に依存する
- `Application` は `Domain` と `Application/Interfaces` に依存する
- `Infrastructure` は `Application/Interfaces` を実装する
- `Views` は `ViewModels` を使う
- `Controllers` から `Infrastructure` を直接参照しない
- `Views` に `Domain/Entities` を直接渡さない

## DTO and Mapper Policy

- View に渡す画面専用データは `ViewModels` に置く
- Domain の計算入力・出力は `Domain/Entities` または計算専用の型に置く
- ViewModel、Domain 型、DTO の責務を混在させない
- ViewModel と Domain 型の変換は `Application/Mappers` に集約する
- Service 内に複雑な型変換を直接書かない
- 条件付き入力の採用・無効化、手入力値とマスタ値の優先順位解決は `Application/Normalizers` に寄せ、Mapper は詰め替えと単位変換を中心に保つ
- DTO は外部API、永続化、Application 層の入出力が ViewModel / Domain 型だけでは表現しづらい場合に追加する
- Mapper 名は `{対象}Mapper` を基本とし、変換元と変換先が分かるメソッド名にする

## CSS and UI

- Figma由来のUI実装やCSS追加を行う場合は、`docs/ui-implementation-guidelines.md` を参照する
- Figmaから取得したReact、Tailwind、絶対配置コードはそのまま貼り付けず、Razor、Bootstrap、`wwwroot/css/site.css` に合わせて変換する
- まず `wwwroot/css/site.css` の既存方針に合わせる
- パディング、マージン、フォントサイズ、角丸、影、主要カラーは、可能な限り共通トークンまたは共通CSSクラスへ寄せる
- CSSやHTMLの重複が増えていないか確認し、同じ見た目や構造は共通化を検討する
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
