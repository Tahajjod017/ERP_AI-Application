using GCTL.Core.ViewModels.MasterSetup.ServiceType;
using GCTL.Data.Models;
using GCTL.Service;
using GCTL.Service.AccessPermissions;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.AdminSettings.GeneralSettings;
using GCTL.Service.RolePermissions;
using GCTL.Service.VisitingPath;
using GCTL_App.EmailServicesMethod;
using GCTL_App.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using QuestPDF.Infrastructure;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

#region Manual
builder.Services.ConfigureContext(builder.Configuration);
builder.Services.ConfigureDapperConnection(builder.Configuration);
builder.Services.ConfigureServices(builder.Configuration);
builder.Services.AddMemoryCache();
builder.Services.AddSignalR();
#endregion


#region Authentication & Authorization

var jwtKey = builder.Configuration["Jwt:Key"] ?? "your-secret-key-123456";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "https://localhost:7086/";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "https://localhost:7086/";

builder.Services.AddAuthentication(options =>
{
    // Default = Cookies (for MVC). APIs explicitly use JWT.
    options.DefaultScheme = Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme;
})
// JWT for APIs
.AddJwtBearer("JwtBearer", options =>
{
    options.RequireHttpsMetadata = true; // Enable in production
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
})
// Cookies for MVC
.AddCookie("Cookies", options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Home/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Always secure in production
    options.SessionStore = new MemoryCacheTicketStore();
});

builder.Services.AddAuthorization(options =>
{
    // APIs => JWT
    options.AddPolicy("ApiPolicy", policy =>
    {
        policy.AddAuthenticationSchemes("JwtBearer");
        policy.RequireAuthenticatedUser();
    });

    // MVC => Cookies
    options.AddPolicy("WebPolicy", policy =>
    {
        policy.AddAuthenticationSchemes("Cookies");
        policy.RequireAuthenticatedUser();
    });
});

#endregion


#region Identity
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.User.RequireUniqueEmail = true;
    options.Lockout.AllowedForNewUsers = true;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
    options.Lockout.MaxFailedAccessAttempts = 3;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();
#endregion


#region Services
builder.Services.AddScoped<PermissionService>();
builder.Services.AddScoped<IAccessControlService, AccessControlService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<UserInfoActionFilter>();
#endregion


#region Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "GCTL API",
        Version = "v1",
        Description = "API documentation for GCTL mobile integration"
    });

    // Filter only [ApiController] controllers
    c.DocInclusionPredicate((docName, apiDesc) =>
    {
        var actionDescriptor = apiDesc.ActionDescriptor as ControllerActionDescriptor;
        return actionDescriptor?.ControllerTypeInfo.GetCustomAttributes(typeof(ApiControllerAttribute), true).Any() ?? false;
    });

    // JWT Bearer token support
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' followed by your JWT token.\nExample: Bearer eyJhbGciOiJIUzI1NiIsInR...",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
#endregion


builder.Services.AddControllers(); // For API controllers
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.AddService<UserInfoActionFilter>(); // Global filter registration
});

// localization
builder.Services.AddScoped<ILocalizationContext, LocalizationContext>();
QuestPDF.Settings.License = LicenseType.Community;

var app = builder.Build();


#region Error Handling & Swagger
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "GCTL API V1");
        c.RoutePrefix = "swagger";
    });
}
#endregion


#region Language Middleware
app.Use(async (context, next) =>
{
    var language = context.Request.Cookies["Language"] ?? "en"; // Default to English
    context.Items["Language"] = language;

    if (context.Request.Cookies["Language"] == null)
    {
        context.Response.Cookies.Append("Language", "en", new CookieOptions
        {
            HttpOnly = true,
            Secure = !app.Environment.IsDevelopment(),
            SameSite = SameSiteMode.Lax
        });
    }
    await next();
});
#endregion


app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseMiddleware<LocalizationMiddleware>();
app.UseAuthorization();

app.MapControllers(); // Maps attribute-routed API controllers
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}"
);

app.Run();
