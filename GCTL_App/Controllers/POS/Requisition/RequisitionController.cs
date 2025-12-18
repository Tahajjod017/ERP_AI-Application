using GCTL.Core.Repository;
using GCTL.Core.ViewModels.POS.Requsition.AddRequisition;
using GCTL.Data.Models;
using GCTL.Service.Language;
using GCTL.Service.POS.Requsition;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GCTL_App.Controllers.POS.Requisition
{
    public class RequisitionController : BaseController
    {

        private readonly IGenericRepository<Requisitions> _requisitionRepository;
        private readonly IGenericRepository<ReqItemApprovalHistory> _reqItemHistoryRepository;
        private readonly IGenericRepository<ProductTypes> _productTypesRepository;
        private readonly IGenericRepository<Products> _productRepository;
       
        private readonly IGenericRepository<UnitTypes> _unitRepository;

        private readonly INewRequisitionService _newRequisitionService;

        private readonly IGenericRepository<GCTL.Data.Models.Employees> _employeeRepository;
        private readonly IGenericRepository<Statuses> _statusRepository;
        private readonly IGenericRepository<Organization> _organizationRepository;


        public RequisitionController(ITranslateService translateService, IUserProfileService userProfileService, IGenericRepository<Requisitions> requisitionRepository, IGenericRepository<ReqItemApprovalHistory> reqItemHistoryRepository, IGenericRepository<ProductTypes> productTypesRepository, IGenericRepository<Products> productRepository, IGenericRepository<UnitTypes> unitRepository, INewRequisitionService newRequisitionService, IGenericRepository<GCTL.Data.Models.Employees> employeeRepository, IGenericRepository<Statuses> statusRepository, IGenericRepository<Organization> organizationRepository) : base(translateService, userProfileService)
        {
            _requisitionRepository = requisitionRepository;
            _reqItemHistoryRepository = reqItemHistoryRepository;
            _productTypesRepository = productTypesRepository;
            _productRepository = productRepository;
            _unitRepository = unitRepository;
            _newRequisitionService = newRequisitionService;
            _employeeRepository = employeeRepository;
            _statusRepository = statusRepository;
            _organizationRepository = organizationRepository;
        }

        public IActionResult Index()
        {
            ViewBag.ProductTypes = new SelectList(_productTypesRepository.AllActive().Select(e => new { Id = e.ProductTypeID, Name = e.ProductTypeName }).ToList(), "Id", "Name");
          
            ViewBag.Products = new SelectList(_productRepository.AllActive().Select(e => new { Id = e.ProductID, Name = e.ProductName }).ToList(), "Id", "Name");

            ViewBag.Units = new SelectList(_unitRepository.AllActive().Select(e => new { Id = e.UnitTypeID, Name = e.UnitTypeName }).ToList(), "Id", "Name");
           
            ViewBag.SubCategories = new SelectList(_statusRepository.AllActive().Select(e => new { Id = e.StatusID, Name = e.StatusName }).ToList(), "Id", "Name");

           

            int? empID = GetCurrentEmployeeIdAsync().Result;

            ViewBag.empIDInti = empID;

            ViewBag.Supervisors = new SelectList(_employeeRepository.AllActive()
                     .Include(e => e.EmployeeOfficeInfoEmployee)
                     .ThenInclude(r => r.Designation)
                     .Where(e => e.EmployeeID == empID)
                     .Select(e => new
                     {
                         id = e.EmployeeID,
                         name = e.FirstName + " " + e.LastName +
                                (e.EmployeeOfficeInfoEmployee.FirstOrDefault().Designation != null
                                    ? " (" + e.EmployeeOfficeInfoEmployee.FirstOrDefault().Designation.DesignationName + ")"
                                    : "")
                     }).ToList(),
                 "id", "name");

           

            return View(new CreateRequisitionViewModel());
        }
    }
}
