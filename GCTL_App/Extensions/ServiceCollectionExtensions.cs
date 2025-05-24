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
using GCTL.Service.ActionLogAudit;
using GCTL.Service.MasterSetup.BloodGroups;
using GCTL.Service.MasterSetup.Countries;
using GCTL.Service.MasterSetup.Currency;
using GCTL.Service.MasterSetup.Degrees;
using GCTL.Service.MasterSetup.Department;
using GCTL.Service.MasterSetup.Designation;
using GCTL.Service.MasterSetup.EducationBoards;
using GCTL.Service.MasterSetup.EducationLevel;
using GCTL.Service.MasterSetup.EmployeeTypes;
using GCTL.Service.MasterSetup.EmploymentNatures;
using GCTL.Service.MasterSetup.Gender;
using GCTL.Service.MasterSetup.Grades;
using GCTL.Service.MasterSetup.MaritalStatuses;
using GCTL.Service.MasterSetup.PaymenPeriodType;
using GCTL.Service.MasterSetup.PaymentMode;
using GCTL.Service.MasterSetup.Religion;
using GCTL.Service.MasterSetup.Statuse;

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

            #region Added by Md. Rakib Hasan
            services.AddScoped<IActionTakenService, ActionTakenService>();
            services.AddScoped<IBloodGroupService, BloodGroupService>();
            services.AddScoped<ICountryService, CountryService>();
            services.AddScoped<ICurrencyService, CurrencyService>();
            services.AddScoped<IDegreeService, DegreeService>();
            services.AddScoped<IDepartmentService, DepartmentService>();
            services.AddScoped<IDesignationService, DesignationService>();
            services.AddScoped<IEducationBoardService, EducationBoardService>();
            services.AddScoped<IEducationLevelsService, EducationLevelService>();
            services.AddScoped<IEmployeeTypesService, EmployeeTypesService>();
            services.AddScoped<IEmploymentNatureService, EmploymentNatureService>();
            services.AddScoped<IGenderService, GenderService>();
            services.AddScoped<IGradeService, GradeService>();
            services.AddScoped<IMaritalStatusService, MaritalStatusService>();
            services.AddScoped<IPaymentPeriodsService, PaymentPeriodsService>();
            services.AddScoped<IPaymentModeService, PaymentModesService>();
            services.AddScoped<IReligionService, ReligionService>();
            services.AddScoped<IStatusService, StatusService>();
            #endregion


            //Siam 
            services.AddScoped<IActionLogService, ActionLogService>();
            services.AddScoped<IUserInfoService, UserInfoService>();

            #region Language Services

            services.AddScoped<ITranslateService, TranslateService>();
            services.AddScoped<ILanguageTableService, LanguageTableService>();

            #endregion


            #region Employee Services

            services.AddScoped<IEmployeePersonalService, EmployeePersonalService>();


            #endregion

        }
    }
}
