using System.Data;
using System;
using GCTL.Data.Models;
using Microsoft.EntityFrameworkCore;
using GCTL.Core.Configurations;
using GCTL.Service.MenuTabs;
using GCTL.Core.Repository;
using GCTL.Service.MasterSetup.ActionTakens;
using Microsoft.Data.SqlClient;
using GCTL.Service.Language;

namespace GCTL_App.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void ConfigureContext(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("connection"))
            .EnableSensitiveDataLogging().LogTo(Console.WriteLine, LogLevel.Information));
        }

        public static void ConfigureDapperConnection(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IDbConnection>(sp =>
                new SqlConnection(configuration.GetConnectionString("connection")));
        }

        public static void ConfigureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Main Settings Start
            services.Configure<ApplicationSettings>(configuration.GetSection("ApplicationSettings"));
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IMenuTabsService, MenuTabsService>();
            //services.AddScoped<IPaginationService, PaginationService>();

            // Master Setup Start
            services.AddScoped<IActionTakenService, ActionTakenService>();

            #region

            services.AddScoped<ITranslateService, TranslateService>();
            services.AddScoped<ILanguageTableService, LanguageTableService>();

            #endregion


        }
    }
}
