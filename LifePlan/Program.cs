using LifePlan.Application.Interfaces;
using LifePlan.Application.Options;
using LifePlan.Application.Services;
using LifePlan.Extensions;
using LifePlan.Infrastructure.Options;
using LifePlan.Infrastructure.Repositories;
using LifePlan.Infrastructure.Services;
using Microsoft.AspNetCore.Rewrite;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(options => options.ConfigureLifePlanModelBindingMessages());
builder.Services.Configure<AffiliateLinksOptions>(builder.Configuration.GetSection(AffiliateLinksOptions.SectionName));
builder.Services.Configure<MicroCmsOptions>(builder.Configuration.GetSection(MicroCmsOptions.SectionName));
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection(SmtpSettings.SectionName));
builder.Services.AddScoped<IAffiliateLinkService, AffiliateLinkService>();
builder.Services.AddScoped<ILifePlanPageService, LifePlanPageService>();
builder.Services.AddScoped<IArticlePageService, ArticlePageService>();
builder.Services.AddScoped<IArticleHtmlSanitizer, ArticleHtmlSanitizer>();
builder.Services.AddScoped<IContactPageService, ContactPageService>();
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();
builder.Services.AddHttpClient<IArticleRepository, MicroCmsArticleRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// ルートドメインへのアクセスは www 付きの URL へ統一する。
app.UseRewriter(new RewriteOptions().AddRedirectToWwwPermanent("futari-kakei.com"));
app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
