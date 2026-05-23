# 記事表示機能 実装方針

## 1. 方針

microCMS 連携は外部 API 取得として扱い、Controller や View に API 呼び出しの詳細を持たせない。

既存の責務分離に合わせ、Controller は `IArticlePageService` を呼び出し、Application Service が記事画面の処理フローを担当する。microCMS への HTTP 通信は `Application/Interfaces` の抽象を通して `Infrastructure` 側に実装する。

## 2. 追加予定の構成

```text
LifePlan/
├─ Controllers/
│  └─ ArticlesController.cs
├─ Views/
│  └─ Articles/
│     ├─ Index.cshtml
│     └─ Details.cshtml
├─ ViewModels/
│  └─ Articles/
│     ├─ ArticleListItemViewModel.cs
│     ├─ ArticleListViewModel.cs
│     ├─ ArticleSidebarViewModel.cs
│     ├─ ArticleCategoryViewModel.cs
│     ├─ ArticlePaginationViewModel.cs
│     └─ ArticleDetailViewModel.cs
├─ Application/
│  ├─ Dto/
│  │  └─ MicroCms/
│  │     ├─ MicroCmsArticleDto.cs
│  │     ├─ MicroCmsArticleListResponseDto.cs
│  │     └─ MicroCmsImageDto.cs
│  ├─ Interfaces/
│  │  ├─ IArticlePageService.cs
│  │  └─ IArticleRepository.cs
│  ├─ Mappers/
│  │  └─ ArticlePageMapper.cs
│  ├─ Options/
│  │  └─ MicroCmsOptions.cs
│  ├─ Results/
│  │  ├─ ArticleListResult.cs
│  │  └─ ArticleDetailResult.cs
│  └─ Services/
│     └─ ArticlePageService.cs
├─ Infrastructure/
│  └─ Repositories/
│     └─ MicroCmsArticleRepository.cs
└─ wwwroot/
   └─ css/
      └─ articles.css
```

`articles.css` は記事画面固有の見た目が増える場合のみ追加する。軽微な調整で足りる場合は既存 CSS 方針に合わせる。

## 3. 責務分担

| 場所 | 責務 |
| --- | --- |
| `ArticlesController` | 一覧・詳細のHTTPリクエスト受付、View返却 |
| `IArticlePageService` / `ArticlePageService` | 一覧・詳細画面の処理フロー、取得失敗時の画面用結果生成 |
| `IArticleRepository` | アプリケーションから見た記事取得契約 |
| `MicroCmsArticleRepository` | `HttpClient` による microCMS API 呼び出し |
| `MicroCmsOptions` | microCMS の設定値受け取り |
| `MicroCmsArticleDto` | microCMS レスポンス構造 |
| `ArticlePageMapper` | DTO から ViewModel への変換 |
| `Article*ViewModel` | Razor 表示用データ |

記事一覧では、メインの記事カード一覧に加えてカテゴリフィルタ、ページネーション、右サイドバーを扱う。これらは View へ直接ロジックを書かず、`ArticleListViewModel` 配下の表示用 ViewModel に詰める。

## 4. DI と設定

`Program.cs` では以下を追加する方針とする。

- `builder.Services.Configure<MicroCmsOptions>(builder.Configuration.GetSection(MicroCmsOptions.SectionName));`
- `builder.Services.AddHttpClient<IArticleRepository, MicroCmsArticleRepository>();`
- `builder.Services.AddScoped<IArticlePageService, ArticlePageService>();`

`MicroCmsOptions` には `ServiceDomain`, `ApiKey`, `ArticlesEndpoint` を持たせる。

## 5. 取得処理

- 一覧は `GET /api/v1/articles` で取得する。
- 詳細は `GET /api/v1/articles/{id}` で取得する。
- APIキーは `X-MICROCMS-API-KEY` ヘッダーに付与する。
- `HttpClient.BaseAddress` は `https://{ServiceDomain}.microcms.io/` とする。
- `CancellationToken` を Controller から Service、Repository へ渡せる形を優先する。
- API 呼び出し失敗時はログに残し、画面には安全なメッセージだけを返す。

## 6. 表示とHTML本文

記事詳細の `body` は HTML として表示する必要がある。

初期実装では次のどちらかを選ぶ。

| 方針 | 内容 |
| --- | --- |
| 信頼前提 | microCMS 編集者を信頼し、`@Html.Raw(Model.Body)` で表示する |
| サニタイズ導入 | HTML サニタイザーを通して許可タグ・属性だけ表示する |

依存関係を増やさない方針を優先するなら、初期実装は信頼前提とし、編集権限を限定する。ただし、外部ライターや複数編集者が本文HTMLを触る運用ならサニタイズ導入を検討する。

## 7. 実装ステップ

1. `MicroCmsOptions` と設定キーを追加する。
2. microCMS レスポンス DTO を追加する。
3. `IArticleRepository` と `MicroCmsArticleRepository` を追加し、GET取得を実装する。
4. `ArticlePageService` と Result を追加する。
5. DTO から ViewModel への Mapper を追加する。
6. `ArticlesController` を追加する。
7. `Views/Articles/Index.cshtml` と `Details.cshtml` を追加する。
8. 一覧ページのカテゴリフィルタ、記事カード、ページネーション、右サイドバーを実装する。
9. `_Layout.cshtml` の記事導線を有効化する。
10. 必要に応じて記事画面用 CSS を追加する。
11. Service / Mapper / Repository のテスト方針を確定し、可能な範囲で単体テストを追加する。
12. `dotnet build LifePlan.sln -m:1` を実行する。
13. 計算ロジックには触れないため、通常は `dotnet test LifePlan.sln -m:1` は任意。ただし Mapper や Service テストを追加した場合は実行する。

## 8. テスト観点

- 一覧レスポンスを ViewModel に変換できる。
- 詳細レスポンスを ViewModel に変換できる。
- `tags` のカンマ区切りが表示用のタグ配列へ変換される。
- サムネイルが null の場合でも画面表示が壊れない。
- microCMS 取得失敗時に、Controller がエラー詳細を露出しない ViewModel を返す。
- APIキー未設定時に起動時または取得時に分かりやすく失敗する。

## 9. 初期実装ではやらないこと

- 管理画面の作成
- 記事の作成・更新・削除 API
- 下書きプレビュー
- カテゴリ別ページ
- タグ別ページ
- 全文検索
- RSS / sitemap 生成
- キャッシュ導入

キャッシュはアクセス数やレスポンス速度の課題が見えた時点で、`IMemoryCache` または CDN を含めて検討する。
