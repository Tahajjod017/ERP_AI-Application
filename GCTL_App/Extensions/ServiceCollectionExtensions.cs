#region References
using System;
using System.Data;
using GCTL.Core.Configurations;
using GCTL.Core.Helpers.AttendenceHelper;
using GCTL.Core.Helpers.CommonSelectMasterDropDown;
using GCTL.Core.Helpers.FeviconDynamic;
using GCTL.Core.Repository;
//using GCTL.Core.SeedData;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.AdminSettings.GeneralSettings;
using GCTL.Service.AdminSettings.OrganizationSettings.ApprovalService;
using GCTL.Service.AdminSettings.OrganizationSettings.BranchService;
using GCTL.Service.AdminSettings.OrganizationSettings.CompanyService;
using GCTL.Service.AdminSettings.OrganizationSettings.DepartmentService;
using GCTL.Service.AdminSettings.OrganizationSettings.DesignationService;
using GCTL.Service.AdminSettings.OrganizationSettings.HolidayService;
using GCTL.Service.AdminSettings.OrganizationSettings.WeekendService;
using GCTL.Service.AdminSettings.SystemSettings.Emailsettingservice;
using GCTL.Service.AdminSettings.SystemSettings.EmailSettingService;
using GCTL.Service.AdminSettings.SystemSettings.ISmsSettingService;
using GCTL.Service.AdminSettings.SystemSettings.OtpSettingService;
using GCTL.Service.AdminSettings.SystemSettings.SmsSettingService;
using GCTL.Service.AllNotifications;
using GCTL.Service.AttendanceManagement;
using GCTL.Service.AttendanceManagement.EmployeeAttendence;
using GCTL.Service.AttendanceManagement.EmployeeAttendenceReportAll.DailyReports;
using GCTL.Service.AttendanceManagement.EmployeeAttendenceReportAll.MonthlyReports;
using GCTL.Service.AttendanceManagement.EmployeeAttendenceReportAll.YearlyReports;
using GCTL.Service.AttendanceManagement.LeaveManagements.LeaveApprovalDecline;
using GCTL.Service.AttendanceManagement.LeaveManagements.LeaveHistoryBalances;
using GCTL.Service.AttendanceManagement.LeaveManagements.LeaveRequest;
using GCTL.Service.AttendanceManagement.LeaveManagements.LeaveSettings;
using GCTL.Service.AttendanceManagement.ManualAttendence;
using GCTL.Service.AttendanceManagement.ScheduleManagement.AddShift;
using GCTL.Service.AttendanceManagement.ScheduleManagement.AssignDefaultShift;
using GCTL.Service.AttendanceManagement.ScheduleManagement.AssignSpiralPattern;
using GCTL.Service.AttendanceManagement.ScheduleManagement.Attendances;
using GCTL.Service.AttendanceManagement.ScheduleManagement.CreateSpiralPattern;
using GCTL.Service.AttendanceManagement.ScheduleManagement.EmployeeShiftView;
using GCTL.Service.AttendanceManagement.ScheduleManagement.OffDayRoster;
using GCTL.Service.AttendanceManagement.ScheduleManagement.OfficeDayRoster;
using GCTL.Service.BackgroundServices;
using GCTL.Service.CommonService;
using GCTL.Service.CRM;
using GCTL.Service.CRM.AddTeam;
using GCTL.Service.CRM.Customer;
using GCTL.Service.CRM.LeadCreate;
using GCTL.Service.CRM.LeadDetail;
using GCTL.Service.CRM.LeadsActivities;
using GCTL.Service.ElementPermission;
using GCTL.Service.Employees.EmployeeAdditional;
using GCTL.Service.Employees.EmployeeAllowance;
using GCTL.Service.Employees.EmployeeBenifit;
using GCTL.Service.Employees.EmployeeContact;
using GCTL.Service.Employees.EmployeeDetails;
using GCTL.Service.Employees.EmployeeEducational;
using GCTL.Service.Employees.EmployeeFamily;
using GCTL.Service.Employees.EmployeeList;
using GCTL.Service.Employees.EmployeeNavigation;
using GCTL.Service.Employees.EmployeeOfficial;
using GCTL.Service.Employees.EmployeePersonal;
using GCTL.Service.Employees.EmployeeReport;
using GCTL.Service.Employees.EmployeeResign;
using GCTL.Service.Employees.EmployeeSalary;
using GCTL.Service.Employees.EmployeeStatus.Increment;
using GCTL.Service.Employees.EmployeeStatus.Promotion;
using GCTL.Service.Employees.EmployeeTermination;
using GCTL.Service.Employees.EmployeeTraining;
using GCTL.Service.Employees.EmpTransfer;
using GCTL.Service.FieldServices;
using GCTL.Service.FieldServices.Advanced_Apporval;
using GCTL.Service.FieldServices.EmployeeAdvanced;
using GCTL.Service.FileHandler;
using GCTL.Service.Finance.AddJournal;
using GCTL.Service.Finance.AddMainAccount;
using GCTL.Service.Finance.AddSubAccount;
using GCTL.Service.Finance.BaseAccount;
using GCTL.Service.Finance.OpeningBalance;
using GCTL.Service.Finance.PostingRule;
using GCTL.Service.Finance.SecondTab;
using GCTL.Service.Finance.TransactionAccount;
using GCTL.Service.HRMsettings.ProbationService;
using GCTL.Service.ImageFileHandler;
using GCTL.Service.Language;
using GCTL.Service.MasterSetup.BloodGroups;
using GCTL.Service.MasterSetup.CompanyTypes;
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
using GCTL.Service.MasterSetup.JobTypes;
using GCTL.Service.MasterSetup.LeadActivityType;
using GCTL.Service.MasterSetup.LeadSource;
using GCTL.Service.MasterSetup.LeadStatus;
using GCTL.Service.MasterSetup.LeadStatuses;
using GCTL.Service.MasterSetup.LicenceType;
using GCTL.Service.MasterSetup.MaritalStatuses;
using GCTL.Service.MasterSetup.Organizations;
using GCTL.Service.MasterSetup.PassingYear;
using GCTL.Service.MasterSetup.PaymenPeriodType;
using GCTL.Service.MasterSetup.PaymentMode;
using GCTL.Service.MasterSetup.Priority;
using GCTL.Service.MasterSetup.ProvisionPeriodTimeType;
using GCTL.Service.MasterSetup.Religion;
using GCTL.Service.MasterSetup.ResultType;
using GCTL.Service.MasterSetup.ServiceType;
using GCTL.Service.MasterSetup.ServiceYear;
using GCTL.Service.MasterSetup.Statuse;
using GCTL.Service.MasterSetup.TrainingYear;
using GCTL.Service.MasterSetup.YearlyEndBonusType;
using GCTL.Service.MenuTabs;
using GCTL.Service.PayRollManagements.EmpAllowanceTypeOrgaization;
using GCTL.Service.PayRollManagements.PayRollEmpAllowance;
using GCTL.Service.PayRollManagements.PayRollEmpSalary;
using GCTL.Service.PayRollManagements.PayRollLoanManagement;
using GCTL.Service.PayRollManagements.PayRollOrgaBenefitsType;
using GCTL.Service.PayRollManagements.PayRollPolicy;
using GCTL.Service.PayRollManagements.PayRollSettings;
using GCTL.Service.POS.Inventory;
using GCTL.Service.POS.Product;
using GCTL.Service.POS.Product.ServiceProduct;
using GCTL.Service.POS.Purchase.PurchaseOrder;
using GCTL.Service.POS.Purchase.PurchaseOrderList;
using GCTL.Service.POS.Purchase.PurchaseReceive;
using GCTL.Service.POS.Requsition;
using GCTL.Service.POS.Requsition.RequisitionApprover;
using GCTL.Service.POS.Requsition.RequisitionToPurchaseOrder;
using GCTL.Service.POS.Sales.InvoiceF;
using GCTL.Service.POS.Sales.InvoiceListF;
using GCTL.Service.POS.Sales.PriceQuotation;
using GCTL.Service.POS.Sales.PriceQuotationList;
using GCTL.Service.POS.Sales.SalesOrderF;
using GCTL.Service.POS.Sales.SalesOrderList;
using GCTL.Service.POS.Sales.Shipment;
using GCTL.Service.POS.Sales.ShipmentList;
using GCTL.Service.RolePermissions;
using GCTL.Service.UserProfile;
using GCTL.Service.VisitingPath;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;


#endregion

namespace GCTL_App.Extensions
{
    public static class ServiceCollectionExtensions
    {
        #region Connection
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
        #endregion

        public static void ConfigureServices(this IServiceCollection services, IConfiguration configuration)
        {
            #region Main Settings Start
            services.Configure<ApplicationSettings>(configuration.GetSection("ApplicationSettings"));
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IMenuTabsService, MenuTabsService>();
            services.AddScoped<ICommonService, CommonService>();
            #endregion

            //services.AddScoped<DataSeeder>();
            services.AddScoped<IEmailService, EmailService>();


            #region Added by Md. Rakib Hasan
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
            services.AddScoped<IOfficeDayRosterService, OfficeDayRosterService>();
            services.AddScoped<IOffDayRosterService, OffDayRosterService>();
            services.AddScoped<ICreateSpiralPatternService, CreateSpiralPatternService>();
            services.AddScoped<IAssignSpiralPatternService, AssignSpiralPatternService>();
            services.AddScoped<IEmployeeShiftViewService, EmployeeShiftViewService>();
            services.AddScoped<IAppsAttendanceService, AppsAttendanceService>();
            services.AddScoped<IBaseAccountService, BaseAccountService>();
            services.AddScoped<ISecondTabService, SecondTabService>();
            services.AddScoped<IAddMainAccountService, AddMainAccountService>();
            services.AddScoped<IAddSubAccountService, AddSubAccountService>();
            services.AddScoped<ITransactionAccountService, TransactionAccountService>();
            services.AddScoped<IPostingRulesService, PostingRulesService>();
            services.AddSingleton<IBackgroundTask, AttendanceTask>();
            services.AddScoped<IAddJournalService, AddJournalService>();
            services.AddScoped<IOpeningBalancesService, OpeningBalancesService>();
            #endregion


            #region //Siam 
            services.AddScoped<IActionLogService, ActionLogService>();
            services.AddScoped<IUserInfoService, UserInfoService>();
            services.AddScoped<IVisitingPathService, VisitingPathService>();
            services.AddScoped<ILeaveRequestService , LeaveRequestService>();
            services.AddScoped<ILeaveSettingsService , LeaveSettingsService>();
            services.AddScoped<ILeaveApprovalService , LeaveApprovalService>();
            services.AddScoped<ILeaveHistoryBalancesService, LeaveHistoryBalancesService>();
            services.AddScoped<IEmployeeTransferService, EmployeeTransferService>();
            services.AddScoped<INotificationsService, NotificationsService>();
            services.AddScoped<IEmpTransferApprovedOrDeclineService, EmpTransferApprovedOrDeclineService>();
            services.AddScoped<IEmployeeBenefitsService, EmployeeBenefitsService>();
            services.AddScoped<IPayRollEmpAllowanceService, PayRollEmpAllowanceService>();
            services.AddScoped<IPayRollEmpSalaryService, PayRollEmploSalaryService>();
            services.AddScoped<IPayRollTaxperCentangeSettingsService, PayRollTaxpercentageSettingsService>();
            services.AddScoped<IEmpAllowanceTypeOrganizationService, EmpAllowanceTypeOrganizationService>();
            services.AddScoped<IPayRollLoanEntryService, PayRollLoanEntryService>();
            services.AddScoped<IPayRollEarlyPaymentService, PayRollEarlyPaymentService>();
            services.AddScoped<IPayRollOrgaBenefitsTypeService, PayRollOrgaBenefitsTypeService>();
            services.AddScoped<ICommonDroDownService, CommonDropDownService>();
            services.AddSingleton<IBackgroundTask, GeneratePayslipTask>();
            #endregion

            #region Asad
            services.AddScoped<IUserProfileService, UserProfileService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IElementPermissionService, ElementPermissionService>();
            services.AddScoped<IEmailSettingService, EmailSettingsService>();
            services.AddScoped<ISmsSettingsService, SmsSettingsService>();
            services.AddScoped<IOtpSettingService, OtpSettingService>();
            services.AddScoped<IHolidaySettingService, HolidaySettingService>();
            services.AddScoped<IWeekendSettingService, WeekendSettingService>();
            services.AddScoped<IApprovalSettingService, ApprovalSettingService>();
            services.AddScoped<ICompanySettingService, CompanySettingService>();
            services.AddScoped<IBranchSettingService, BranchSettingService>();
            services.AddScoped<IDesignationSettingService, GCTL.Service.AdminSettings.OrganizationSettings.DesignationService.DesignationSettingService>();
            services.AddScoped<IDepartmentSettingService, DepartmentSettingService>();
            services.AddScoped<IProbationSettingService, ProbationSettingService>();
            services.AddScoped<IEmployeeAttendanceReport, EmployeeAttendanceService>();
            services.AddScoped<IDailyReportService, DailyReportService>();
            services.AddTransient<HolidayHelper>();
            services.AddTransient<WeekendHelper>();
            services.AddTransient<LeaveHelper>();
            services.AddScoped<IMonthlyReportService, MonthlyReportService>();
            services.AddScoped<IYearlyReportService, YearlyReportService>();
            services.AddScoped<ILocalizationSettingService, LocalizationSettingService>();
            services.AddScoped<IBrandingAssetService, BrandingAssetService>();

            #endregion

            #region POS

            #region Product
            services.AddScoped<ISingleProduct, SingleProductService>();
            services.AddScoped<IAttributeProduct, AttributeProductService>();
            services.AddScoped<IServiceProduct, ServiceProductService>();
            #endregion

            #region Sales

            services.AddScoped<IPriceQuotation, PriceQuotationService>();
            services.AddScoped<IPriceQuotationList, PriceQuotationListService>();

            services.AddScoped<ISalesOrder, SalesOrderService>();
            services.AddScoped<ISalesOrderList, SalesOrderListService>();

            services.AddScoped<IInvoice, InvoiceService>();
            services.AddScoped<IInvoiceList, InvoiceListService>();

            // Shipment Services
            services.AddScoped<IChallan, ChallanService>();
            services.AddScoped<IChallanList, ChallanListService>();


            #endregion

            #region Purchase

            // Purchase Order Services
            services.AddScoped<IPurchaseOrder, PurchaseOrderService>();
            services.AddScoped<IPurchaseOrderList, PurchaseOrderListService>();
            services.AddScoped<IPurchaseReceiveService, PurchaseReceiveService>();

            #endregion

            #region Requisition
            services.AddScoped<INewRequisitionService, NewRequisitionService>();
            services.AddScoped<IRequisitionApproverService, RequisitionApproverService>();
            services.AddScoped<IRequisitionToPurchaseOrderService, RequisitionToPurchaseOrderService>();

            #endregion

            services.AddScoped<IInventoryService, InventoryService>();

            #region Inventory

            #endregion

            #endregion

            #region Tahajjod 
            services.AddScoped<IEmployeeAdvanced,EmployeeAdvancedService>();
            services.AddScoped<IAdvancedApprovalService,AdvancedApprovalService>();
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

            #region Attendance Management Services

            services.AddScoped<IManualAttendenceService, ManualAttendenceService>();
           

            #endregion

            #region File Handler

            services.AddScoped<IImageFileHandlerService, ImageFileHandlerService>();
            services.AddScoped<IPdfFileHandler, PdfFileHandler>();

            #endregion

            #region Employee Status Management(Increment, Promotion , Termination , Resignation)

            services.AddScoped<IincrementService, IncrementService>();
            services.AddScoped<IPromotionService, PromotionService>();
            services.AddScoped<IEmployeeResign, EmployeeResignService>();
            services.AddScoped<IEmployeeTermination, EmployeeTerminationService>();


            #endregion

            #region e43
            services.AddScoped<IServiceTypeService, ServiceTypeService>();
            services.AddScoped<ILeadStatusService, LeadStatusService>();
            services.AddScoped<ILeadSourceService, LeadSourceService>();
            services.AddScoped<ILeadCreateService, LeadCreateService>();
            services.AddScoped<ICRMService, CRMService>();
            services.AddScoped<ILeadDetailsService, LeadDetailsService>();
            services.AddScoped<IPriorityService, PriorityService>();
            services.AddScoped<ILeadsActivityService, LeadsActivityService>();
            services.AddScoped<ILeadActivityTypeService, LeadActivityTypeService>();
            services.AddScoped<IAddTeamService, AddTeamService>();
            services.AddScoped<ICreateJobService, CreateJobService>();
            services.AddScoped<IJobTypeService, JobTypeService>();
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<IOrganizationTypeService, OrganizationTypeService>();
            #endregion


            

            services.AddHostedService<ScheduledTaskService>();
        }
    }
}
