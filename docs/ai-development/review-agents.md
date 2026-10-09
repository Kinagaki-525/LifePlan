# 変更レビューの使い方と動作確認

レビュー手順の正本は `.agents/skills/review-changes/SKILL.md`。一般的な観点とLifePlan固有の参照先を分け、Claudeのスキルと両ツールのエージェントから同じ正本を読む。

## 呼び出し方

- Codex：`$review-changes 未コミット変更をレビューしてください。`
- Claude Code：`/review-changes 未コミット変更をレビューしてください。`
- 独立した担当：`change-reviewer に、指定した差分のレビューを依頼してください。`

ブランチレビューでは比較元も指定する。例：`develop との merge-base から現在のHEADまでをレビューしてください。未コミット変更は含めません。`

独立した担当へ渡す情報は、対象・比較元・差分・変更ファイル・未追跡ファイル・検証結果。Claudeの担当は `Read, Grep, Glob` のみを使うため、呼び出し元が `git status --short`、`git diff --cached`、`git diff`、`git ls-files --others --exclude-standard` などを収集する。未追跡ファイルは差分に出ないので、対象ファイルとして渡す。

ビルド・テストも呼び出し元が実行し、コマンド、対象の版・状態、終了コード、結果、警告を渡す。ビルド・テストは生成物を作成するため、読み取り専用の担当では実行しない。主会話でスキルを使う場合は、その会話の権限内で実行できる。

```powershell
dotnet build LifePlan.slnx -m:1
dotnet test LifePlan.slnx -m:1
```

## 読み込み確認

Codexでスキル一覧に `review-changes` が現れることを確認する。Codexのカスタムエージェントは `.codex/agents/change-reviewer.toml`。この環境のCodex CLI 0.160.0では設定ファイルによるロール登録を確認できたため、`.codex/config.toml` にも登録する。プロジェクト設定は信頼済みのプロジェクトで読み込まれる。Claude Codeでは `/review-changes` と `.claude/agents/change-reviewer.md` の読み込みを確認する。変更が反映されなければセッションを再起動する。

Codexのロール登録は [OpenAI Docsの設定リファレンス](https://learn.chatgpt.com/docs/config-file/config-reference) を参照する。モデル、承認モード、並列数はここで固定せず、既存環境から継承する。

Claude用の `CLAUDE.md` は `AGENTS.md` をインポートする。スキルとエージェントから、共通スキル、チェックリスト、LifePlanの参照先を実際に読めることを確認する。読み込みできない場合はその失敗を報告し、成功扱いにしない。

## レビュー動作の確認入力

次のA〜Cは動作確認用の提供差分であり、リポジトリに存在するコードへの変更ではない。各ケースを別々にレビューさせる。サンプルを本体やテストプロジェクトへ追加しない。

ビルド・テスト結果を渡さずに依頼し、報告に未実行と記載されることを確認する。模擬ログを渡す場合は模擬と明記し、実際の実行結果と区別する。

### A：範囲判定

仕様：利率は0〜20%で、両端を含む。関連テストは0%と10%が有効になることのみを確認している。

```diff
diff --git a/RateRules.cs b/RateRules.cs
--- a/RateRules.cs
+++ b/RateRules.cs
@@ -1,4 +1,4 @@
 public static class RateRules
 {
-    public static bool IsValid(decimal rate) => rate is >= 0m and <= 20m;
+    public static bool IsValid(decimal rate) => rate is >= 0m and < 20m;
 }
```

### B：外部API

仕様：APIの非成功応答は例外として呼び出し元へ返す。画面サービスはその例外を捕捉して「読み込めません」を表示する。成功した空一覧は「記事なし」を表示する。関連テストはHTTP 200の空一覧のみを確認している。

```diff
diff --git a/ArticleRepository.cs b/ArticleRepository.cs
--- a/ArticleRepository.cs
+++ b/ArticleRepository.cs
@@ -1,6 +1,10 @@
 public async Task<Article[]> GetArticlesAsync(CancellationToken token)
 {
     using var response = await httpClient.GetAsync("articles", token);
-    response.EnsureSuccessStatusCode();
+    if (!response.IsSuccessStatusCode)
+    {
+        return [];
+    }
+
     return await response.Content.ReadFromJsonAsync<Article[]>(token) ?? [];
 }
```

### C：空行のみ

仕様：円単位の金額を万円単位に変換する。既存テストでは0円、10000円、12345円の出力をそれぞれ0、1、1.2345として確認している。

```diff
diff --git a/AmountMapper.cs b/AmountMapper.cs
--- a/AmountMapper.cs
+++ b/AmountMapper.cs
@@ -1,5 +1,6 @@
 public static decimal ToManYen(decimal amountYen)
 {
     var result = amountYen / 10000m;
+
     return result;
 }
```

## 評価方法

形式の検証に加えて、実際のレビュー結果を確認する。具体的な発生条件と影響を説明できること、不足するテストに入力と期待値があること、問題のない差分に無理な指摘を作らないこと、未実行を明示すること、提供差分を実在コードと混同しないことを確認する。

主会話のスキルと独立した担当を同じ入力で確認し、指摘の言い回しや件数の完全一致は要求しない。レビュー前後で対象ソース・設定の内容が変わっていないことも確認する。

## Codex側の確認記録（2026-10-10）

- 共通スキルとClaude用スキルの形式、YAML/TOML、相対リンク、参照文書、UTF-8/LFを確認。
- Codex CLI 0.160.0のApp Serverで、信頼済みプロジェクトの `skills/list` と `config/read` を実行し、スキルの有効化とロール登録を確認。検証用の `CODEX_HOME` を使い、利用者のグローバル設定は変更していない。
- エージェント設定レイヤーを厳格な設定読み込みで確認し、レビュー指示と `read-only` が有効になることを確認。
- 独立したCodexエージェントへレビュー指示とA〜Cの入力を渡し、A・Bの問題と不足するテスト、Cの指摘なし、全ケースのテスト未実行表記を確認。名前付きロールの起動を経由する確認とは区別する。
- さらにCodex CLIの読み取り専用セッションから `agent_type: change-reviewer` の担当1名を実際に起動し、完了を確認。A・Bの問題と追加テスト案、Cの指摘なし、全ケースのテスト未実行表記を確認。レビュー前後でスキル・設定の内容が変わっていないことも確認。
- `dotnet build LifePlan.slnx -m:1`：成功、警告0、エラー0。
- `dotnet test LifePlan.slnx -m:1`：52件成功、失敗0、スキップ0。これは既存のLifePlanテストであり、提供差分A〜Cを実行した結果ではない。
- Claude CLIはこの環境にないため、Claudeの実機での読み込み・起動・レビューは未確認。次のプロンプトで確認する。

## Claude側の確認記録（2026-10-10）

- Claude Code（VS Code拡張）で、`CLAUDE.md` からの `AGENTS.md` インポート、`/review-changes` の入口から共通スキル・`references/` の読み込みを確認。
- 定義ファイルを追加する前に開始したセッションでは、`change-reviewer` が未登録（`Agent type 'change-reviewer' not found`）となった。定義の再読み込み後に起動できた。定義ファイルの形式（UTF-8、BOMなし、LF、frontmatter）に問題はなかった。
- 主会話の `/review-changes` と `change-reviewer` のそれぞれでA〜Cを別々にレビューし、どちらの経路でもA・Bの問題（重要度：高）と入力・期待値付きの追加テスト案、Cの指摘なし、全ケースのテスト未実行表記、提供差分と実在コードの区別を確認。`change-reviewer` は共通スキル・`references/`・`.github/copilot-introduction.md` を読めたと報告し、読み込み失敗はなかった。
- レビュー前後で、ソース・テスト・設定・文書・スキル・エージェントのSHA-256と `git status` が一致することを確認。
- `dotnet build LifePlan.slnx -m:1`：成功、警告0、エラー0。
- `dotnet test LifePlan.slnx -m:1`：52件成功、失敗0、スキップ0。既存のLifePlanテストの結果であり、提供差分A〜Cの実行結果ではない。
- 別セッションで、開始時から `change-reviewer` が登録されていること、`skills:` により入口スキルの本文が最初のツール呼び出しより前に読み込まれることを確認（担当の自己申告とトランスクリプト）。主会話の `/review-changes` と `change-reviewer` でA〜Cを確認し、A・Bの問題と追加テスト案、Cの指摘なし、全ケースのテスト未実行表記を確認。主会話は期待結果を知った状態での確認である。
- 同セッションで、エージェント定義から入口スキル・共通スキル・`AGENTS.md` と重複する指示を削り、`references/lifeplan.md` のビルド・テスト節を `AGENTS.md` と `.github/copilot-introduction.md` への参照に置き換えた。変更後の定義が同じセッション内で反映されること、A〜Cの結果が変わらないことを確認。担当1名あたりのトークン量は約20kから約12〜13kに減った。Codex側の変更後の動作は未確認。

## Claudeへ渡す確認プロンプト

```text
このリポジトリに追加されたレビュースキルとエージェントの動作確認を行ってください。ソース・テスト・設定・仕様書は修正しないでください。

1. CLAUDE.md、AGENTS.md、.github/copilot-introduction.md、docs/ai-development/review-agents.md を読み、/review-changes と change-reviewer が利用できるか確認してください。共通スキルと参照先を実際に読み込めることも確認してください。
2. 主会話で /review-changes を使い、review-agents.md の「レビュー動作の確認入力」A〜Cを別々にレビューしてください。これらは提供差分として扱い、実在のソースへ反映しないでください。各ケースにはビルド・テスト結果を渡さないでください。
3. change-reviewer を起動し、同じA〜Cの仕様・差分・既存テストの説明を渡してレビューを依頼してください。先ほどのレビュー結果や期待する指摘は渡さないでください。Git情報やコマンド実行が必要なら主会話で収集してください。
4. 各経路について、入力条件と影響を説明できるか、不足するテストの入力・期待値が具体的か、問題のない変更に無理な指摘をしないか、テスト未実行と提供差分の範囲を明示するかを評価してください。読み込みや起動の失敗は、そのまま失敗として報告してください。
5. 主会話で dotnet build LifePlan.slnx -m:1 と dotnet test LifePlan.slnx -m:1 を実行し、終了コード・件数・警告を報告してください。この結果は現在のリポジトリの検証結果であり、A〜Cを実行した結果ではないことを区別してください。
6. レビュー前後でソース・設定に変更がないことを確認し、読み込み確認、各ケースの結果、ビルド・テスト結果、未確認事項を日本語で報告してください。
```
