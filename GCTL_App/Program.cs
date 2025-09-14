//using GCTL.Core.SeedData;
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


#region JWT
var jwtKey = builder.Configuration["Jwt:Key"] ?? "your-secret-key-123456"; // Store in appsettings.json
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "your-app";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});
#endregion


#region identity
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.User.RequireUniqueEmail = true;
    //options.Password.RequireDigit = true;
    //options.Password.RequiredLength = 6;
    //options.Password.RequireLowercase = true;
    //options.Password.RequireNonAlphanumeric = false;
    //options.Password.RequireUppercase = true;
    options.Lockout.AllowedForNewUsers = true;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 3;
    // options.Stores.SchemaVersion = "dbo"; // Set the schema version to dbo
    //options.Stores.MaxLengthForKeys = 128; // Set the maximum length for keys
    // options.Stores.ProtectPersonalData = true; // Protect personal data
}).AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();

// Add Cookie Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.LoginPath = "/Account/Login"; // Where to redirect for login
    options.LogoutPath = "/Account/Logout"; // Where to redirect for logout
    options.AccessDeniedPath = "/Home/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // Set cookie expiration time
    options.Cookie.SecurePolicy = CookieSecurePolicy.None;  // Make sure cookies are secure
    options.SessionStore = new MemoryCacheTicketStore();

});

builder.Services.AddScoped<PermissionService>();
builder.Services.AddScoped<IAccessControlService, AccessControlService>();
builder.Services.AddScoped<IEmailService, EmailService>();

//Globally retrieve LIP, LMAC, and CreatedBy, UpdatedBy, DeletedBy fields. [Added by Siam]
//builder.Services.AddHttpContextAccessor(); // 1?? Required for accessing HttpContext
builder.Services.AddScoped<UserInfoActionFilter>();

builder.Services.AddEndpointsApiExplorer(); // Swagger for API documentation
// Swagger for API documentation
//builder.Services.AddSwaggerGen(c =>
//{
//    c.SwaggerDoc("v1", new OpenApiInfo { Title = "GCTL API", Version = "v1" });

//    // Only include [ApiController] controllers
//    c.DocInclusionPredicate((docName, apiDesc) =>
//    {
//        var actionDescriptor = apiDesc.ActionDescriptor as ControllerActionDescriptor;
//        return actionDescriptor?.ControllerTypeInfo.GetCustomAttributes(typeof(ApiControllerAttribute), true).Any() ?? false;
//    });
//});
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


builder.Services.AddControllers(); // For API controllers
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.AddService<UserInfoActionFilter>(); // 2?? Global filter registration
});
#endregion

// localization
builder.Services.AddScoped<ILocalizationContext, LocalizationContext>();

QuestPDF.Settings.License = LicenseType.Community;


var app = builder.Build();

#region Seed data before running app using bogus
//using (var scope = app.Services.CreateScope())
//{
//    var services = scope.ServiceProvider;
//    var seeder = services.GetRequiredService<DataSeeder>();
//    var context = services.GetRequiredService<AppDbContext>();

//    if (!context.Employees.Any())
//        await seeder.SeedEmployeesAsync(context);

//    if (!context.EmployeeOfficeInfo.Any())
//        await seeder.SeedEmployeeOfficeInfoAsync(context);

//    if (!context.Shifts.Any())
//        await seeder.SeedShiftsAsync(context);
//}
#endregion


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    //app.UseDeveloperExceptionPage(); // Detailed error pages in development
    //app.UseSwagger(); // Swagger for development
    //app.UseSwaggerUI(); // Swagger for development
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "GCTL API V1");
        c.RoutePrefix = "swagger"; // Swagger UI will be at /swagger
    });
}


#region Language
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
