---
name: speckit-clarify
description: "Identify underspecified areas in the current feature spec by asking up to 5 highly targeted clarification questions and encoding answers back into the spec."
argument-hint: "Optional areas to clarify in the spec"
---

# Spec Kit：speckit-clarify の入口

最初に [共通スキル](../../../.agents/skills/speckit-clarify/SKILL.md) を読み、その手順に従う。手順の正本は共通スキルにあり、Spec KitのCodex連携が管理する。

- 共通スキルの「User Input」欄（引数のプレースホルダー）は、この入口の「User Input」の内容に置き換えて扱う。
- 共通スキル内の `$speckit-<name>` は、Claude Codeでは `/speckit-<name>` と読み替える。
- 共通スキル内のパス（`.specify/` など）はリポジトリルートを基準に解決する。
- 共通スキルを読めない場合は、読み込みの失敗を報告する。未読のまま手順に従ったと報告しない。

## User Input

```text
$ARGUMENTS
```
