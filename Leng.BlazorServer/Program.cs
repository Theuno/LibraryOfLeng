using Leng.Application.Services;
using Leng.Infrastructure;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

StaticWebAssetsLoader.UseStaticWebAssets(builder.Environment, builder.Configuration);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor()
                .AddMicrosoftIdentityConsentHandler();
builder.Services.AddMudServices();

// Add Database context
builder.Services.AddDbContextFactory<LengDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("sqlConnectionString")));

// Add Auth - Azure AD
//builder.Services.AddMicrosoftIdentityWebAppAuthentication(builder.Configuration, "AzureAd")
//    .EnableTokenAcquisitionToCallDownstreamApi()
//    .AddMicrosoftGraph(o =>
//    {
//        o.Scopes = "user.read profile";
//        o.BaseUrl = "https://graph.microsoft.com/v1.0";
//    })
//    .AddInMemoryTokenCaches();

// Add Auth - Azure AD B2C
var initialScopes = builder.Configuration.GetValue<string>("AzureAdB2C:Scopes")?.Split(' ');

builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAdB2C"))
        .EnableTokenAcquisitionToCallDownstreamApi(initialScopes)
        .AddMicrosoftGraph()
        .AddInMemoryTokenCaches();

builder.Services.AddAuthorization(options =>
{
    // By default, all incoming requests will be authorized according to the default policy
    options.FallbackPolicy = options.DefaultPolicy;
});

builder.Services.AddControllersWithViews()
    .AddMicrosoftIdentityUI();

builder.Services.AddTransient<IMTGDbService>(sp =>
{
    var contextFactory = sp.GetRequiredService<IDbContextFactory<LengDbContext>>();
    return new MTGDbService(contextFactory);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();