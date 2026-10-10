# LifePlan

@AGENTS.md

作業ルールと責務分離は `.github/copilot-introduction.md` を読む。

変更レビューには `/review-changes` を使う。独立したレビュー担当が必要な場合は `change-reviewer` に依頼する。

レビュー担当へは対象、比較元、差分、未追跡ファイル一覧、検証結果を渡す。読み取り専用の担当に代わり、呼び出し元がGit情報の収集と必要なビルド・テストを実行する。
