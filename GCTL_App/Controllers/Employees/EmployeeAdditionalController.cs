
﻿using GCTL.Core.Repository;
using GCTL.Core.ViewModels.Employee.EmployeeAdditional;
using GCTL.Data.Models;
using GCTL.Service.Employees.EmployeeAdditional;
using GCTL.Service.Employees.EmployeeNavigation;
using GCTL.Service.Language;

using GCTL.Service.UserProfile;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GCTL_App.Controllers.Employees
{
    public class EmployeeAdditionalController : BaseController
    {

        private readonly IEmployeeAdditionalService _employeeAdditionalService;
        private readonly IGenericRepository<GCTL.Data.Models.Employees> _employeeRepository;
        private readonly IGenericRepository<GCTL.Data.Models.LicenceTypes> _licenceTypesRepository;

        private readonly IEmployeeNavigationService _employeeNavigationService;


        public EmployeeAdditionalController(ITranslateService translateService, IUserProfileService userProfileService, IEmployeeAdditionalService employeeAdditionalService, IGenericRepository<GCTL.Data.Models.Employees> employeeRepository, IGenericRepository<LicenceTypes> licenceTypesRepository, IEmployeeNavigationService employeeNavigationService) : base(translateService, userProfileService)
        {
            _employeeAdditionalService = employeeAdditionalService;
            _employeeRepository = employeeRepository;
            _licenceTypesRepository = licenceTypesRepository;
            _employeeNavigationService = employeeNavigationService;
        }

        public IActionResult Index(int id)
        {

            var navigationModel = _employeeNavigationService.GetEmployeeNavigation("AdditionalInfo");
            ViewBag.Navigation = navigationModel;

            ViewBag.EmployeeDD = new SelectList(_employeeRepository.All().Select(e => new { e.EmployeeID, FullName = e.FirstName + " " + e.LastName }), "EmployeeID", "FullName");
           
           
            ViewBag.LicenseTypeDD = _licenceTypesRepository.GetSelectListById( e => e.LicenceTypeID, e => e.LicenceTypeName);

            var employee =  _employeeAdditionalService.GetEmployeeAdditionalByIdAsync(id).Result;


            SetSmartPageCode(113000);
            return View(employee);
        }


        [HttpPost]
        public async Task<IActionResult> Index(EmployeeAdditionalPostViewModel model)
        {
           

          
                var res = await _employeeAdditionalService.SubmitAsync(model);
                return Ok(res);
            
          
            
        }

        public async Task<IActionResult> GetEmployeeData(int id)
        {

            var employee = await _employeeAdditionalService.GetEmployeeAdditionalByIdAsync(id);

            return Ok(employee);
        }
    }
}
