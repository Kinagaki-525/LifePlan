# Spec Kitの使い方と構成

[GitHub Spec Kit](https://github.com/github/spec-kit) を Codex と Claude Code の両方から使う。手順の正本は Codex 連携が生成する `.agents/skills/speckit-*/SKILL.md`。Claude Code の `.claude/skills/speckit-*/SKILL.md` は正本を読む入口で、手順を複製しない。`review-changes` と同じ構成。

## 呼び出し方

| 手順 | Codex | Claude Code |
| --- | --- | --- |
| 憲章の作成・改訂 | `$speckit-constitution` | `/speckit-constitution` |
| 仕様 | `$speckit-specify` | `/speckit-specify` |
| 曖昧点の確認（任意） | `$speckit-clarify` | `/speckit-clarify` |
| 実装計画 | `$speckit-plan` | `/speckit-plan` |
| チェックリスト（任意） | `$speckit-checklist` | `/speckit-checklist` |
| タスク分解 | `$speckit-tasks` | `/speckit-tasks` |
| 整合性分析（任意） | `$speckit-analyze` | `/speckit-analyze` |
| 実装 | `$speckit-implement` | `/speckit-implement` |
| 未実装分のタスク追記 | `$speckit-converge` | `/speckit-converge` |
| GitHub Issue化 | `$speckit-taskstoissues` | `/speckit-taskstoissues` |

成果物は `specs/NNN-機能名/` に作られる。要件の正本は従来どおり `docs/` で、`specs/` は機能単位の作業成果物とする（憲章の原則 I）。

## 構成

| パス | 内容 | 管理 |
| --- | --- | --- |
| `.agents/skills/speckit-*` | スキルの正本（Codex 連携） | Spec Kit |
| `.claude/skills/speckit-*` | Claude Code の入口 | 手動 |
| `.specify/memory/constitution.md` | 憲章 | `speckit-constitution` |
| `.specify/templates/`、`.specify/scripts/powershell/` | テンプレートと補助スクリプト | Spec Kit |
| `.specify/integration.json`、`.specify/init-options.json` | 連携設定（既定：`codex`、スクリプト：`ps`） | Spec Kit |

Spec Kit の Claude 連携は入れていない。入れると `.claude/skills/speckit-*` が正本の複製で上書きされるため、`specify integration install claude` は実行しない。

補助スクリプトは PowerShell 版で、Windows PowerShell 5.1 でも動作する。

## CLIの導入と更新

```powershell
winget install --id astral-sh.uv -e
uv tool install specify-cli
specify integration status
```

Spec Kit を更新する場合：

```powershell
uv tool upgrade specify-cli
specify integration upgrade codex
```

更新後は、`.agents/skills/` に追加・削除されたスキルがないか確認し、あれば `.claude/skills/` の入口も合わせる。入口の書式は既存の `.claude/skills/speckit-plan/SKILL.md` に合わせる。正本と入口の差分は呼び出し記法（`$speckit-x` と `/speckit-x`）だけなので、入口側で読み替える。

## 確認記録（2026-10-10）

- Spec Kit 1.1.2。`specify integration status`：既定 `codex`、変更・欠落した管理ファイルなし。
- 既存の `AGENTS.md`、`CLAUDE.md`、`review-changes` は変更されないことを確認。
- Windows PowerShell 5.1 で `resolve-template.ps1`、`check-prerequisites.ps1`、`create-new-feature.ps1` が動作することを確認（`create-new-feature.ps1` はリポジトリの複製で実行）。
- Claude Code で `/speckit-constitution` の入口から正本スキルを読み込み、憲章 v1.0.0 を作成。
- Codex CLI 0.162.0-alpha.17.2（VS Code 拡張同梱）の App Server で、信頼済みプロジェクトの `skills/list` を実行し、`speckit-*` 10個と `review-changes` が `repo` スコープで有効、読み込みエラーなしを確認。検証用の `CODEX_HOME` を使い、利用者のグローバル設定は変更していない。Codex のセッションからスキルを実行する確認は未実施。
- Claude Code（VS Code 拡張）で、`speckit-*` 10個がスキル一覧に表示されることを確認。入口10個の frontmatter（YAML、`name` とフォルダ名の一致）、正本へのリンク、UTF-8（BOMなし）・LF を確認。
- Claude Code で `/speckit-analyze` を実行し、入口から正本を読み込み、`check-prerequisites.ps1` が機能未作成のため想定どおり停止すること、ファイルが変更されないことを確認。
