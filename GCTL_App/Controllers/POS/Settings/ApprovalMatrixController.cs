using GCTL.Core.Helpers;
using GCTL.Core.Helpers.Jsonserialize;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.POS.Sales;
using GCTL.Core.ViewModels.POS.Settings;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using static Dapper.SqlMapper;

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

            var approvalType = await _approvalTypeRepository.AllActive()
                .FirstOrDefaultAsync(a => a.OrganizationID == viewModel.OrganizationID
                    && a.OrganizationBranchID == viewModel.OrganizationBranchID
                    && a.ApprovalTypeName == ApprovalTypeName);
           


            await _approvalSettingRepository.BeginTransactionAsync();

            try
            {
                
                if (approvalType == null)
                {
                     approvalType = new ApprovalTypes
                    {
                        OrganizationID = viewModel.OrganizationID,
                        OrganizationBranchID = viewModel.OrganizationBranchID,
                        ApprovalTypeName = ApprovalTypeName
                    };
                    await _approvalTypeRepository.AddAsync(approvalType, viewModel);
                    await _userInfoService.ActionLogAsync("ApprovalTypes", ActionName.DataAdd, null, approvalType, approvalType.ApprovalTypeID, viewModel);
                }


                var exists = await _reqApprovalSettingRepository.AllActive()
                    .FirstOrDefaultAsync(e => e.ApprovalTypeID == approvalType.ApprovalTypeID
                        && (viewModel.StartDate <= e.EndDate && viewModel.EndDate >= e.StartDate));

                if (exists != null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "An approval setting with the same Organization, Branch, and Approval Type already exists within the given date range."
                    });
                }


                //var exists = await _reqApprovalSettingRepository.AllActive().FirstOrDefaultAsync(e=>e.ApprovalTypeID == approvalType.ApprovalTypeID);

                //var a = exists.StartDate;
                //var b = exists.EndDate;
                //var c = viewModel.StartDate;
                //var d = viewModel.EndDate;

                //if (exists != null)
                //{
                //    return Json(new { success = false, message = "An approval setting with the same Organization, Branch, and Approval Type already exists." });
                //}

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


            var query = _reqApprovalSettingRepository.AllActive()
                .Include(e => e.ApprovalType).ThenInclude(a => a.Organization)
                .Include(e => e.ApprovalType).ThenInclude(a => a.Organization)
               // .Include(a => a.ReqApprovalSettingID)
                .Include(a => a.ApprovalType)
                .Include(a => a.ReqApprovalStepApprovers)
                .ThenInclude(al => al.Approver)
                .AsNoTracking();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(a => a.ApprovalType.ApprovalTypeName.Contains(search) || a.ApprovalType.Organization.OrganizationName.Contains(search));
            }

            if (organizationId.HasValue)
            {
                query = query.Where(a => a.ApprovalType.OrganizationID == organizationId);
            }

            if (approvalTypeId.HasValue)
            {
                query = query.Where(a => a.ApprovalTypeID == approvalTypeId);
            }

            query = sortColumn switch
            {
                "ApprovalSettingID" => sortDirection == "asc" ? query.OrderBy(a => a.ReqApprovalSettingID) : query.OrderByDescending(a => a.ReqApprovalSettingID),
                "ApprovalTypeName" => sortDirection == "asc" ? query.OrderBy(a => a.ApprovalType.ApprovalTypeName) : query.OrderByDescending(a => a.ApprovalType.ApprovalTypeName),
                "OrganizationName" => sortDirection == "asc" ? query.OrderBy(a => a.ApprovalType.Organization.OrganizationName) : query.OrderByDescending(a => a.ApprovalType.Organization.OrganizationName),
                _ => query.OrderBy(a => a.ReqApprovalSettingID)
            };

            var totalRecords = await query.CountAsync();
            var data = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new
                {
                    a.ReqApprovalSettingID,
                    a.ApprovalType.OrganizationID,
                    OrganizationName = a.ApprovalType.Organization.OrganizationName,
                    a.ApprovalType.OrganizationBranchID,
                    BranchName = a.ApprovalType.OrganizationBranch.OrganizationBranchName,
                    a.ApprovalTypeID,
                    ApprovalTypeName = a.ApprovalType.ApprovalTypeName,
                    StartDate = a.StartDate.Value.ToString("dd/MM/yyyy") ?? "",
                    EndDate = a.EndDate.Value.ToString("dd/MM/yyyy") ?? "",
                    a.ReqApprovalStepApprovers.Count,
                    ApprovalLevels = a.ReqApprovalStepApprovers.Select(al => new
                    {
                        al.Step,
                        ApproverName = al.Approver.FirstName + " " + al.Approver.LastName,
                       
                    }).ToList()
                })
                .ToListAsync();

            return Json(new { data, totalRecords });
        }

        #endregion




        #region Get By Id

        [HttpGet]
        public async Task<IActionResult> GetApprovalSettingById(int id)
        {
            var setting = await _reqApprovalSettingRepository.AllActive()
                .Include(e=>e.ApprovalType)
                
                .Include(a => a.ReqApprovalStepApprovers)
               
                .FirstOrDefaultAsync(a => a.ReqApprovalSettingID == id);

            if (setting == null)
            {
                return Json(new { success = false, message = "Approval setting not found." });
            }

            var viewModel = new
            {
                setting.ReqApprovalSettingID,
                setting.ApprovalType.OrganizationID,
                setting.ApprovalType.OrganizationBranchID,
                setting.ApprovalTypeID,
                setting.StartDate,
                setting.EndDate,
               
                ApprovalLevels = setting.ReqApprovalStepApprovers.Select(al => new
                {
                    al.Step,
                    al.ApproverID,
                   
                }).ToList()
            };

            return Json(new { success = true, data = viewModel });
        }

        #endregion


        #region Edit

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromForm] ApprovalSettingViewModel viewModel)
        {
            await _reqApprovalSettingRepository.BeginTransactionAsync();
            try
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

                    await _reqApprovalSettingRepository.RollbackTransactionAsync();
                    return Json(new { success = false, errors = errors, message = messages });
                }

                //var alreadyExists = await _reqApprovalSettingRepository.AllActive()
                //    .FirstOrDefaultAsync(a => a.ReqApprovalSettingID != viewModel.ApprovalSettingID
                //       );


                //var existing = await _reqApprovalSettingRepository.AllActive()
                //    .Include(a => a.ReqApprovalStepApprovers)
                //    .FirstOrDefaultAsync(a => a.ReqApprovalSettingID == viewModel.ApprovalSettingID);

                //if (existing == null)
                //{
                //    return Json(new { success = false, message = "Approval setting not found." });
                //}

                


                var alreadyExists = await _reqApprovalSettingRepository.AllActive()
                        .FirstOrDefaultAsync(a =>
                            a.ReqApprovalSettingID != viewModel.ApprovalSettingID
                            && a.ApprovalTypeID == viewModel.ApprovalTypeID
                            && (
                                // overlap condition
                                viewModel.StartDate <= a.EndDate && viewModel.EndDate >= a.StartDate
                            ));

                var existing = await _reqApprovalSettingRepository.AllActive()
                    .Include(a => a.ReqApprovalStepApprovers)
                    .FirstOrDefaultAsync(a => a.ReqApprovalSettingID == viewModel.ApprovalSettingID);

                if (existing == null)
                {
                    return Json(new { success = false, message = "Approval setting not found." });
                }

                if (alreadyExists != null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "An approval setting with the same Organization, Branch, and Approval Type already exists within the given date range."
                    });
                }


                existing.StartDate = viewModel.StartDate;
                existing.EndDate = viewModel.EndDate;
        
                existing.UpdatedBy = viewModel.CreatedBy; // Replace with actual user ID
                existing.UpdatedAt = DateTime.Now;
                existing.LIP = viewModel.LIP;
                existing.LMAC = viewModel.LMAC; // Replace with actual MAC address logic if needed



                //var beforeEntity = JsonConvert.DeserializeObject<ApprovalSettingViewModel>(JsonConvert.SerializeObject(existing, JsonSettings.IgnoreReferenceLoop));


                //var afterEntity = JsonConvert.DeserializeObject<ApprovalSettingViewModel>(JsonConvert.SerializeObject(existing, JsonSettings.IgnoreReferenceLoop));
                //await _userInfoService.ActionLogAsync("ReqApprovalSetting", ActionName.DataUpdated, beforeEntity, afterEntity, existing.ReqApprovalSettingID, viewModel);




                var assignmentsToDelete = existing.ReqApprovalStepApprovers.ToList(); // shallow copy


                await _reqApprovalStepApproversRepository.DeleteRangeAsync(assignmentsToDelete);

                //foreach (var item in assignmentsToDelete)
                //{
                //    await _reqApprovalStepApproversRepository.DeleteAsync(item.ReqApprovalStepApproverID);
                //}



                existing.ReqApprovalStepApprovers = viewModel.ApprovalLevels
                     .Select((al, index) => new ReqApprovalStepApprovers
                     {
                         ReqApprovalSettingID = viewModel.ApprovalSettingID,
                         Step = index + 1,
                         ApproverID = al.ApproverEmployeeID
                     })
                     .ToList();

                


                await _reqApprovalSettingRepository.UpdateAsync(existing);

                await _reqApprovalSettingRepository.CommitTransactionAsync();
                return Json(new { success = true, message = "Approval setting updated successfully." });
            }
            catch (Exception)
            {
                await _reqApprovalSettingRepository.RollbackTransactionAsync();
                return Json(new { success = false, message = "Approval setting updated unsuccessfully." });
           
            }


        }

        #endregion

        #region Delete

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, BaseViewModel? model)
        {

            await _reqApprovalSettingRepository.BeginTransactionAsync();

            try
            {
                var setting = await _reqApprovalSettingRepository.AllActive().Include(e => e.ReqApprovalStepApprovers).Include(e => e.ApprovalType)
                .FirstOrDefaultAsync(a => a.ReqApprovalSettingID == id);

                if (setting == null)
                {
                    return Json(new { success = false, message = "Approval setting not found." });
                }

                var beforeEntity = JsonConvert.DeserializeObject<ApprovalSettingViewModel>(JsonConvert.SerializeObject(setting, JsonSettings.IgnoreReferenceLoop));


               
                await _userInfoService.ActionLogAsync("ReqApprovalSetting", ActionName.DataDeleted, beforeEntity, null, setting.ReqApprovalSettingID, model);



                await _reqApprovalStepApproversRepository.DeleteRangeAsync(setting.ReqApprovalStepApprovers);
                await _reqApprovalSettingRepository.DeleteAsync(setting.ReqApprovalSettingID);


                await _reqApprovalSettingRepository.CommitTransactionAsync();

                //setting.DeletedAt = DateTime.Now;
                //setting.DeletedBy = model.CreatedBy ?? null; // Replace with actual user ID
                //setting.LIP = model.LIP;
                //setting.LMAC = model.LMAC;

                //await _reqApprovalSettingRepository.UpdateAsync(setting);


                return Json(new { success = true, message = "Approval setting deleted successfully." });
            }
            catch (Exception ex)
            {
                await _reqApprovalSettingRepository.RollbackTransactionAsync();
                return Json(new { success = false, message = "Failed to delete approval setting.", error = ex.Message });
            }

            
        }

        #endregion



    }
}
