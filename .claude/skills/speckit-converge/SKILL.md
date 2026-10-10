---
name: speckit-converge
description: "Assess the current codebase against the feature's spec, plan, and tasks, then append any remaining unbuilt work as new tasks to tasks.md so implement can complete it."
argument-hint: "Optional focus or constraints for the convergence check"
---

# Spec Kit：speckit-converge の入口

最初に [共通スキル](../../../.agents/skills/speckit-converge/SKILL.md) を読み、その手順に従う。手順の正本は共通スキルにあり、Spec KitのCodex連携が管理する。

- 共通スキルの「User Input」欄（引数のプレースホルダー）は、この入口の「User Input」の内容に置き換えて扱う。
- 共通スキル内の `$speckit-<name>` は、Claude Codeでは `/speckit-<name>` と読み替える。
- 共通スキル内のパス（`.specify/` など）はリポジトリルートを基準に解決する。
- 共通スキルを読めない場合は、読み込みの失敗を報告する。未読のまま手順に従ったと報告しない。

## User Input

```text
$ARGUMENTS
```
