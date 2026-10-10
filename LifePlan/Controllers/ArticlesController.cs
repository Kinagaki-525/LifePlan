using LifePlan.Application.Factories;
using LifePlan.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LifePlan.Controllers
{
    public class ArticlesController : Controller
    {
        private const string CanonicalBaseUrl = "https://www.futari-kakei.com";

        private readonly IArticlePageService articlePageService;

        public ArticlesController(IArticlePageService articlePageService)
        {
            this.articlePageService = articlePageService;
        }

        public async Task<IActionResult> Index(string? category, int? page)
        {
            var result = await articlePageService.CreateListPage(category, page, HttpContext.RequestAborted);

            if (result.ShouldRedirect)
            {
                return Redirect(result.RedirectUrl!);
            }

            if (result.StatusCode.HasValue)
            {
                Response.StatusCode = result.StatusCode.Value;
            }

            ViewData["Title"] = "お役立ち記事";
            ViewData["Description"] = "結婚・家計・子育てのお金にまつわる情報をわかりやすく解説";
            ViewData["OgTitle"] = ViewData["Title"];
            ViewData["OgDescription"] = ViewData["Description"];
            ViewData["OgType"] = "website";
            ViewData["OgUrl"] = CreateCurrentUrl();

            // カテゴリ別・2ページ目以降の canonical は扱わず、カテゴリ未指定の1ページ目のみ設定する。
            if (!result.StatusCode.HasValue && string.IsNullOrWhiteSpace(category) && page.GetValueOrDefault(1) <= 1)
            {
                ViewData["CanonicalUrl"] = CanonicalBaseUrl + ArticleUrlFactory.CreateListUrl();
            }

            return View(result.Page);
        }

        [HttpGet("/Articles/{slug}")]
        public async Task<IActionResult> Details(string slug)
        {
            var result = await articlePageService.CreateDetailPage(slug, HttpContext.RequestAborted);

            if (result.StatusCode.HasValue)
            {
                Response.StatusCode = result.StatusCode.Value;
            }

            ViewData["Title"] = result.Page.Title;
            ViewData["Description"] = result.Page.MetaDescription;

            if (!result.Page.HasError)
            {
                ViewData["OgTitle"] = result.Page.Title;
                ViewData["OgDescription"] = result.Page.MetaDescription;
                ViewData["OgImage"] = result.Page.ThumbnailUrl;
                ViewData["OgType"] = "article";
                ViewData["OgUrl"] = CreateCurrentUrl();
                ViewData["CanonicalUrl"] = CanonicalBaseUrl + ArticleUrlFactory.CreateDetailUrl(slug);
            }

            return View(result.Page);
        }

        private string CreateCurrentUrl()
        {
            return $"{Request.Scheme}://{Request.Host}{Request.PathBase}{Request.Path}{Request.QueryString}";
        }
    }
}
