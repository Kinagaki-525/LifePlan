# 記事機能ドキュメント

このフォルダは、microCMS を利用した記事表示機能に関する仕様・実装方針・未決事項を管理する。

## ドキュメント

| ファイル | 内容 |
| --- | --- |
| `feature-spec.md` | 記事表示機能の仕様 |
| `implementation-plan.md` | ASP.NET Core MVC での実装方針 |
| `content-authoring-guide.md` | microCMS での記事作成・入力ルール |
| `open-issues.md` | 実装前に確認が必要な事項 |

## 実装の基本方針

- microCMS からの記事取得はサーバー側の `HttpClient` で行う。
- API キーはコードやクライアント JavaScript に置かず、Azure App Service の環境変数またはローカルの User Secrets で管理する。
- Controller は Application Service の interface に依存し、外部 API 呼び出しの詳細を持たない。
- microCMS のレスポンス DTO と画面表示用 ViewModel は分ける。
- 記事本文 HTML はサーバー側でサニタイズし、許可済み HTML だけを ViewModel 経由で表示する。
