# LifePlan Constitution

本書は Spec Kit（`$speckit-*` / `/speckit-*`）が参照する LifePlan の原則である。
ルール本文の正本は `.github/copilot-introduction.md` と `AGENTS.md` にあり、本書はそれらを複製せず、
仕様・計画・タスク・実装の各段階で照合すべき要点と参照先を定める。

## Core Principles

### I. 仕様の正本は docs/

- 要件仕様の正本は `docs/` 配下とする（シミュレーター：`docs/simulator/index.md`、
  記事表示：`docs/articles/feature-spec.md`、UI：`docs/common/ui-implementation-guidelines.md`）。
  実装方針は `docs/simulator/implementation-plan.md`、`docs/articles/implementation-plan.md` を参照する。
- `specs/NNN-*/`（spec.md・plan.md・tasks.md など）は機能単位の作業成果物であり、MUST `docs/` の正本に従う。
  spec.md は正本の該当箇所を参照し、要件を独自に再定義してはならない。
- 作業中に要件の追加・変更が必要になった場合は、反映先と内容を説明してユーザーの了承を得てから `docs/` に反映する。
  `specs/` だけを更新して要件変更を完了扱いにしてはならない。
- 正本同士、または正本と `specs/` の間で仕様・責務・画面挙動・データ構造に影響する矛盾がある場合は、
  推測で進めず MUST ユーザーへ確認する。未確定事項は機能ごとの既存ファイルで管理する
  （シミュレーター：`docs/simulator/specs/open-issues.md`、記事表示：`docs/articles/open-issues.md`）。
  リポジトリ直下の `specs/` には置かない。

理由：仕様の置き場が二重化すると、どちらが正しいかを判断できなくなるため。

### II. レイヤー責務と依存方向

- 責務の置き場所と依存方向は MUST `.github/copilot-introduction.md` の Layer Responsibilities、
  Dependency Direction、DTO and Mapper Policy に従う。
- 要点：Controller は Application の Service interface に依存し、Infrastructure を直接参照しない。
  業務判断は Domain に寄せる。View には Domain Entity ではなく ViewModel を渡す。
  型変換は Mapper、入力の正規化は Normalizer、検証は Validator に置く。
- plan.md では、追加・変更する型ごとに配置レイヤーを明記し、上記と照合する。

理由：責務分離が崩れると、テスト容易性とレビュー効率が継続的に下がるため。

### III. 計算ロジックのテスト

- `Domain/Logic` の計算ロジックや `Domain/Rules` を変更する場合は、MUST `LifePlan.Tests`（xUnit）の
  単体テストを追加・更新する。テストは本体の名前空間・フォルダ構成に対応させて配置する。
- 期待値は仕様の正本から導き、境界値（年齢・利率・期間の両端）、単位変換（万円⇔円）、丸めを含める。
- Application Service・Validator・Normalizer は、分岐や画面フローへの影響が増える場合にテストを追加する。
  UI・Controller のテストは、検証観点が明確になった時点で別途方針化する。
- tasks.md には、上記に該当する変更ごとにテスト作成タスクを含める。

理由：シミュレーターの価値は計算結果の正しさに依存するため。

### IV. 最小変更と事前確認

- 必要最小限の変更で目的を達成する。不要な大規模リファクタ、関係のないファイルへの横断的な変更をしない。
- 既存の命名規則（例：`{機能名}Service` と `I{機能名}Service`）、コードスタイル、ディレクトリ構成を MUST 尊重する。
- 新しい依存パッケージの追加、破壊的操作、影響範囲が広い変更は、MUST 実装前に方針を説明して確認を得る。
- ガイドライン・仕様書への追記は、追記先と内容を説明して了承を得てから行う。

理由：小さく確認可能な差分を保つことで、レビューと回帰の検出を容易にするため。

### V. 検証とメッセージの正本

- 入力検証の正本はサーバー側 Validator とする。クライアント側検証は UX 補助に限り、異なる条件で再実装しない。
- 入力バリデーションのエラーメッセージは MUST `LifePlanValidationMessages` を正本とし、
  View や `wwwroot/js` に文言を直接書かない。
- 業務ルール・計算・認可を JavaScript に持たせない。

理由：検証条件と文言の二重管理による不整合を防ぐため。

## 技術的制約

- 技術スタックは C#、ASP.NET Core MVC、Razor Views、Bootstrap とし、既存の MVC 構成を優先する。
- UI 実装は `docs/common/ui-implementation-guidelines.md` と `wwwroot/css/site.css` の既存方針に合わせる。
  Figma 由来のコードはそのまま貼り付けず変換する。
- アフィリエイトリンクは設定値と ViewModel 経由で扱う（詳細は `.github/copilot-introduction.md` の Affiliate Links）。
- テキストファイルの改行コードは LF とする（`.gitattributes` の `* text=auto eol=lf` を正とする）。
- 画面文言や説明、Spec Kit の成果物（spec.md・plan.md・tasks.md）は日本語を優先する。

## 開発ワークフローと品質ゲート

- Spec Kit の正本スキルは `.agents/skills/speckit-*`（Codex 連携、Spec Kit が管理）とする。
  Claude Code の `.claude/skills/speckit-*` は正本を読む入口であり、手順を複製しない。
- 標準の流れ：`speckit-specify` →（必要に応じて `speckit-clarify`）→ `speckit-plan` → `speckit-tasks`
  →（必要に応じて `speckit-analyze`）→ `speckit-implement`。
- `speckit-plan` の Constitution Check では、原則 I〜V を照合し、違反がある場合は理由と代替案を記録する。
- 実装後は MUST `dotnet build LifePlan.slnx -m:1` を実行し、計算ロジックやテストに関わる変更では
  `dotnet test LifePlan.slnx -m:1` も実行する。ネットワーク制限由来の警告は、ビルド・テスト結果と分けて報告する。
- 変更レビューは `review-changes` スキル（Codex：`$review-changes`、Claude Code：`/review-changes`）または
  `change-reviewer` エージェントで行う。

## Governance

- 作業ルールの優先順位は `AGENTS.md` の Conflict Resolution に従い、`.github/copilot-introduction.md` が最優先である。
  本書とそれらが矛盾する場合は正本に従い、本書を改訂する。
- 本書の改訂は、変更内容と理由を説明してユーザーの了承を得てから `$speckit-constitution`
  （Claude Code では `/speckit-constitution`）で行う。
- バージョンはセマンティックバージョニングに従う。MAJOR：原則の削除・再定義、MINOR：原則・節の追加や大幅な拡張、
  PATCH：文言の明確化・誤字修正。
- plan.md の Constitution Check、`speckit-analyze`、変更レビューで本書への準拠を確認する。

**Version**: 1.0.1 | **Ratified**: 2026-10-10 | **Last Amended**: 2026-10-10
