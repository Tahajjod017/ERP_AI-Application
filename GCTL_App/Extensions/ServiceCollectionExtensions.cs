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

using GCTL.Service.VisitingPath;

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
using GCTL.Service.Employees.EmployeePersonal;
using GCTL.Service.Employees.EmployeeOfficial;
using GCTL.Service.MasterSetup.LicenceType;
using GCTL.Service.MasterSetup.Organizations;
using GCTL.Service.MasterSetup.PassingYear;
using GCTL.Service.MasterSetup.ProvisionPeriodTimeType;
using GCTL.Service.MasterSetup.ResultType;
using GCTL.Service.MasterSetup.ServiceYear;
using GCTL.Service.MasterSetup.TrainingYear;
using GCTL.Service.MasterSetup.YearlyEndBonusType;
using GCTL.Service.AttendanceManagement.ScheduleManagement.AddShift;
using GCTL.Service.Employees.EmployeeSalary;
using GCTL.Service.Employees.EmployeeBenifit;
using GCTL.Service.AttendanceManagement.LeaveManagements.LeaveRequest;
using GCTL.Service.UserProfile;
using GCTL.Service.Employees.EmployeeAllowance;
using GCTL.Service.Employees.EmployeeAdditional;
using GCTL.Service.Employees.EmployeeTraining;
using GCTL.Service.Employees.EmployeeEducational;
using GCTL.Service.Employees.EmployeeFamily;
using GCTL.Service.RolePermissions;

using GCTL.Service.AttendanceManagement.LeaveManagements.LeaveSettings;

using GCTL.Service.Employees.EmployeeContact;
using GCTL.Service.AttendanceManagement.ScheduleManagement.AssignDefaultShift;
using GCTL.Service.Employees.EmployeeList;
using GCTL.Service.Employees.EmployeeDetails;
using GCTL.Service.Employees.EmployeeNavigation;
using GCTL.Service.ElementPermission;
using GCTL.Service.AdminSettings.SystemSettings.Emailsettingservice;
using GCTL.Service.AdminSettings.SystemSettings.EmailSettingService;
using GCTL.Service.AttendanceManagement.ScheduleManagement.OfficeDayRoster;

using GCTL.Service.AttendanceManagement.LeaveManagements.LeaveApprovalDecline;

using GCTL.Service.AdminSettings.OrganizationSettings.HolidayService;

using GCTL.Service.AdminSettings.OrganizationSettings.WeekendService;
using GCTL.Service.AdminSettings.OrganizationSettings.ApprovalService;
using GCTL.Service.AdminSettings.OrganizationSettings.CompanyService;
using GCTL.Service.ImageFileHandler;
using GCTL.Service.Employees.EmployeeReport;

using GCTL.Service.FileHandler;

using GCTL.Service.AdminSettings.OrganizationSettings.BranchService;
using GCTL.Service.AdminSettings.OrganizationSettings.DepartmentService;
using GCTL.Service.HRMsettings.ProbationService;
using GCTL.Service.AdminSettings.OrganizationSettings.DesignationService;
using GCTL.Service.AttendanceManagement.EmployeeAttendence;









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
            services.AddScoped<IDesignationService, GCTL.Service.MasterSetup.Designation.DesignationSettingService>();
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
            services.AddScoped<ILicenceTypeService, LicenceTypeService>();
            services.AddScoped<IOrganizationsService, OrganizationsService>();
            services.AddScoped<IPassingYearService, PassingYearService>();
            services.AddScoped<IProvisionPeriodTtimeTypesService, ProvisionPeriodTtimeTypesService>();
            services.AddScoped<IResultTypeService, ResultTypeService>();
            services.AddScoped<IServiceYearService, ServiceYearService>();
            services.AddScoped<ITrainingYearService, TrainingYearService>();
            services.AddScoped<IYearlyEndBonusTypeService, YearlyEndBonusTypeService>();
            services.AddScoped<IAddShiftService, AddShiftService>();
            services.AddScoped<IAssignDefaultShiftService, AssignDefaultShiftService>();
            //services.AddScoped<IOfficeDayRosterService, OfficeDayRosterService>();
            #endregion


            //Siam 
            services.AddScoped<IActionLogService, ActionLogService>();
            services.AddScoped<IUserInfoService, UserInfoService>();
            services.AddScoped<IVisitingPathService, VisitingPathService>();
            services.AddScoped<ILeaveRequestService , LeaveRequestService>();
            services.AddScoped<ILeaveSettingsService , LeaveSettingsService>();
            services.AddScoped<ILeaveApprovalService , LeaveApprovalService>();

            #region Asad
            services.AddScoped<IUserProfileService, UserProfileService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IElementPermissionService, ElementPermissionService>();
            services.AddScoped<IEmailSettingService, EmailSettingsService>();
            services.AddScoped<IHolidaySettingService, HolidaySettingService>();
            services.AddScoped<IWeekendSettingService, WeekendSettingService>();
            services.AddScoped<IApprovalSettingService, ApprovalSettingService>();
            services.AddScoped<ICompanySettingService, CompanySettingService>();
            services.AddScoped<IBranchSettingService, BranchSettingService>();
            services.AddScoped<IDesignationSettingService, GCTL.Service.AdminSettings.OrganizationSettings.DesignationService.DesignationSettingService>();
            services.AddScoped<IDepartmentSettingService, DepartmentSettingService>();
            services.AddScoped<IProbationSettingService, ProbationSettingService>();
            services.AddScoped<IEmployeeAttendanceReport, EmployeeAttendanceService>();
            #endregion


            #region Language Services


            services.AddScoped<ITranslateService, TranslateService>();
            services.AddScoped<ILanguageTableService, LanguageTableService>();

            #endregion


            #region Employee Services

            services.AddScoped<IEmployeePersonalService, EmployeePersonalService>();
            services.AddScoped<IEmployeeOfficialService, EmployeeOfficialService>();
            services.AddScoped<IEmployeeSalaryService, EmployeeSalaryService>();
            services.AddScoped<IEmployeeBenifitService, EmployeeBenifitService>();
            services.AddScoped<IEmployeeAllowanceService, EmployeeAllowanceService>();

            services.AddScoped<IEmployeeAdditionalService, EmployeeAdditionalService>();
            services.AddScoped<IEmployeeTrainingService, EmployeeTrainingService>();
            services.AddScoped<IEmployeeEducationalService, EmployeeEducationalService>();
            services.AddScoped<IEmployeeFamilyService, EmployeeFamilyService>();
            services.AddScoped<IEmployeeContactService, EmployeeContactService>();
            services.AddScoped<IEmployeeListService, EmployeeListService>();
            services.AddScoped<IEmployeeDetailsService, EmployeeDetailsService>();

            services.AddScoped<IEmployeeNavigationService, EmployeeNavigationService>();
            services.AddScoped<IEmployeeReportService, EmployeeReportService>();

            #endregion


            #region File Handler

            services.AddScoped<IImageFileHandlerService, ImageFileHandlerService>();
            services.AddScoped<IPdfFileHandler, PdfFileHandler>();

            #endregion
        }
    }
}
