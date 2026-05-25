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
│  │  ├─ IArticleRepository.cs
│  │  └─ IArticleHtmlSanitizer.cs
│  ├─ Mappers/
│  │  └─ ArticlePageMapper.cs
│  ├─ ReferenceData/
│  │  └─ ArticleCategoryCatalog.cs
│  ├─ Results/
│  │  ├─ ArticleListResult.cs
│  │  └─ ArticleDetailResult.cs
│  └─ Services/
│     ├─ ArticlePageService.cs
│     └─ ArticleHtmlSanitizer.cs
├─ Infrastructure/
│  ├─ Options/
│  │  └─ MicroCmsOptions.cs
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
| `IArticleHtmlSanitizer` | 記事本文HTMLを表示前に安全化する契約 |
| `ArticleHtmlSanitizer` | 許可タグ・属性の allowlist に基づく記事本文HTMLのサニタイズ |
| `MicroCmsArticleRepository` | `HttpClient` による microCMS API 呼び出し |
| `MicroCmsOptions` | Infrastructure 側で microCMS 接続設定を受け取る |
| `MicroCmsArticleDto` | microCMS レスポンス構造 |
| `ArticleCategoryCatalog` | カテゴリ表示名とURL用slugの固定対応表 |
| `ArticlePageMapper` | DTO から ViewModel への変換 |
| `Article*ViewModel` | Razor 表示用データ |

記事一覧では、メインの記事カード一覧に加えてカテゴリフィルタ、ページネーション、右サイドバーを扱う。これらは View へ直接ロジックを書かず、`ArticleListViewModel` 配下の表示用 ViewModel に詰める。

## 4. DI と設定

`Program.cs` では以下を追加する方針とする。

- `builder.Services.Configure<MicroCmsOptions>(builder.Configuration.GetSection(MicroCmsOptions.SectionName));`
- `builder.Services.AddHttpClient<IArticleRepository, MicroCmsArticleRepository>();`
- `builder.Services.AddScoped<IArticlePageService, ArticlePageService>();`
- `builder.Services.AddScoped<IArticleHtmlSanitizer, ArticleHtmlSanitizer>();`

`MicroCmsOptions` には `ServiceDomain`, `ApiKey`, `ArticlesEndpoint` を持たせる。

`MicroCmsOptions` は外部APIクライアント実装が利用する設定のため、`Application/Options` ではなく `Infrastructure/Options` に配置する。

## 5. 取得処理

- 一覧は `GET /api/v1/articles` で取得する。
- 詳細は `GET /api/v1/articles?filters=slug[equals]{slug}&limit=1` で取得する。
- 初期実装では取得結果をキャッシュせず、リクエストごとに microCMS API から取得する。
- 一覧のページサイズは6件固定とする。
- 一覧取得時の `limit` は6、`offset` は `(page - 1) * 6`、`orders` は `-publishedAt` とする。
- 初期実装では下書きプレビューを扱わないため、microCMS API に `draftKey` は渡さない。
- `filters` は microCMS のクエリ文字列としてURLエンコードする。
- APIキーは `X-MICROCMS-API-KEY` ヘッダーに付与する。
- `HttpClient.BaseAddress` は `https://{ServiceDomain}.microcms.io/` とする。
- `thumbnail` は microCMS 側で必須項目とし、一覧・詳細・新着記事で画像表示に利用する。
- `category` は microCMS 側で必須項目とし、固定カテゴリのいずれかを利用する。
- 記事詳細URLは `/Articles/{slug}` とし、`slug` は microCMS の記事フィールドを利用する。
- カテゴリ指定がある場合は、`ArticleCategoryCatalog` で slug から日本語カテゴリ名へ変換し、`category[contains]{カテゴリ名}` を microCMS の `filters` に渡す。
- カテゴリslugが `ArticleCategoryCatalog` に存在しない場合は、microCMS API に渡さず `/Articles` へリダイレクトできる結果を返す。
- microCMS の `category` が配列で返る場合は、初期実装では先頭のカテゴリを主カテゴリとして ViewModel に変換する。
- 右サイドバーのカテゴリ件数は、カテゴリごとに `filters` と `limit=1` を指定して取得した `totalCount` から生成する。
- カテゴリ件数は現在表示中ページではなく、全公開記事を対象にする。
- `tags` は microCMS 側のカンマ区切り文字列を `,` で分割し、前後空白を trim して空要素を除外した配列として ViewModel に設定する。
- `CancellationToken` を Controller から Service、Repository へ渡せる形を優先する。
- API 呼び出し失敗時はログに残し、画面には安全なメッセージだけを返す。

## 6. 表示とHTML本文

記事詳細の `body` は HTML として表示する必要がある。

初期実装から HTML サニタイズを導入し、許可タグ・属性だけを残した本文 HTML を ViewModel に渡す。View はサニタイズ済み本文 HTML のみ `@Html.Raw(...)` で表示し、microCMS の `body` を直接表示しない。

サニタイズ処理は `IArticleHtmlSanitizer` として Application 層に契約を置き、実装では `Ganss.Xss.HtmlSanitizer` を利用する。許可するタグ・属性・URLスキームはアプリ用の狭い allowlist として明示し、ライブラリのデフォルト許可設定に依存しない。

初期 allowlist は以下を基本とする。

- 許可タグ: `p`, `h2`, `h3`, `h4`, `ul`, `ol`, `li`, `a`, `strong`, `em`, `blockquote`, `br`, `code`, `pre`, `img`, `figure`, `figcaption`
- 許可属性: `a[href]`, `a[title]`, `img[src]`, `img[alt]`, `img[width]`, `img[height]`
- 許可URLスキーム: `https` と相対URL
- 禁止: `script`, `iframe`, `style`, `form`, `input`, `button`, `svg`, イベント属性、`javascript:` URL、`data:` 画像

記事詳細ページのデザインは仮仕様とし、一覧ページと同じ右サイドバー用 ViewModel を再利用する。本文表示、記事ヘッダー、戻るリンク、見つからない表示は `ArticleDetailViewModel` に必要な表示用プロパティを持たせ、View で外部API DTOを直接参照しない。

仮UIでは、詳細ページ上部に「記事一覧へ戻る」リンク、カテゴリラベル、公開日、タイトル、リード文、タグ、16:9のサムネイル画像を表示し、その下にサニタイズ済み本文を表示する。本文下部CTAは初期実装では追加せず、右サイドバーCTAのみ表示する。詳細ページの右サイドバー新着記事は、現在表示中の記事を除外して最大3件表示する。

詳細取得で該当 `slug` の記事が見つからない場合は、Controller が `Response.StatusCode = StatusCodes.Status404NotFound` を設定し、記事が見つからない旨と記事一覧へ戻るリンクを含む詳細ViewModelを返す。microCMS 障害、タイムアウト、認証エラーなどのAPI取得失敗は記事なしとは区別し、Controller が `Response.StatusCode = StatusCodes.Status503ServiceUnavailable` を設定できる結果として扱う。microCMS のエラー詳細や内部例外情報は View に渡さず、ログにだけ残す。

SEO用の `ViewData["Description"]` と最小OGP metaは、Controller または ViewModel で表示用値を組み立てて View に渡す。記事詳細ページでは `title`、`metaDescription` / `description`、`thumbnail.url`、現在URLを使って記事ごとの OGP を出力する。記事一覧ページでは固定の一覧ページ用 OGP を出力する。canonical と構造化データは初期実装では扱わない。

## 7. 実装ステップ

1. `MicroCmsOptions` と設定キーを `Infrastructure` 側に追加する。
2. `slug` を含む microCMS レスポンス DTO を追加する。
3. `ArticleCategoryCatalog` を追加し、カテゴリ表示名とslugの固定対応表を定義する。
4. `IArticleRepository` と `MicroCmsArticleRepository` を追加し、GET取得を実装する。
5. `ArticlePageService` と Result を追加する。
6. `IArticleHtmlSanitizer` とサニタイズ実装を追加する。
7. DTO から ViewModel への Mapper を追加し、詳細本文にはサニタイズ済みHTMLを設定する。
8. `ArticlesController` を追加する。
9. `Views/Articles/Index.cshtml` と `Details.cshtml` を追加する。
10. 一覧ページのカテゴリフィルタ、記事カード、ページネーション、右サイドバーを実装する。
11. `_Layout.cshtml` の共通ヘッダーナビゲーションに `/Articles` への「記事」リンクを追加する。
12. 必要に応じて記事画面用 CSS を追加する。
13. Service / Mapper / Repository / Sanitizer のテスト方針を確定し、可能な範囲で単体テストを追加する。
14. `dotnet build LifePlan.sln -m:1` を実行する。
15. 計算ロジックには触れないため、通常は `dotnet test LifePlan.sln -m:1` は任意。ただし Mapper や Service テストを追加した場合は実行する。

## 8. テスト観点

- 一覧レスポンスを ViewModel に変換できる。
- 詳細レスポンスを ViewModel に変換できる。
- 詳細URL生成に microCMS の `slug` が利用される。
- 一覧取得時に `orders=-publishedAt` が生成される。
- microCMS の `filters` がURLエンコードされる。
- 不正なカテゴリslugが microCMS API に渡らず、`/Articles` へのリダイレクト結果になる。
- 詳細取得時に `slug` の一致条件と `limit=1` が生成される。
- 詳細レスポンスの `metaDescription` が空の場合に `description` を代替利用できる。
- 記事詳細ページの最小OGP metaに記事タイトル、説明文、サムネイル、URLを反映できる。
- 記事一覧ページの固定OGP metaを出力できる。
- 詳細本文HTMLから `script` タグやイベント属性などの実行可能なHTMLが除去される。
- 詳細本文HTMLから `javascript:` URL、`data:` 画像、`iframe`、`style`、`form`、`input`、`button`、`svg` が除去される。
- 詳細本文HTMLの許可済み `img` は `src`、`alt`、`width`、`height` を保持できる。
- 詳細ViewModelにはサニタイズ済み本文HTMLだけが設定される。
- 詳細取得で記事が見つからない場合に、HTTP 404 とエラー詳細を露出しない表示用結果を返せる。
- microCMS API 取得失敗時に、HTTP 503 相当とエラー詳細を露出しない表示用結果を返せる。
- `tags` のカンマ区切りが表示用のタグ配列へ変換される。
- カテゴリslugが日本語カテゴリ名へ変換され、microCMS の絞り込み条件に使われる。
- microCMS の `category` が配列の場合に、先頭のカテゴリを主カテゴリとして表示できる。
- 右サイドバーのカテゴリ件数が、現在ページ内件数ではなく全公開記事の `totalCount` から生成される。
- `page` から `limit=6` と `offset=(page - 1) * 6` が生成される。
- 範囲外ページ指定時に1ページ目へ戻せる。
- サムネイル画像を一覧・詳細・新着記事に表示できる。
- 詳細ページでタイトル、リード文、カテゴリ、公開日、タグ、サムネイル、本文、右サイドバーを表示できる。
- 詳細ページの新着記事から現在表示中の記事を除外できる。
- microCMS 取得失敗時に、Controller がエラー詳細を露出しない ViewModel を返す。
- APIキー未設定時に起動時または取得時に分かりやすく失敗する。
- 共通ヘッダーに `/Articles` への「記事」リンクが表示される。
- 記事一覧・記事詳細表示時に、必要に応じてヘッダーの現在位置表示が破綻しない。

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
