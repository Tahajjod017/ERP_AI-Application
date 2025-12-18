using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.POS.Settings;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GCTL_App.Controllers.POS.Settings
{
    public class ApprovalMatrixController : BaseController
    {
        private readonly IGenericRepository<Organization> _organizationRepository;
        private readonly IGenericRepository<OrganizationBranches> _organizationBranchRepository;
        private readonly IGenericRepository<ApprovalTypes> _approvalTypeRepository;
        private readonly IGenericRepository<ApprovalSettings> _approvalSettingRepository;
        private readonly IGenericRepository<ReqApprovalSettings> _reqApprovalSettingRepository;
        private readonly IGenericRepository<ReqApprovalStepApprovers> _reqApprovalStepApproversRepository;
        private readonly IUserInfoService _userInfoService;


        private readonly IGenericRepository<GCTL.Data.Models.Employees> _employeeRepository;
        public ApprovalMatrixController(ITranslateService translateService, IUserProfileService userProfileService, IGenericRepository<Organization> organizationRepository, IGenericRepository<OrganizationBranches> organizationBranchRepository, IGenericRepository<ApprovalTypes> approvalTypeRepository, IGenericRepository<ApprovalSettings> approvalSettingRepository, IGenericRepository<GCTL.Data.Models.Employees> employeeRepository, IGenericRepository<ReqApprovalSettings> reqApprovalSettingRepository, IUserInfoService userInfoService, IGenericRepository<ReqApprovalStepApprovers> reqApprovalStepApproversRepository) : base(translateService, userProfileService)
        {
            _organizationRepository = organizationRepository;
            _organizationBranchRepository = organizationBranchRepository;
            _approvalTypeRepository = approvalTypeRepository;
            _approvalSettingRepository = approvalSettingRepository;
            _employeeRepository = employeeRepository;
            _reqApprovalSettingRepository = reqApprovalSettingRepository;
            _userInfoService = userInfoService;
            _reqApprovalStepApproversRepository = reqApprovalStepApproversRepository;
        }

        public IActionResult Index()
        {
            SetSmartPageCode(995485200);

            ViewBag.Organizations = new SelectList(_organizationRepository.AllActive().Select(e => new { e.OrganizationID, e.OrganizationName }), "OrganizationID", "OrganizationName");
            ViewBag.OrganizationBranches = new SelectList(_organizationBranchRepository.AllActive().Select(e => new { e.OrganizationBranchID, e.OrganizationBranchName }), "OrganizationBranchID", "OrganizationBranchName");
            ViewBag.ApprovalTypes = new SelectList(_approvalTypeRepository.AllActive().Select(e => new { e.ApprovalTypeID, e.ApprovalTypeName }), "ApprovalTypeID", "ApprovalTypeName");
            // ViewBag.Employees = new SelectList(_employeeRepository.AllActive().Select(e => new { e.EmployeeID, FullName = e.FirstName + " "+ e.LastName }), "EmployeeID", "FullName");


            ViewBag.Employees = new SelectList(_employeeRepository.AllActive()
                 .Include(e => e.EmployeeOfficeInfoEmployee)
                 .ThenInclude(r => r.Designation)
                 .OrderBy(e => e.EmployeeOfficeInfoEmployee.First().Designation.Ranking)

                 .Select(e => new
                 {
                     id = e.EmployeeID,
                     name = e.FirstName + " " + e.LastName +
                            (e.EmployeeOfficeInfoEmployee.FirstOrDefault().Designation != null
                                ? " (" + e.EmployeeOfficeInfoEmployee.FirstOrDefault().Designation.DesignationName + ")"
                                : "")
                 }).ToList(),
             "id", "name");



            return View();
        }



        #region Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] ApprovalSettingViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToList()
                        );

                var messages = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .SelectMany(x => x.Value.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();


                return Json(new { success = false, errors = errors, message = messages });
            }

            var ApprovalTypeName = "product" + "_" + viewModel.OrganizationID + "_" + viewModel.OrganizationBranchID;

            var exists = await _approvalTypeRepository.AllActive()
                .FirstOrDefaultAsync(a => a.OrganizationID == viewModel.OrganizationID
                    && a.OrganizationBranchID == viewModel.OrganizationBranchID
                    && a.ApprovalTypeName == ApprovalTypeName);
            if (exists != null)
            {
                return Json(new { success = false, message = "An approval setting with the same Organization, Branch, and Approval Type already exists." });
            }


            await _approvalSettingRepository.BeginTransactionAsync();

            try
            {

                var approvalType = new ApprovalTypes
                {
                    OrganizationID = viewModel.OrganizationID,
                    OrganizationBranchID = viewModel.OrganizationBranchID,
                    ApprovalTypeName = ApprovalTypeName
                };
                await _approvalTypeRepository.AddAsync(approvalType, viewModel);
                await _userInfoService.ActionLogAsync("ApprovalTypes", ActionName.DataAdd, null, approvalType, approvalType.ApprovalTypeID, viewModel);

                var model = new ReqApprovalSettings
                {

                    ApprovalTypeID = approvalType.ApprovalTypeID,
                    StartDate = viewModel.StartDate,
                    EndDate = viewModel.EndDate,

                };

                await _reqApprovalSettingRepository.AddAsync(model, viewModel);
                await _userInfoService.ActionLogAsync("ReqApprovalSettings", ActionName.DataAdd, null, model, model.ReqApprovalSettingID, viewModel);

                int i = 1;

                foreach (var item in viewModel.ApprovalLevels)
                {
                  
                   var approvalLevelAssignment = new ReqApprovalStepApprovers
                    {
                        ReqApprovalSettingID = model.ReqApprovalSettingID,
                        Step = i,
                        ApproverID = item.ApproverEmployeeID,
                        
                    };
                    i++;
                    await _reqApprovalStepApproversRepository.AddAsync(approvalLevelAssignment, viewModel);
                    await _userInfoService.ActionLogAsync("ReqApprovalStepApprovers", ActionName.DataAdd, null, approvalLevelAssignment, approvalLevelAssignment.ReqApprovalStepApproverID, viewModel);

                }



                await _approvalSettingRepository.CommitTransactionAsync();

                return Json(new { success = true, message = "Approval setting created successfully." });
            }
            catch (Exception ex)
            {
                await _approvalSettingRepository.RollbackTransactionAsync();
                return Json(new { success = false, message = ex.Message });
            }


        }

        #endregion

        #region Get Alll 

        [HttpGet]
        public async Task<IActionResult> GetApprovalSettings(int page = 1, int pageSize = 10, string search = "", string sortColumn = "ApprovalSettingID", string sortDirection = "asc", int? organizationId = null, int? approvalTypeId = null)
        {
            return Ok();

            //var query = _reqApprovalSettingRepository.AllActive()
            //    .Include(e=>e.ApprovalType).ThenInclude(a => a.Organization)
            //    .Include(e=>e.ApprovalType).ThenInclude(a => a.Organization)
            //    .Include(a => a.)
            //    .Include(a => a.ApprovalType)
            //    .Include(a => a.ApprovalLevelAssignments)
            //    .ThenInclude(al => al.ApproverEmployee)
            //    .Where(a => a.DeletedAt == null);

            //if (!string.IsNullOrEmpty(search))
            //{
            //    query = query.Where(a => a.ApprovalType.ApprovalTypeName.Contains(search) || a.Organization.OrganizationName.Contains(search));
            //}

            //if (organizationId.HasValue)
            //{
            //    query = query.Where(a => a.OrganizationID == organizationId);
            //}

            //if (approvalTypeId.HasValue)
            //{
            //    query = query.Where(a => a.ApprovalTypeID == approvalTypeId);
            //}

            //query = sortColumn switch
            //{
            //    "ApprovalSettingID" => sortDirection == "asc" ? query.OrderBy(a => a.ApprovalSettingID) : query.OrderByDescending(a => a.ApprovalSettingID),
            //    "ApprovalTypeName" => sortDirection == "asc" ? query.OrderBy(a => a.ApprovalType.ApprovalTypeName) : query.OrderByDescending(a => a.ApprovalType.ApprovalTypeName),
            //    "OrganizationName" => sortDirection == "asc" ? query.OrderBy(a => a.Organization.OrganizationName) : query.OrderByDescending(a => a.Organization.OrganizationName),
            //    _ => query.OrderBy(a => a.ApprovalSettingID)
            //};

            //var totalRecords = await query.CountAsync();
            //var data = await query
            //    .Skip((page - 1) * pageSize)
            //    .Take(pageSize)
            //    .Select(a => new
            //    {
            //        a.ApprovalSettingID,
            //        a.OrganizationID,
            //        OrganizationName = a.Organization.OrganizationName,
            //        a.OrganizationBranchID,
            //        BranchName = a.OrganizationBranch.OrganizationBranchName,
            //        a.ApprovalTypeID,
            //        ApprovalTypeName = a.ApprovalType.ApprovalTypeName,
            //        a.StartDate,
            //        a.EndDate,
            //        a.AllowSelfApproval,
            //        SelfExceptionApprovalName = a.SelfExceptionApproval != null ? a.SelfExceptionApproval.FirstName +" "+ a.SelfExceptionApproval.LastName : null,
            //        ApprovalLevels = a.ApprovalLevelAssignments.Select(al => new
            //        {
            //            al.LevelNumber,
            //            ApproverName = al.ApproverEmployee.FirstName +" "+ al.ApproverEmployee.LastName,
            //            al.IsEnabled
            //        }).ToList()
            //    })
            //    .ToListAsync();

            //return Json(new { data, totalRecords });
        }

        #endregion


    }
}
