using GCTL.Data.Models;
using GCTL.Service;
using GCTL.Service.AccessPermissions;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.AdminSettings.GeneralSettings;
using GCTL.Service.RolePermissions;
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

// FIX: JWT key now comes ONLY from config/User Secrets — no hardcoded fallback
// Run in terminal: dotnet user-secrets set "Jwt:Key" 
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException(
        "JWT Key is not configured. Run: dotnet user-secrets set \"Jwt:Key\" \"your-secret\"");

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddJwtBearer("JwtBearer", options =>
{
    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.Zero // tokens expire exactly on time, no grace period
        // FIX: Removed IssuerValidator and AudienceValidator bypasses.
        // Those bypasses accepted ANY issuer/audience — meaning stolen tokens
        // from other apps would be accepted. Now we use the dynamic event below.
    };

    // Dynamic host-based issuer/audience — still flexible, but now properly validated
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var request = context.HttpContext.Request;
            var currentHost = $"{request.Scheme}://{request.Host}/";
            context.Options.TokenValidationParameters.ValidIssuer = currentHost;
            context.Options.TokenValidationParameters.ValidAudience = currentHost;
            return Task.CompletedTask;
        },
        // FIX: Log auth failures so you can see WHY tokens are rejected
        OnAuthenticationFailed = context =>
        {
            var logger = context.HttpContext.RequestServices
                .GetRequiredService<ILogger<Program>>();
            logger.LogWarning("JWT auth failed: {Error}", context.Exception.Message);
            return Task.CompletedTask;
        }
    };
})
.AddCookie("Cookies", options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Home/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.None       // HTTP allowed in dev
        : CookieSecurePolicy.Always;    // HTTPS required in production
    options.SessionStore = new MemoryCacheTicketStore();
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ApiPolicy", policy =>
    {
        policy.AddAuthenticationSchemes("JwtBearer");
        policy.RequireAuthenticatedUser();
    });

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

    c.DocInclusionPredicate((docName, apiDesc) =>
    {
        var actionDescriptor = apiDesc.ActionDescriptor as ControllerActionDescriptor;
        return actionDescriptor?.ControllerTypeInfo
            .GetCustomAttributes(typeof(ApiControllerAttribute), true).Any() ?? false;
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' followed by your JWT token.\nExample: Bearer eyJhbGci...",
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


builder.Services.AddControllers();
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.AddService<UserInfoActionFilter>();
});


// ====================================================================
// BUG 2 FIX — CORS:
// BEFORE: options.AddPolicy("Allowall", ...)  ← lowercase 'a'
//         app.UseCors("AllowAll");             ← uppercase 'A' — MISMATCH!
//         Result: CORS silently not applied. All cross-origin calls fail.
//
// AFTER:  Same name "AllowAll" used in BOTH places.
//         Also: AllowAnyOrigin() + AllowCredentials() is invalid combination
//         (browsers block it). Fixed below with environment-based origins.
// ====================================================================
#region CORS
builder.Services.AddCors(options =>
{
    // FIX: Policy name is now "AllowAll" — matches app.UseCors("AllowAll") below
    options.AddPolicy("AllowAll", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            // Development: allow Angular dev server + any localhost port
            policy
                .WithOrigins(
                    "http://localhost:4200",   // Angular dev server
                    "http://localhost:3000",   // React dev server
                    "http://localhost:5173",   // Vite dev server
                    "https://localhost:7001")  // Your own HTTPS dev
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();          // Needed for cookies + SignalR
        }
        else
        {
            // Production: lock down to your actual domain
            // TODO: Replace with your real production domain
            policy
                .WithOrigins("https://yourdomain.com", "https://www.yourdomain.com")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }
    });
});
#endregion


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
    var language = context.Request.Cookies["Language"] ?? "en";
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


// ====================================================================
// FIX: Middleware ORDER matters in ASP.NET Core.
// CORRECT order:
//   UseHttpsRedirection
//   UseStaticFiles
//   UseRouting          ← CORS must come AFTER this
//   UseCors             ← FIX: moved here, AFTER UseRouting
//   UseAuthentication
//   UseAuthorization
//   MapControllers
//
// BEFORE: app.UseCors("AllowAll") was called before UseRouting — wrong order,
//         AND wrong policy name. Both are now fixed.
// ====================================================================
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// FIX: UseCors is now AFTER UseRouting and uses the CORRECT policy name "AllowAll"
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseMiddleware<LocalizationMiddleware>();
app.UseAuthorization();

app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}"
);

app.Run();