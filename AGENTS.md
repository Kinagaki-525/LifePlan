# AGENTS.md

このリポジトリで Codex が作業する際のルールです。

## Language

- 回答・説明は日本語を優先する。

## Source of Truth

- 作業ルールと責務分離は `.github/copilot-introduction.md` を優先する。
- 要件仕様は `docs/index.md` を正とする。
- 実装方針・PR分割は `docs/implementation-plan.md` を参照する。

## Conflict Resolution

- 各ドキュメントに矛盾がある場合は、実装前に関連箇所を確認する。
- 優先順位だけで安全に判断できる軽微な差分は、以下の順で判断する。
  1. `.github/copilot-introduction.md`
  2. `docs/index.md`
  3. `docs/implementation-plan.md`
- 仕様・責務・画面挙動・データ構造に影響する矛盾がある場合は、実装前にユーザーへ質問する。
- 判断に迷う場合も、推測で進めずユーザーへ確認する。

## Working Rules

- 変更前に既存コードと関連ファイルを確認する。
- 必要最小限の変更で目的を達成する。
- 不要な大規模リファクタは行わない。
- 関係のないファイルへ変更を広げない。
- 破壊的操作は明示的な依頼がない限り行わない。
- 既存の命名規則、コードスタイル、ディレクトリ構成を尊重する。

## Verification

- 変更後は可能なら `dotnet build LifePlan.sln -m:1` を実行する。
- 計算ロジックやテストに関わる変更後は可能なら `dotnet test LifePlan.sln -m:1` を実行する。
- NuGet 脆弱性データ取得警告など、ネットワーク制限由来の警告が出た場合は、ビルド・テスト結果と分けて報告する。
