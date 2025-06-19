using GCTL.Core.Repository;
using GCTL.Core.ViewModels.Employee.EmployeeSalary;
using GCTL.Data.Models;
using GCTL.Service.Employees.EmployeeNavigation;
using GCTL.Service.Employees.EmployeeSalary;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GCTL_App.Controllers.Employees
{
    public class EmployeeSalaryController : BaseController
    {
        #region CTOR
        private readonly IGenericRepository<GCTL.Data.Models.Employees> _employeeRepository;
        private readonly IGenericRepository<Grade> _gradeRepository;
        private readonly IGenericRepository<Currencies> _currencyRepository;
        private readonly IGenericRepository<PaymentPeriodTypes> _paymentPeriodTypeRepository;
        private readonly IGenericRepository<PaymentModes> _paymentModeRepository;
        private readonly IGenericRepository<EmployeeSalarySettings> _employeeSalaryRepository;
        private readonly IEmployeeSalaryService _employeeSalaryService;
        private readonly IEmployeeNavigationService _employeeNavigationService;

        private readonly UserManager<ApplicationUser> _userManagerRepository2;
        private readonly IGenericRepository<GCTL.Data.Models.MenuTab> _menuTabRepository;
        private readonly IGenericRepository<RoleModulePermissions> _rolePermissionRepository;
        private readonly RoleManager<ApplicationRole> _roleManagerRepository2;

        public EmployeeSalaryController(ITranslateService translateService, IUserProfileService userProfileService, IGenericRepository<GCTL.Data.Models.Employees> employeeRepository, IGenericRepository<Grade> gradeRepository, IGenericRepository<Currencies> currencyRepository, IGenericRepository<PaymentPeriodTypes> paymentPeriodTypeRepository, IGenericRepository<PaymentModes> paymentModeRepository, IGenericRepository<EmployeeSalarySettings> employeeSalaryRepository, IEmployeeSalaryService employeeSalaryService, IEmployeeNavigationService employeeNavigationService, UserManager<ApplicationUser> userManagerRepository2, IGenericRepository<GCTL.Data.Models.MenuTab> menuTabRepository, IGenericRepository<RoleModulePermissions> rolePermissionRepository, RoleManager<ApplicationRole> roleManagerRepository2) : base(translateService, userProfileService)
        {
            _employeeRepository = employeeRepository;
            _gradeRepository = gradeRepository;
            _currencyRepository = currencyRepository;
            _paymentPeriodTypeRepository = paymentPeriodTypeRepository;
            _paymentModeRepository = paymentModeRepository;
            _employeeSalaryRepository = employeeSalaryRepository;
            _employeeSalaryService = employeeSalaryService;
            _employeeNavigationService = employeeNavigationService;
            _userManagerRepository2 = userManagerRepository2;
            _menuTabRepository = menuTabRepository;
            _rolePermissionRepository = rolePermissionRepository;
            _roleManagerRepository2 = roleManagerRepository2;
        }
        #endregion

        public async Task< IActionResult> Index(int id)
        {

            var loggedUser = await _userManagerRepository2.GetUserAsync(User);

            if (loggedUser != null)
            {

                var userId = loggedUser.Id;

                var user = await _userManagerRepository2.FindByIdAsync(userId); // Get the ApplicationUser
                var roleNames = await _userManagerRepository2.GetRolesAsync(user); // List<string> of role names

                var roleIds = _roleManagerRepository2.Roles.Where(role => roleNames.Contains(role.Name)).Select(role => role.Id).ToList();

                var menuTabs = (from rp in _rolePermissionRepository.All()
                                join mt in _menuTabRepository.All() on rp.MenuTabId equals mt.MenuTabId
                                where roleIds.Contains(rp.RoleId) && mt.ControllerName.StartsWith("Employee")
                                select mt.ControllerName).Distinct().ToList();


                var navigationModel = _employeeNavigationService.GetEmployeeNavigation(menuTabs, "PayrollInfo",  "EmployeeSalary");
                ViewBag.Navigation = navigationModel;
            }

           

            ViewBag.EmployeeDD = new SelectList( _employeeRepository.All().Select(e => new { e.EmployeeID, FullName = e.FirstName + " " + e.LastName }), "EmployeeID", "FullName");
            ViewBag.GradeDD = new SelectList( _gradeRepository.All().Select(o => new { o.GradeID, o.GradeName }), "GradeID", "GradeName" );     
            ViewBag.CurrencyDD = new SelectList( _currencyRepository.All().Select(o => new { o.CurrencyID, o.CurrencyName }), "CurrencyID", "CurrencyName");     
            ViewBag.PaymenPeriodTypeDD = new SelectList( _paymentPeriodTypeRepository.All().Select(o => new { o.PaymentPeriodTypeID, o.PaymentPeriodTypeName }), "PaymentPeriodTypeID", "PaymentPeriodTypeName");     
            ViewBag.PaymenModeDD = new SelectList( _paymentModeRepository.All().Select(o => new { o.PaymentModeID, o.PaymentModeName }), "PaymentModeID", "PaymentModeName");     
            
            SetSmartPageCode(117000);

            var employeeSalaryData = await _employeeSalaryService.GetEmployeeSalaryByEmployeeIdPostAsync(id);

            return View(employeeSalaryData);
        }

        #region GetEmployeeSalaryData

        [HttpGet]
        public async Task<IActionResult> GetEmployeeSalaryData(int employeeId)
        {
            try
            {
                
                var employeeSalaryData = await _employeeSalaryService.GetEmployeeSalaryByEmployeeIdAsync(employeeId);

                if (employeeSalaryData != null)
                {
                    var responseData = new
                    {
                        personalPhone = employeeSalaryData.PersonalPhone,
                        personalEmail = employeeSalaryData.PersonalEmail,
                        employeeSalarySettingsID = employeeSalaryData.EmployeeSalarySettingsID,
                        bankName = employeeSalaryData.BankName,
                        branchName = employeeSalaryData.BranchName,
                        accountName = employeeSalaryData.AccountName,
                        accountNo = employeeSalaryData.AccountNo,
                        address = employeeSalaryData.Address,
                        atmCardNo = employeeSalaryData.ATMCardNo,
                        routingNo = employeeSalaryData.RoutingNo,
                        swiftCode = employeeSalaryData.SWIFTCode,
                        ifscCode = employeeSalaryData.IFSCCode,
                        bKashAccountNo = employeeSalaryData.bKashAccountNo,
                        roketAccountNo = employeeSalaryData.RoketAccountNo,
                        nagodAccountNo = employeeSalaryData.NagodAccountNo,
                        employeeGID = employeeSalaryData.EmployeeGID,
                        gradeID = employeeSalaryData.GradeID,
                        salary = employeeSalaryData.Salary,
                        currencyID = employeeSalaryData.CurrencyID,
                        paymenPeriodTypeID = employeeSalaryData.PaymenPeriodTypeID,
                        paymentModeIds = employeeSalaryData.PaymentModeIds,
                        primaryPaymentModeId = employeeSalaryData.PrimaryPaymentModeId,
                        primaryPaymentPercent = employeeSalaryData.PrimaryPaymentPercent,
                        secondaryPaymentModeId = employeeSalaryData.SecondaryPaymentModeId
                    };

                    return Json(new { success = true, data = responseData });
                }
                else
                {
                    // Return empty data structure for new employee
                    return Json(new { success = true, data = new { } });
                }
            }
            catch (Exception ex)
            {


                return Json(new
                {
                    success = false,
                    message = "An error occurred while loading employee data. Please try again."
                });
            }
        }


        #endregion

        [HttpPost]
        public async Task<IActionResult> Index(EmployeeSalaryPostViewModel model)
        {
            try
            {
                var loggedUser = await _userManagerRepository2.GetUserAsync(User);

                if (loggedUser != null)
                {

                    var userId = loggedUser.Id;

                    var user = await _userManagerRepository2.FindByIdAsync(userId); // Get the ApplicationUser
                    var roleNames = await _userManagerRepository2.GetRolesAsync(user); // List<string> of role names

                    var roleIds = _roleManagerRepository2.Roles.Where(role => roleNames.Contains(role.Name)).Select(role => role.Id).ToList();

                    var menuTabs = (from rp in _rolePermissionRepository.All()
                                    join mt in _menuTabRepository.All() on rp.MenuTabId equals mt.MenuTabId
                                    where roleIds.Contains(rp.RoleId) && mt.ControllerName.StartsWith("Employee")
                                    select mt.ControllerName).Distinct().ToList();


                    var navigationModel = _employeeNavigationService.GetEmployeeNavigation(menuTabs, "EmployeeSalary");
                    ViewBag.Navigation = navigationModel;
                }

                if (model.EmployeeSalarySettingsID == 0 || model.EmployeeSalarySettingsID == null)
                {
                    var result = await _employeeSalaryService.SaveEmployeeSalaryAsync(model);

                    if (!result.Success)
                    {
                        return Ok(result);
                    }

                    return Ok(result);
                }
                else
                {
                    var result = await _employeeSalaryService.UpdateEmployeeSalaryAsync(model);

                    if (!result.Success)
                    {
                        return Ok(result);
                    }

                    return Ok(result);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message); // Replace with proper logging
                TempData["ErrorMessage"] = "An error occurred while saving. Please try again.";

                

                return View(model);
            }
        }

       
        //[HttpGet]
        //public async Task<IActionResult> GetEmployeeSalaryData(int employeeId)
        //{
        //    try
        //    {
              

        //        var employeeSalary = await _employeeSalaryService.GetEmployeeSalaryByEmployeeIdAsync(employeeId);

        //        if (employeeSalary == null)
        //        {
        //            return Json(new { success = false, message = "No salary data found for this employee." });
        //        }

        //        var data = new
        //        {
        //            employeeSalarySettingsID = employeeSalary.EmployeeSalarySettingsID,
        //            personalPhone = employeeSalary.PersonalPhone,
        //            personalEmail = employeeSalary.PersonalEmail,
        //            bankName = employeeSalary.BankName,
        //            branchName = employeeSalary.BranchName,
        //            address = employeeSalary.Address,
        //            accountName = employeeSalary.AccountName,
        //            accountNo = employeeSalary.AccountNo,
        //            atmCardNo = employeeSalary.ATMCardNo,
        //            routingNo = employeeSalary.RoutingNo,
        //            swiftCode = employeeSalary.SWIFTCode,
        //            ifscCode = employeeSalary.IFSCCode,
        //            bKashAccountNo = employeeSalary.bKashAccountNo,
        //            roketAccountNo = employeeSalary.RoketAccountNo,
        //            nagodAccountNo = employeeSalary.NagodAccountNo,
        //            employeeGID = employeeSalary.EmployeeGID,
        //            gradeID = employeeSalary.GradeID,
        //            salary = employeeSalary.Salary,
        //            currencyID = employeeSalary.CurrencyID,
        //            paymenPeriodTypeID = employeeSalary.PaymenPeriodTypeID,
        //            paymentModeIds = employeeSalary.PaymentModeIds.ToList(),
        //            primaryPaymentModeId = employeeSalary.PrimaryPaymentModeId,
        //            primaryPaymentPercent = employeeSalary.PrimaryPaymentPercent,
        //            secondaryPaymentModeId = employeeSalary.SecondaryPaymentModeId
        //        };

        //        return Json(new { success = true, data });
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.Message); // Replace with proper logging
        //        return Json(new { success = false, message = "Error retrieving employee salary data." });
        //    }
        //}


    }
}
