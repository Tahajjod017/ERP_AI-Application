using GCTL.Data.Models;
using GCTL.Service.AccessPermissions;
using GCTL.Service;
using GCTL_NBR.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.ConfigureContext(builder.Configuration);
builder.Services.ConfigureDapperConnection(builder.Configuration);
builder.Services.ConfigureServices(builder.Configuration);
builder.Services.AddMemoryCache();
builder.Services.AddControllersWithViews();

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Register SignalR
builder.Services.AddSignalR();


// Add Cookie Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/Account/Login"; // Where to redirect for login
    options.LogoutPath = "/Account/Logout"; // Where to redirect for logout
    options.AccessDeniedPath = "/Home/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // Set cookie expiration time
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;  // Make sure cookies are secure
    options.Cookie.SameSite = SameSiteMode.Strict; // Prevent cross-site requests

});

//builder.Services.AddSingleton<IHubContext<MenuHub>, IHubContext<MenuHub>>();

builder.Services.AddScoped<PermissionService>();
builder.Services.AddScoped<IAccessControlService, AccessControlService>();
builder.Services.AddHttpContextAccessor();

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
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
