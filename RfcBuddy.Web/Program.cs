
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using RfcBuddy.Web.Services;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IAppSettingsService, AppSettingsService>();

// Add services to the container.
builder.Services.AddControllersWithViews();

//Authentication
builder.Services.AddAuthentication(options =>
{
    //Sets cookie authentication scheme
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
    .AddCookie(cookie =>
    {
        cookie.AccessDeniedPath = "/";
        cookie.LogoutPath = "/";
        //Sets the cookie name and maxage, so the cookie is invalidated.
        cookie.Cookie.Name = "keycloak.cookie";
        cookie.Cookie.MaxAge = TimeSpan.FromMinutes(600);
        cookie.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        cookie.SlidingExpiration = true;
    })
    .AddOpenIdConnect(options =>
    {
        options.Authority = $"{builder.Configuration.GetSection("Keycloak")["auth-server-url"]}/realms/{builder.Configuration.GetSection("Keycloak")["realm"]}";
        options.ClientId = builder.Configuration.GetSection("Keycloak")["resource"];
        options.ClientSecret = builder.Configuration.GetSection("Keycloak").GetSection("credentials")["secret"];
        options.MetadataAddress = $"{builder.Configuration.GetSection("Keycloak")["auth-server-url"]}/realms/{builder.Configuration.GetSection("Keycloak")["realm"]}/.well-known/openid-configuration";
        options.RequireHttpsMetadata = true;
        options.GetClaimsFromUserInfoEndpoint = true;
        options.ResponseType = OpenIdConnectResponseType.Code;
        options.NonceCookie.SameSite = SameSiteMode.Unspecified;
        options.CorrelationCookie.SameSite = SameSiteMode.Unspecified;
        options.SaveTokens = true;
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            NameClaimType = "name",
            RoleClaimType = ClaimTypes.Role,
            ValidateIssuer = true,
        };
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
