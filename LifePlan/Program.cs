using LifePlan.Application.Interfaces;
using LifePlan.Application.Options;
using LifePlan.Application.Services;
using LifePlan.Extensions;
<<<<<<< HEAD
using LifePlan.Infrastructure.Options;
using LifePlan.Infrastructure.Repositories;
=======
>>>>>>> origin/master
using LifePlan.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(options => options.ConfigureLifePlanModelBindingMessages());
builder.Services.Configure<AffiliateLinksOptions>(builder.Configuration.GetSection(AffiliateLinksOptions.SectionName));
<<<<<<< HEAD
builder.Services.Configure<MicroCmsOptions>(builder.Configuration.GetSection(MicroCmsOptions.SectionName));
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection(SmtpSettings.SectionName));
builder.Services.AddScoped<IAffiliateLinkService, AffiliateLinkService>();
builder.Services.AddScoped<ILifePlanPageService, LifePlanPageService>();
builder.Services.AddScoped<IArticlePageService, ArticlePageService>();
builder.Services.AddScoped<IArticleHtmlSanitizer, ArticleHtmlSanitizer>();
builder.Services.AddScoped<IContactPageService, ContactPageService>();
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();
builder.Services.AddHttpClient<IArticleRepository, MicroCmsArticleRepository>();
=======
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection(SmtpSettings.SectionName));
builder.Services.AddScoped<IAffiliateLinkService, AffiliateLinkService>();
builder.Services.AddScoped<ILifePlanPageService, LifePlanPageService>();
builder.Services.AddScoped<IContactPageService, ContactPageService>();
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();
>>>>>>> origin/master

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
