---
name: review-changes
description: Review code changes and related tests for correctness, regressions, and missing verification using the shared repository review workflow. Use for requested reviews of uncommitted changes, commits, branches, or supplied diffs.
argument-hint: "[対象：コミット・ブランチ・ファイルなど。省略時は未コミット変更すべて]"
---

# 変更レビューの入口

最初に [共通レビュースキル](../../../.agents/skills/review-changes/SKILL.md) を読み、その手順に従う。レビュー観点と出力形式の正本は共通スキルにある。

共通スキル内の相対リンクは `.agents/skills/review-changes/` を基準に解決する。この入口のディレクトリを基準にしない。

共通スキルを読めない場合は、読み込みの失敗を報告する。未読のまま共通手順に従ったと報告しない。

下の「User Input」は、共通スキルでいうユーザーが指定した対象として扱う。空の場合、または引数に置き換わっていない場合（エージェントへの事前読み込みなど）は、依頼文で指定された対象に従い、指定がなければ共通スキルの既定の対象に従う。

## User Input

```text
$ARGUMENTS
```
