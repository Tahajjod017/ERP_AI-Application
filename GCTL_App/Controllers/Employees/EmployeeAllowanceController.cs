using GCTL.Core.Repository;
using GCTL.Core.ViewModels.Employee.EmployeeAllowance;
using GCTL.Service.Employees.EmployeeAllowance;
using GCTL.Service.Language;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GCTL_App.Controllers.Employees
{
    public class EmployeeAllowanceController : BaseController
    {
        private readonly IGenericRepository<GCTL.Data.Models.Employees> _employeeRepository;
        private readonly IEmployeeAllowanceService _employeeAllowanceService;
        public EmployeeAllowanceController(ITranslateService translateService, IGenericRepository<GCTL.Data.Models.Employees> employeeRepository, IEmployeeAllowanceService employeeAllowanceService) : base(translateService)
        {
            _employeeRepository = employeeRepository;
            _employeeAllowanceService = employeeAllowanceService;
        }

        public IActionResult Index(int id)
        {
            PopulateViewBag();

            var model = _employeeAllowanceService.GetEmployeeAllowance(id).Result;

            SetSmartPageCode(119000);
            return View(model);
        }

        private void PopulateViewBag()
        {
            #region ViewBag

            ViewBag.EmployeeDD = new SelectList(_employeeRepository.All().Select(e => new { e.EmployeeID, FullName = e.FirstName + " " + e.LastName }), "EmployeeID", "FullName");


            ViewBag.HouseRentAllowanceDD = new SelectList(new List<SelectListItem>
            {
                new SelectListItem { Value = "35", Text = "35 %" },
                new SelectListItem { Value = "40", Text = "40 %" },
                new SelectListItem { Value = "45", Text = "45 %" },
                new SelectListItem { Value = "50", Text = "50 %" },
                new SelectListItem { Value = "60", Text = "60 %" },
                new SelectListItem { Value = "70", Text = "70 %" },
                new SelectListItem { Value = "100", Text = "100 %" }
            }, "Value", "Text");

            ViewBag.MedicalAllowanceDD = new SelectList(new List<SelectListItem>
            {
                new SelectListItem { Value = "8", Text = "8 %" },
                new SelectListItem { Value = "10", Text = "10 %" },
                new SelectListItem { Value = "15", Text = "15 %" }
            }, "Value", "Text");

            ViewBag.ConveyanceAllowanceDD = new SelectList(new List<SelectListItem>
            {
                new SelectListItem { Value = "8", Text = "8 %" },
                new SelectListItem { Value = "10", Text = "10 %" },
                new SelectListItem { Value = "15", Text = "15 %" }
            }, "Value", "Text");

            #endregion
        }


        [HttpPost]
        public async Task<IActionResult> Index(EmployeeAdditionalPostViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    );

                return BadRequest(errors);
            }

            try
            {
                var res = await _employeeAllowanceService.SaveEmployeeAllowanceAsync(model);

                return Ok(res);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetEmployeeAllowance(int employeeId)
        {
            try
            {
                
                 var allowanceData = await _employeeAllowanceService.GetEmployeeAllowance(employeeId);

                // Return the data as JSON
                return Json(new
                {
                    employeePersonalId = allowanceData.EmployeePersonalId,
                    employeeBaseAllowanceID =  allowanceData?.EmployeeBaseAllowanceID ?? 0,
                    personalEmail = allowanceData.PersonalEmail ?? "",
                    personalPhone = allowanceData.PersonalPhone,
                    mobileInternetAllowance = allowanceData?.MobileInternetAllowance,
                    isMobileInternetAllowanceEnabled =  allowanceData?.IsMobileInternetAllowanceEnabled ?? false,
                    shiftAllowance =allowanceData?.ShiftAllowance,
                    isShiftAllowanceEnabled =  allowanceData?.IsShiftAllowanceEnabled ?? false,
                    houseRentAllowancePercentage =  allowanceData?.HouseRentAllowancePercentage,
                    isHouseRentAllowancePercentageEnabled =  allowanceData?.IsHouseRentAllowancePercentageEnabled ?? false,
                    medicalAllowancePercentage =  allowanceData?.MedicalAllowancePercentage,
                    isMedicalAllowancePercentageEnabled =  allowanceData?.IsMedicalAllowancePercentageEnabled ?? false,
                    conveyanceAllowancePercentage =  allowanceData?.ConveyanceAllowancePercentage,
                    isConveyanceAllowancePercentageEnabled =  allowanceData?.IsConveyanceAllowancePercentageEnabled ?? false,
                    isEmployeeAllowanceEnabled =  allowanceData?.IsEmployeeAllowanceEnabled ?? false
                });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }
    }
}
