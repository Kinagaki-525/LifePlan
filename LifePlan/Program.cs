using LifePlan.Application.Interfaces;
using LifePlan.Application.Options;
using LifePlan.Application.Services;
using LifePlan.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(options => options.ConfigureLifePlanModelBindingMessages());
builder.Services.Configure<AffiliateLinksOptions>(builder.Configuration.GetSection(AffiliateLinksOptions.SectionName));
builder.Services.AddScoped<IAffiliateLinkService, AffiliateLinkService>();
builder.Services.AddScoped<ILifePlanPageService, LifePlanPageService>();

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
