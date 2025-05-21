using GCTL.Data.Models;
using GCTL.Service.AccessPermissions;
using GCTL.Service;
using GCTL_App.Extensions;
using Microsoft.AspNetCore.Identity;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.VisitingPath;

var builder = WebApplication.CreateBuilder(args);

//Globally retrieve LIP, LMAC, and CreatedBy, UpdatedBy, DeletedBy fields. [Added by Siam]
builder.Services.AddHttpContextAccessor(); // 1?? Required for accessing HttpContext
builder.Services.AddScoped<UserInfoActionFilter>();
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.AddService<UserInfoActionFilter>(); // 2?? Global filter registration
});
//end
// Add services to the container.
#region Manual
builder.Services.ConfigureContext(builder.Configuration);
builder.Services.ConfigureDapperConnection(builder.Configuration);
builder.Services.ConfigureServices(builder.Configuration);
builder.Services.AddMemoryCache();
#endregion

builder.Services.AddControllersWithViews();

#region Language

builder.Services.AddHttpContextAccessor();

#endregion


// Register SignalR
builder.Services.AddSignalR();




#region Manual
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

builder.Services.AddScoped<PermissionService>();
builder.Services.AddScoped<IAccessControlService, AccessControlService>();
builder.Services.AddHttpContextAccessor();
#endregion

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


#region lang

app.Use(async (context, next) =>
{
    var language = context.Request.Cookies["Language"] ?? "en"; // Default to English
    context.Items["Language"] = language;

    if (context.Request.Cookies["Language"] == null)
    {
        context.Response.Cookies.Append("Language", "en", new CookieOptions
        {
            HttpOnly = true,
            Secure = !app.Environment.IsDevelopment(), // Secure in production
            SameSite = SameSiteMode.Lax
        });
    }

    await next();
});

#endregion



app.UseRouting();
app.UseMiddleware<UserVisitLoggingMiddleware>();  // added by Siam
app.UseAuthorization();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
