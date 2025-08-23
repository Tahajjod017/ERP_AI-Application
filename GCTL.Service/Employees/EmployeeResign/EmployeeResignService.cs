using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.Employee.EmployeeResign;
using GCTL.Core.ViewModels.Employee.EmployeeStatusManagement.Promotion;
using GCTL.Core.ViewModels.Employee.Universal;
using GCTL.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.SqlServer.Server;

namespace GCTL.Service.Employees.EmployeeResign
{
    public class EmployeeResignService : IEmployeeResign
    {

        #region CTOR

        private readonly IGenericRepository<GCTL.Data.Models.Employees> _employeeRepository;
      
        private readonly IGenericRepository<Resignations> _resignationRepository;

        

        private readonly IGenericRepository<EmployeeActionTypes> _employeeActionTypeRepository;
        private readonly IGenericRepository<ResignationsApprovalHistory> _resignationsApprovalHistoryRepository;

        private readonly IGenericRepository<Departments> _deptRepository;
        private readonly IGenericRepository<Designations> _desigRepository;
        private readonly IGenericRepository<Statuses> _statusRepository;
        private readonly IGenericRepository<EmployeeOfficeInfo> _empOfficialRepository;
        private readonly IGenericRepository<ApprovalTypes> _approvalTypeRepository;
        private readonly IGenericRepository<ApprovalSettings> _approvalSettingRepository;
        private readonly IGenericRepository<ApprovalDesignation> _approvalDesignationRepository;
        private readonly IGenericRepository<Alerts> alertsRepository;
        private readonly IGenericRepository<AlertForEmployee> alertForEmployeeRepository;


        public EmployeeResignService(IGenericRepository<Data.Models.Employees> employeeRepository, IGenericRepository<Resignations> resignationRepository, IGenericRepository<EmployeeActionTypes> employeeActionTypeRepository, IGenericRepository<ResignationsApprovalHistory> resignationsApprovalHistoryRepository, IGenericRepository<Departments> deptRepository, IGenericRepository<Designations> desigRepository, IGenericRepository<Statuses> statusRepository, IGenericRepository<EmployeeOfficeInfo> empOfficialRepository, IGenericRepository<ApprovalTypes> approvalTypeRepository, IGenericRepository<ApprovalSettings> approvalSettingRepository, IGenericRepository<ApprovalDesignation> approvalDesignationRepository, IGenericRepository<Alerts> alertsRepository, IGenericRepository<AlertForEmployee> alertForEmployeeRepository)
        {
            _employeeRepository = employeeRepository;
            _resignationRepository = resignationRepository;
            _employeeActionTypeRepository = employeeActionTypeRepository;
            _resignationsApprovalHistoryRepository = resignationsApprovalHistoryRepository;
            _deptRepository = deptRepository;
            _desigRepository = desigRepository;
            _statusRepository = statusRepository;
            _empOfficialRepository = empOfficialRepository;
            _approvalTypeRepository = approvalTypeRepository;
            _approvalSettingRepository = approvalSettingRepository;
            _approvalDesignationRepository = approvalDesignationRepository;
            this.alertsRepository = alertsRepository;
            this.alertForEmployeeRepository = alertForEmployeeRepository;
        }




        #endregion

        #region Entry Page 




        #region Get All
      public object GetResignations(int page, int pageSize, string sortColumn, string sortDirection, string fromDate, string toDate, string imgSrcThumb)
  {
      //var resignations = new List<ResignationViewModel>(_resignations);
      var resignations = _resignationRepository.AllActive().Include(e=>e.Employee)
          .ThenInclude(r=>r.EmployeeOfficeInfoEmployee).ThenInclude(p=>p.Department).Select(e=> new ResignationViewModel()
          {
              ResigId = e.ResignationID,
              REmpDept = e.Employee.EmployeeOfficeInfoEmployee.FirstOrDefault().Department.DepartmentName,
              REmpName = e.Employee.FirstName + " " + e.Employee.LastName,
              ResignResons = e.Reason,
              ResinDate = e.ResignationDate.Value.ToString("dd/MM/yyyy"),
              ResNoticeDate = e.NoticeDate.Value.ToString("dd/MM/yyyy"),
              EmployeeId = e.EmployeeID,

              CompanyId = e.Employee.EmployeeOfficeInfoEmployee.FirstOrDefault().OrganizationID,
              Image = imgSrcThumb + e.Employee.EmployeeImageFileName

          }).ToList();



      // Apply date range filter
      if (!string.IsNullOrEmpty(fromDate) && !string.IsNullOrEmpty(toDate))
      {
          DateTime from = DateTime.ParseExact(fromDate, "dd/MM/yyyy", null);
          DateTime to = DateTime.ParseExact(toDate, "dd/MM/yyyy", null);
          resignations = resignations
              .Where(r => {
                  DateTime noticeDate = DateTime.ParseExact(r.ResinDate, "dd/MM/yyyy", null);
                  return noticeDate >= from && noticeDate <= to;
              })
              .ToList();
      }

      // Apply sorting
      resignations = sortColumn switch
      {
          "rEmpName" => sortDirection == "asc" ? resignations.OrderBy(r => r.REmpName).ToList() : resignations.OrderByDescending(r => r.REmpName).ToList(),
          "rEmpDept" => sortDirection == "asc" ? resignations.OrderBy(r => r.REmpDept).ToList() : resignations.OrderByDescending(r => r.REmpDept).ToList(),
          "resignResons" => sortDirection == "asc" ? resignations.OrderBy(r => r.ResignResons).ToList() : resignations.OrderByDescending(r => r.ResignResons).ToList(),
          "resNoticeDate" => sortDirection == "asc" ? resignations.OrderBy(r => r.ResNoticeDate).ToList() : resignations.OrderByDescending(r => r.ResNoticeDate).ToList(),
          "resinDate" => sortDirection == "asc" ? resignations.OrderBy(r => r.ResinDate).ToList() : resignations.OrderByDescending(r => r.ResinDate).ToList(),
          _ => resignations
      };

      // Apply pagination
      var totalRecords = resignations.Count;
      var pagedData = resignations.Skip((page - 1) * pageSize).Take(pageSize).ToList();




      return new
      {
          data = pagedData,
          recordsTotal = totalRecords,
          recordsFiltered = totalRecords
      };
  }

        #endregion

        #region Create 

        public async Task<CommonReturnViewModel> InsertResignation(ResignationPostViewModel model)
        {
            #region validato

            var result = new CommonReturnViewModel()
            {
                Success = false,

            };

            if (!DateTime.TryParseExact(model.NoticeDate, "dd/MM/yyyy", CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out DateTime noticeDate))
            {
                result.Message = "Invalid Notice Date format. Expected format is dd/MM/yyyy.";
                return result;
            }

            if (!DateTime.TryParseExact(model.ResignationDate, "dd/MM/yyyy", CultureInfo.InvariantCulture,
                                        DateTimeStyles.None, out DateTime resignationDate))
            {
                result.Message = "Invalid Resignation Date format. Expected format is dd/MM/yyyy.";
                return result;
            }


            var emp = _employeeRepository.AllActive().FirstOrDefault(e => e.EmployeeID == model.EmployeeId);

            if (emp == null)
            {
                result.Message = "Employee is invalid";
                return result;
            }

            #endregion


            try
            {
                bool AllowSecondApproval;
                bool AllowThirdApproval;
                bool AllowSelfApproval;
                int firstApproverId = 0;
                int secondApproverId = 0;
                int thirdApproverId = 0;
                int selfApprovalExceptionId = 0;

                #region Action Type and Status Setup



                var status = await _statusRepository.AllActive()
                    .FirstOrDefaultAsync(s => s.StatusName.ToLower() == "pending");
                if (status == null)
                {
                    status = new Statuses
                    {
                        StatusName = "Pending",
                        StatusType = "AppDecPen",
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = model.CreatedBy,
                        LIP = model.LIP,
                        LMAC = model.LMAC
                    };
                    await _statusRepository.AddAsync(status);
                }
                #endregion

                #region Employee Validation

                //var test = await _empOfficialRepository.AllActive().ToListAsync();

                //var employee = test.FirstOrDefault(e => e.EmployeeID == model.EmployeeId);

                var employee = await _empOfficialRepository.AllActive().Where(e => e.EmployeeID == model.EmployeeId).FirstOrDefaultAsync();

                var employeeInfo = await _employeeRepository.AllActive().Where(e => e.EmployeeID == model.EmployeeId).FirstOrDefaultAsync();


                if (employee == null || employeeInfo == null)
                {
                    return new CommonReturnViewModel { Success = false, Message = "Employee not found" };
                }
                #endregion

                #region Approval Person Determination

                var approvalType = await _approvalTypeRepository.AllActive()
                    .FirstOrDefaultAsync(a => a.ApprovalTypeName.Trim().ToLower() == "resignation approval");
                if (approvalType == null)
                {
                    approvalType = new ApprovalTypes()
                    {
                        ApprovalTypeName = "Resignation Approval",
                        //OrganizationID = employee.OrganizationID,
                        //OrganizationBranchID = employee.OrganizationBranchID
                    };

                    await _approvalTypeRepository.AddAsync(approvalType, model);

                    
                }

                var approvalSettings = await _approvalSettingRepository.AllActive()
                    .FirstOrDefaultAsync(a => a.ApprovalTypeID == approvalType.ApprovalTypeID
                        && a.OrganizationID == employee.OrganizationID
                        && a.OrganizationBranchID == employee.OrganizationBranchID);
                if (approvalSettings == null)
                {
                    return new CommonReturnViewModel { Success = false, Message = "Approval settings not found" };
                }

                int initialApproverId = 0;
                int initialStage = 1;

                // Fill boolean fields from approval settings
                AllowSecondApproval = approvalSettings.IsEnableSecondApproval;
                AllowThirdApproval = approvalSettings.IsEnableThirdApproval;
                AllowSelfApproval = approvalSettings.AllowSelfApproval ?? true;
                selfApprovalExceptionId = approvalSettings.SelfExceptionApprovalID ?? 0;

                bool isSelfApplication = model.CreatedBy == model.EmployeeId; // Check if applying for self

                // Determine first approver
                if (approvalSettings.IsDesignationOrEmpFirstApprovalID)
                {
                    var approvalDesig = await _approvalDesignationRepository.AllActive()
                        .FirstOrDefaultAsync(e => e.ApprovalDesignationID == approvalSettings.FirstApprovalID);
                    firstApproverId = approvalDesig?.Code switch
                    {
                        1 => employee.ImmediateSupervisorId ?? 0,
                        2 => employee.SeniorSupervisorId ?? 0,
                        3 => employee.HeadOfDepartmentId ?? 0,
                        _ => 0
                    };
                }
                else
                {
                    firstApproverId = approvalSettings.FirstApprovalID ?? 0;
                }

                // Determine second approver if enabled
                if (AllowSecondApproval)
                {
                    if (approvalSettings.IsDesignationOrEmpSecondApprovalID)
                    {
                        var approvalDesig = await _approvalDesignationRepository.AllActive()
                            .FirstOrDefaultAsync(e => e.ApprovalDesignationID == approvalSettings.SecondApprovalID);
                        secondApproverId = approvalDesig?.Code switch
                        {
                            1 => employee.ImmediateSupervisorId ?? 0,
                            2 => employee.SeniorSupervisorId ?? 0,
                            3 => employee.HeadOfDepartmentId ?? 0,
                            _ => 0
                        };
                    }
                    else
                    {
                        secondApproverId = approvalSettings.SecondApprovalID ?? 0;
                    }
                }

                // Determine third approver if enabled
                if (AllowThirdApproval)
                {
                    if (approvalSettings.IsDesignationOrEmpThirdApprovalID)
                    {
                        var approvalDesig = await _approvalDesignationRepository.AllActive()
                            .FirstOrDefaultAsync(e => e.ApprovalDesignationID == approvalSettings.ThirdApprovalID);
                        thirdApproverId = approvalDesig?.Code switch
                        {
                            1 => employee.ImmediateSupervisorId ?? 0,
                            2 => employee.SeniorSupervisorId ?? 0,
                            3 => employee.HeadOfDepartmentId ?? 0,
                            _ => 0
                        };
                    }
                    else
                    {
                        thirdApproverId = approvalSettings.ThirdApprovalID ?? 0;
                    }
                }

                // Set initial approver based on self-approval logic
                if (isSelfApplication && AllowSelfApproval)
                {
                    // If self-approval is allowed and it's a self-application
                    if (selfApprovalExceptionId > 0)
                    {
                        // Use self-approval exception person
                        initialApproverId = selfApprovalExceptionId;
                    }
                    else
                    {
                        // Use first approver
                        initialApproverId = firstApproverId;
                    }
                }
                else
                {
                    // Normal flow - use first approver
                    initialApproverId = firstApproverId;
                }

                if (initialApproverId == 0)
                {
                    return new CommonReturnViewModel { Success = false, Message = "No valid first approver found" };
                }
                #endregion

                #region Save Method

                var toCreate = new Resignations
                {
                    //OrganizationID = model.CompanyId,
                    EmployeeID = model.EmployeeId,
                    ResignationDate = resignationDate,
                    NoticeDate = noticeDate,
                    Reason = model.Reason,

                    StatusID = status.StatusID,
                    ApprovalPersonID = initialApproverId,
                    ApprovalStep = initialStage,

                };
                await _resignationRepository.AddAsync(toCreate, model);

                #endregion


                #region Auto Approve By employee

                if (model.EmployeeId == firstApproverId)
                {
                    var a = AddAutoApprovedHistoryAsync(toCreate, model, firstApproverId, 1);

                    if (AllowSecondApproval)
                    {
                        toCreate.ApprovalPersonID = secondApproverId;
                        toCreate.ApprovalStep = 2;
                    }
                    else if (!AllowSecondApproval && !AllowSelfApproval)
                    {
                        toCreate.ApprovalPersonID = selfApprovalExceptionId;
                        toCreate.ApprovalStep = 2;
                    }
                    else
                    {
                        toCreate.IsFinalApproved = true;
                    }

                    await _resignationRepository.UpdateAsync(toCreate, model);


                }
                else if (model.EmployeeId == secondApproverId)
                {
                    var a = AddAutoApprovedHistoryAsync(toCreate, model, firstApproverId, 1);
                    var b = AddAutoApprovedHistoryAsync(toCreate, model, secondApproverId, 2);

                    if (AllowThirdApproval)
                    {
                        toCreate.ApprovalPersonID = thirdApproverId;
                        toCreate.ApprovalStep = 3;
                    }
                    else if (!AllowThirdApproval && !AllowSelfApproval)
                    {
                        toCreate.ApprovalPersonID = selfApprovalExceptionId;
                        toCreate.ApprovalStep = 3;
                    }
                    else
                    {
                        toCreate.IsFinalApproved = true;
                    }
                    await _resignationRepository.UpdateAsync(toCreate, model);
                }
                else if (model.EmployeeId == thirdApproverId)
                {
                    var a = AddAutoApprovedHistoryAsync(toCreate, model, firstApproverId, 1);
                    var b = AddAutoApprovedHistoryAsync(toCreate, model, secondApproverId, 2);
                    var c = AddAutoApprovedHistoryAsync(toCreate, model, thirdApproverId, 3);

                    if (!AllowSelfApproval)
                    {
                        toCreate.ApprovalPersonID = selfApprovalExceptionId;
                        toCreate.ApprovalStep = 4;
                    }
                    else
                    {
                        toCreate.IsFinalApproved = true;
                    }
                    await _resignationRepository.UpdateAsync(toCreate, model);
                }


                #endregion


                #region Alert


                var alert = new Alerts
                {
                    AlertTitle = "Employee Resign",
                    AlertNote = $"{employeeInfo.FirstName} {employeeInfo.LastName} has requested for Resign.",
                    LMAC = model.LMAC,
                    LIP = model.LIP,
                    CreatedBy = model.CreatedBy,
                    CreatedAt = DateTime.Now,
                };

                await alertsRepository.AddAsync(alert);
                var empAlert = new AlertForEmployee
                {
                    AlertID = alert.AlertID,
                    EmployeeID = toCreate.ApprovalPersonID,  // for alert Employee
                    IsChecked = false,
                    LIP = model.LIP,
                    LMAC = model.LMAC,
                    CreatedAt = DateTime.Now,
                    CreatedBy = model.CreatedBy,
                };
                await alertForEmployeeRepository.AddAsync(empAlert);

                #endregion



                result.Success = true;
                result.Message = "Saved successfull";
                return result;

            }
            catch (Exception ex)
            {
                {
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = ex.Message
                    };
                }
            }
        }

        #endregion


        #region Update And 

        public CommonReturnViewModel UpdateResignation(int resignationId, ResignationPostViewModel model)
        {
            try
            {
                var result = new CommonReturnViewModel
                {
                    Success = false
                };

                // Validate Notice Date
                if (!DateTime.TryParseExact(model.NoticeDate, "dd/MM/yyyy", CultureInfo.InvariantCulture,
                                            DateTimeStyles.None, out DateTime noticeDate))
                {
                    result.Message = "Invalid Notice Date format. Expected format is dd/MM/yyyy.";
                    return result;
                }

                // Validate Resignation Date
                if (!DateTime.TryParseExact(model.ResignationDate, "dd/MM/yyyy", CultureInfo.InvariantCulture,
                                            DateTimeStyles.None, out DateTime resignationDate))
                {
                    result.Message = "Invalid Resignation Date format. Expected format is dd/MM/yyyy.";
                    return result;
                }

                // Check if resignation exists
                var resignation = _resignationRepository.AllActive()
                    .FirstOrDefault(r => r.ResignationID == resignationId);

                if (resignation == null)
                {
                    result.Message = "Resignation record not found.";
                    return result;
                }

                // Validate Employee
                var emp = _employeeRepository.AllActive()
                    .FirstOrDefault(e => e.EmployeeID == model.EmployeeId);

                if (emp == null)
                {
                    result.Message = "Employee is invalid.";
                    return result;
                }

                // Update fields
               // resignation.OrganizationID = model.CompanyId;
                resignation.EmployeeID = model.EmployeeId;
                resignation.ResignationDate = resignationDate;
                resignation.NoticeDate = noticeDate;
                resignation.Reason = model.Reason;

                _resignationRepository.UpdateAsync(resignation, model);

                result.Success = true;
                result.Message = "Updated successfully.";
                return result;
            }
            catch (Exception ex)
            {
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        #endregion

        #region Delter And Get by id

        public CommonReturnViewModel DeleteResignation(DeleteRequestVM delete)
        {
            try
            {
                var result = new CommonReturnViewModel
                {
                    Success = false
                };

                foreach (var item in delete.Ids)
                {
                    var resignation = _resignationRepository.AllActive()
                        .FirstOrDefault(r => r.ResignationID == item);

                    if (resignation == null)
                    {
                        result.Message = "Resignation record not found.";
                        return result;
                    }

                    var apperoveCheck = _resignationsApprovalHistoryRepository.AllActive().Where(e => e.ResignationID == item).ToList();
                    if (apperoveCheck.Any())
                    {
                        result.Message = "Can not Delete !! Resign is Already is in Approval matrix";
                        return result;
                    }


                    resignation.DeletedAt = DateTime.UtcNow;
                    resignation.DeletedBy = delete.DeletedBy;

                    _resignationRepository.UpdateAsync(resignation, delete);

                    result.Success = true;
                    result.Message = "Deleted successfully.";
                   
                }

                return result;



            }
            catch (Exception ex)
            {
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }


        public CommonReturnViewModel GetResignationById(int resignationId)
        {
            try
            {
                var apperoveCheck = _resignationsApprovalHistoryRepository.AllActive().Where(e => e.ResignationID == resignationId).ToList();

                var resign = _resignationRepository.AllActive().Join(
                                _empOfficialRepository.AllActive(),
                                resignation => resignation.EmployeeID,
                                office => office.EmployeeID,
                                (resignation, office) => new { resignation, office }
                            )

                .Where(e => e.resignation.ResignationID == resignationId).Select(r => new ResignationViewModel
                {
                    ResigId = r.resignation.ResignationID,
                    ResignResons = r.resignation.Reason,
                    ResinDate = r.resignation.ResignationDate.Value.ToString("dd/MM/yyyy"),
                    ResNoticeDate = r.resignation.NoticeDate.Value.ToString("dd/MM/yyyy"),
                    EmployeeId = r.resignation.EmployeeID,
                    CompanyId = r.office.OrganizationID,

                }).FirstOrDefault();

                if (resign == null)
                {
                    return new CommonReturnViewModel { Success = false, Message = "No resignation found", };
                }

                if (apperoveCheck.Any())
                {
                    return new CommonReturnViewModel { Success = false, Message = "Can not edit data is Already is in Approval matrix", Data = resign };
                }

                return new CommonReturnViewModel { Success = true, Message = "Resignation loaded successfully", Data = resign };
            }
            catch (Exception ex)
            {

                return new CommonReturnViewModel { Success = false, Message = ex.Message, };

            }



        }


        #endregion


        #endregion


        #region Helper

        private async Task AddAutoApprovedHistoryAsync(Resignations toCreate, ResignationPostViewModel model, int firstApproverId, int stage)
        {
            try
            {
                var approvedStatus = await GetOrCreateStatusAsync("approve", new PromotionActionModel
                {
                    CreatedBy = model.CreatedBy,
                    LIP = model.LIP,
                    LMAC = model.LMAC
                });

                string comment = "Auto Approved (Stage " + stage + " )";
                var history = new ResignationsApprovalHistory
                {
                    ResignationID = toCreate.ResignationID,
                    EmployeeID = model.EmployeeId,
                    StatusID = approvedStatus.StatusID,
                    ApprovalPersonID = firstApproverId,
                    ApprovalPersonNote = comment,
                    LIP = model.LIP,
                    LMAC = model.LMAC,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = model.CreatedBy
                };

                await _resignationsApprovalHistoryRepository.AddAsync(history);
            }
            catch (Exception)
            {

                throw;
            }


        }

        private async Task<Statuses> GetOrCreateStatusAsync(string action, PromotionActionModel actionModel)
        {
            string statusName = action == "approve" ? "Approved" : "Declined";
            var status = await _statusRepository.AllActive()
                .FirstOrDefaultAsync(s => s.StatusName.ToLower() == statusName.ToLower());

            if (status == null)
            {
                status = new Statuses
                {
                    StatusName = statusName,
                    StatusType = "AppDecPen",
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = actionModel.CreatedBy,
                    LIP = actionModel.LIP,
                    LMAC = actionModel.LMAC
                };
                await _statusRepository.AddAsync(status);
            }

            return status;
        }

        private async Task<int> ResolveNextApproverAsync(Resignations career)
        {
            if (career.EmployeeID == null || career.ApprovalStep == null)
                return 0;

            var employee = await _empOfficialRepository.AllActive()
                .FirstOrDefaultAsync(e => e.EmployeeID == career.EmployeeID);
            if (employee == null)
                return 0;

            var approvalType = await _approvalTypeRepository.AllActive()
                .FirstOrDefaultAsync(a => a.ApprovalTypeName.ToLower() == "promotion approval");
            if (approvalType == null)
                return 0;

            var approvalSettings = await _approvalSettingRepository.AllActive()
                .FirstOrDefaultAsync(a => a.ApprovalTypeID == approvalType.ApprovalTypeID
                    && a.OrganizationID == employee.OrganizationID
                    && a.OrganizationBranchID == employee.OrganizationBranchID);
            if (approvalSettings == null)
                return 0;

            int currentStage = career.ApprovalStep.Value;
            if (currentStage == 1 && approvalSettings.IsEnableSecondApproval)
            {
                return await ResolveApproverAsync(approvalSettings, employee, 2);
            }
            else if (currentStage == 2 && approvalSettings.IsEnableThirdApproval)
            {
                return await ResolveApproverAsync(approvalSettings, employee, 3);
            }

            return 0; // No further approvers needed
        }

        private async Task<int> ResolveApproverAsync(ApprovalSettings settings, EmployeeOfficeInfo employee, int stage)
        {
            bool isDesignationBased;
            int? approverID;
            bool allowSelfApproval = settings.AllowSelfApproval ?? false;
            int? selfExceptionID = settings.SelfExceptionApprovalID;

            if (stage == 2)
            {
                if (!settings.IsEnableSecondApproval) return 0;
                isDesignationBased = settings.IsDesignationOrEmpSecondApprovalID;
                approverID = settings.SecondApprovalID;
            }
            else if (stage == 3)
            {
                if (!settings.IsEnableThirdApproval) return 0;
                isDesignationBased = settings.IsDesignationOrEmpThirdApprovalID;
                approverID = settings.ThirdApprovalID;
            }
            else
            {
                return 0; // Unsupported stage
            }

            if (!approverID.HasValue)
                return 0;

            if (isDesignationBased)
            {
                var approvalDesig = await _approvalDesignationRepository.AllActive()
                    .FirstOrDefaultAsync(e => e.ApprovalDesignationID == approverID);
                return approvalDesig?.Code switch
                {
                    1 => employee.ImmediateSupervisorId ?? 0,
                    2 => employee.SeniorSupervisorId ?? 0,
                    3 => employee.HeadOfDepartmentId ?? 0,
                    _ => 0
                };
            }

            if (approverID == employee.EmployeeID && allowSelfApproval)
                return employee.EmployeeID ?? 0;

            return selfExceptionID ?? approverID ?? 0;
        }

        private async Task LogCareerChangeHistoryAsync(Resignations career, BaseViewModel action, string hrComments, bool isAuto = false)
        {
            var history = new ResignationsApprovalHistory
            {
                ResignationID = career.ResignationID,
                EmployeeID = career.EmployeeID,
                StatusID = career.StatusID,
                ApprovalPersonID = isAuto ? career.ApprovalPersonID : action.CreatedBy,
                // ApprovalPersonID = career.ApprovalPersonID,
                //ApprovalPersonID = action.CreatedBy,
                ApprovalPersonNote = hrComments,
                LIP = action.LIP,
                LMAC = action.LMAC,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = action.CreatedBy
            };
            await _resignationsApprovalHistoryRepository.AddAsync(history);
        }

        #endregion


        #region Approve Page

        #region Pendeing

        public async Task<List<ResignationGetViewModel>> GetPendingResignations(string dateRange, string department, string designation, string imgSrcThumb, int? currentUser)
        {
            try
            {
                var query = _resignationRepository.AllActive()
                .Include(r => r.Employee)
                    .ThenInclude(e => e.EmployeeOfficeInfoEmployee)
                        .ThenInclude(office => office.Designation)
                .Include(r => r.Employee)
                    .ThenInclude(e => e.EmployeeOfficeInfoEmployee)
                        .ThenInclude(office => office.Department)
                .Include(r => r.Status)
                .Where(r => r.ApprovalPersonID == currentUser && 

                              ((r.IsFinalApproved == false && r.IsDecline == null) ||
                              (r.IsFinalApproved == r.IsDecline && r.IsDecline != true)  ||
                              (r.IsDecline == false && r.IsFinalApproved == null) ||
                              (r.IsFinalApproved == null && r.IsDecline == null) )
                               
                );

                var test = query.ToList();

                if (!string.IsNullOrEmpty(dateRange))
                {
                    var dates = dateRange.Split(" to ");
                    if (dates.Length == 2 &&
                        DateTime.TryParseExact(dates[0], "d/M/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var fromDate) &&
                        DateTime.TryParseExact(dates[1], "d/M/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var toDate))
                    {
                        query = query.Where(r => r.NoticeDate >= fromDate && r.NoticeDate <= toDate);
                    }
                }

                // Apply department filter
                if (!string.IsNullOrEmpty(department))
                {
                    query = query.Where(r => r.Employee.EmployeeOfficeInfoEmployee.Any(o => o.DepartmentID == int.Parse(department)));
                }

                // Apply designation filter
                if (!string.IsNullOrEmpty(designation))
                {
                    query = query.Where(r => r.Employee.EmployeeOfficeInfoEmployee.Any(o => o.DesignationID == int.Parse(designation)));
                }

                var test1 = query.ToList();

                var resignations = await query.Select(r => new ResignationGetViewModel
                {
                    Id = r.ResignationID,
                    EmployeeName = r.Employee.FirstName + " " + r.Employee.LastName,
                    EmployeeCode = r.Employee.EmployeeCode,
                    Department = r.Employee.EmployeeOfficeInfoEmployee.FirstOrDefault().Department.DepartmentName,
                    Position = r.Employee.EmployeeOfficeInfoEmployee.FirstOrDefault().Designation.DesignationName,
                    Reason = r.Reason,
                    NoticeDate = r.NoticeDate.Value.ToString("dd/MM/yyyy"),
                    LastWorkingDay = r.ResignationDate.Value.ToString("dd/MM/yyyy"),
                    Status = r.Status.StatusName ?? "Pending",
                    ProfileImage = r.Employee.EmployeeImageFileName != null ? imgSrcThumb + r.Employee.EmployeeImageFileName : "https://placehold.co/400",
                    EmployeeId = r.EmployeeID.ToString(),
                    YearsOfService = CalculateYearsOfService(r.Employee.EmployeeOfficeInfoEmployee.FirstOrDefault().JoiningDate, r.ResignationDate.Value),
                    NoticePeriod = (r.ResignationDate.Value - r.NoticeDate.Value).Days.ToString() + " Days",
                    //CurrentSalary = r.Employee.EmployeeOfficeInfoEmployee.FirstOrDefault().ToString("C", CultureInfo.GetCultureInfo("bn-BD")) ?? "৳0",
                    PendingDues = "৳0", // Placeholder, adjust based on actual logic
                                        //HandoverStatus = r.HandoverStatus ?? "pending",
                                        //AssetReturned = r.AssetReturned ?? false,
                                        //ClearanceCompleted = r.ClearanceCompleted ?? false,
                                        //DocumentsPrepared = r.DocumentsPrepared ?? false
                }).ToListAsync();

                return resignations;
            }
            catch (Exception)
            {

                throw;
            }
            
        }

        #endregion

        #region Approved 

        public async Task<List<ResignationGetViewModel>> GetProcessedResignations(string dateRange, string department, string designation, string imgSrcThumb, int? currentUser)
        {
            var query = _resignationsApprovalHistoryRepository.AllActive()

                //_resignationRepository.AllActive()
                    .Include(i => i.Resignation)
                    .Include(r => r.Employee)
                        .ThenInclude(e => e.EmployeeOfficeInfoEmployee)
                            .ThenInclude(office => office.Designation)
                    .Include(r => r.Employee)
                        .ThenInclude(e => e.EmployeeOfficeInfoEmployee)
                            .ThenInclude(office => office.Department)
                    .Include(r=>r.Status)
                    
                     .Where(r => r.ApprovalPersonID == currentUser &&

                                ((r.Resignation.IsFinalApproved == true && r.Resignation.IsDecline == null) ||
                              (r.Resignation.IsFinalApproved == r.Resignation.IsDecline && r.Resignation.IsDecline != false  && r.Resignation.IsDecline != null) ||
                              (r.Resignation.IsDecline == true && r.Resignation.IsFinalApproved == null) ||
                              (r.Resignation.IsFinalApproved != null && r.Resignation.IsDecline != null) )
                              

                     );

            

            // Apply date range filter
            if (!string.IsNullOrEmpty(dateRange))
            {
                var dates = dateRange.Split(" to ");
                if (dates.Length == 2 &&
                    DateTime.TryParseExact(dates[0], "d/M/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var fromDate) &&
                    DateTime.TryParseExact(dates[1], "d/M/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var toDate))
                {
                    query = query.Where(r => r.Resignation.ResignationDate >= fromDate && r.Resignation.ResignationDate <= toDate);
                }
            }

            // Apply department filter
            if (!string.IsNullOrEmpty(department))
            {
                query = query.Where(r => r.Employee.EmployeeOfficeInfoEmployee.Any(o => o.DepartmentID == int.Parse(department)));
            }

            // Apply designation filter
            if (!string.IsNullOrEmpty(designation))
            {
                query = query.Where(r => r.Employee.EmployeeOfficeInfoEmployee.Any(o => o.DesignationID == int.Parse(designation)));
            }

            var resignations = await query.Select(r => new ResignationGetViewModel
            {
                Id = r.Resignation.ResignationID,
                EmployeeName = r.Employee.FirstName + " " + r.Employee.LastName,
                EmployeeCode = r.Employee.EmployeeCode,
                Department = r.Resignation.Employee.EmployeeOfficeInfoEmployee.FirstOrDefault().Department.DepartmentName,
                Position = r.Resignation.Employee.EmployeeOfficeInfoEmployee.FirstOrDefault().Designation.DesignationName,
                Reason = r.Resignation.Reason,
                NoticeDate = r.Resignation.NoticeDate.Value.ToString("dd/MM/yyyy"),
                LastWorkingDay = r.Resignation.ResignationDate.Value.ToString("dd/MM/yyyy"),
                ProcessedDate = r.Resignation.NoticeDate.Value.ToString("dd/MM/yyyy") ?? "",
                Status = r.Status.StatusName  ?? "Approved",
                ProfileImage = r.Employee.EmployeeImageFileName != null ? imgSrcThumb + r.Employee.EmployeeImageFileName : "https://placehold.co/400",
                EmployeeId = r.EmployeeID.ToString(),
                YearsOfService = CalculateYearsOfService(r.Employee.EmployeeOfficeInfoEmployee.FirstOrDefault().JoiningDate, r.Resignation.ResignationDate.Value),
                NoticePeriod = (r.Resignation.ResignationDate.Value - r.Resignation.NoticeDate.Value).Days.ToString() + " Days",
                CurrentSalary = r.Resignation.Employee.EmployeeSalarySettingsEmployee.FirstOrDefault().Salary.ToString() ?? "৳0",
                PendingDues = "৳0", // Placeholder, adjust based on actual logic
                //HandoverStatus = r.HandoverStatus ?? "pending",
                //AssetReturned = r.AssetReturned ?? false,
                //ClearanceCompleted = r.ClearanceCompleted ?? false,
                //DocumentsPrepared = r.DocumentsPrepared ?? false
            }).ToListAsync();

            return resignations;
        }

        #endregion


        #region Get by id and Action 
        public async Task<ResignationGetViewModel> GetAppResignationById(int id)
        {
            var resignation = await _resignationRepository.AllActive()
                .Include(r => r.Employee)
                .ThenInclude(e => e.EmployeeOfficeInfoEmployee)
                    .ThenInclude(office => office.Designation)
            .Include(r => r.Employee)
                .ThenInclude(e => e.EmployeeOfficeInfoEmployee)
                    .ThenInclude(office => office.Department)
                .FirstOrDefaultAsync(r => r.ResignationID == id);

            

            if (resignation == null)
            {
                return null;
            }

            return new ResignationGetViewModel
            {
                Id = resignation.ResignationID,
                EmployeeName = resignation.Employee.FirstName + " " + resignation.Employee.LastName + " (" + resignation.Employee.EmployeeCode + ")",
                Department = resignation.Employee.EmployeeOfficeInfoEmployee.FirstOrDefault()?.Department.DepartmentName ?? "Unknown",
                Position = resignation.Employee.EmployeeOfficeInfoEmployee.FirstOrDefault()?.Designation.DesignationName ?? "Unknown",
                Reason = resignation.Reason,
                NoticeDate = resignation.NoticeDate.Value.ToString("dd/MM/yyyy"),
                LastWorkingDay = resignation.ResignationDate.Value.ToString("dd/MM/yyyy"),
                //ProcessedDate = resignation.ProcessedDate?.ToString("dd/MM/yyyy") ?? "",
                Status = _statusRepository.AllActive().FirstOrDefault(s => s.StatusID == resignation.StatusID)?.StatusName ?? ((bool)resignation.IsFinalApproved ? "Approved" : "Pending"),
                ProfileImage = resignation.Employee.EmployeeImageFileName != null ? "/images/employees/" + resignation.Employee.EmployeeImageFileName : "https://placehold.co/400",
                EmployeeId = resignation.EmployeeID.ToString(),
                YearsOfService = CalculateYearsOfService(resignation.Employee.EmployeeOfficeInfoEmployee.FirstOrDefault().JoiningDate, resignation.ResignationDate.Value),
                NoticePeriod = (resignation.ResignationDate.Value - resignation.NoticeDate.Value).Days.ToString() + " Days",
                CurrentSalary = resignation.Employee.EmployeeSalarySettingsEmployee.FirstOrDefault()?.Salary.ToString() ?? "৳0",
                PendingDues = "৳0", // Placeholder, adjust based on actual logic
                //HandoverStatus = resignation.HandoverStatus ?? "pending",
                //AssetReturned = resignation.AssetReturned ?? false,
                //ClearanceCompleted = resignation.ClearanceCompleted ?? false,
                //DocumentsPrepared = resignation.DocumentsPrepared ?? false
            };
        }

        public async Task<CommonReturnViewModel> ProcessResignation(int resignationId, string action, string hrComments, string handoverStatus, bool assetReturned, bool clearanceCompleted, bool documentsPrepared, CommonBaseViewModel? bm)
        {
            try
            {
                if (action == null || resignationId <= 0 || string.IsNullOrEmpty(action))
                {
                    return new CommonReturnViewModel { Success = false, Message = "Invalid action model" };
                }

                var resignation = await _resignationRepository.AllActive().FirstOrDefaultAsync(r => r.ResignationID == resignationId);
                if (resignation == null)
                {
                    
                    return new CommonReturnViewModel { Success = false, Message = "Resignation not found" };

                }

                var employee = await _employeeRepository.AllActive().FirstOrDefaultAsync(e => e.EmployeeID == resignation.EmployeeID);
                if (employee == null)
                {
                    return new CommonReturnViewModel { Success = false, Message = "Employee not found" };

                   
                }

                if (resignation.IsFinalApproved == true)
                {
                    return new CommonReturnViewModel { Success = false, Message = "Increment already approved" };
                }

                if (resignation.IsDecline == true)
                {
                    return new CommonReturnViewModel { Success = false, Message = "Increment has been declined" };
                }

                if (resignation.ApprovalPersonID != bm.CreatedBy)
                {
                    return new CommonReturnViewModel { Success = false, Message = "You are not authorized to approve this increment" };
                }

                var status = await GetOrCreateStatusAsync(action.ToLower(), new PromotionActionModel
                {
                    CreatedBy = bm.CreatedBy,
                    LIP = bm.LIP,
                    LMAC = bm.LMAC
                }); 


                if (status == null)
                {
                    return new CommonReturnViewModel { Success = false, Message = "Failed to retrieve or create status" };
                }

                resignation.StatusID = status.StatusID;
                bool isDecline = action.ToLower() == "decline";
                resignation.IsDecline = isDecline;

                if (!isDecline)
                {
                    resignation.ApprovalStep = (resignation.ApprovalStep ?? 0);
                    int nextApproverID = await ResolveNextApproverAsync(resignation);
                    if (nextApproverID != 0)
                    {
                        resignation.ApprovalPersonID = nextApproverID;
                        resignation.IsFinalApproved = false;
                        resignation.ApprovalStep = resignation.ApprovalStep + 1;
                    }
                    else
                    {


                        resignation.IsFinalApproved = true;
                        resignation.ApprovalStep = resignation.ApprovalStep + 1;



                    }
                }
                else
                {
                        resignation.IsFinalApproved = false;
                    resignation.ApprovalStep = resignation.ApprovalStep ?? 1;
                }

                resignation.UpdatedAt = DateTime.UtcNow;
                resignation.UpdatedBy = bm.UpdatedBy;
                resignation.LIP = bm.LIP;
                resignation.LMAC = bm.LMAC;

                await _resignationRepository.UpdateAsync(resignation);
                await LogCareerChangeHistoryAsync(resignation, bm, hrComments);


                #region Alert
                if (resignation.IsFinalApproved != true && resignation.IsDecline != true)
                {
                    var alert = new Alerts
                    {
                        AlertTitle = "Employee Increment",
                        AlertNote = $"{employee.FirstName} {employee.LastName} has requested an increment.",
                        LMAC = bm.LMAC,
                        LIP = bm.LIP,
                        CreatedBy = bm.CreatedBy,
                        CreatedAt = DateTime.Now,
                    };

                    await alertsRepository.AddAsync(alert);
                    var empAlert = new AlertForEmployee
                    {
                        AlertID = alert.AlertID,
                        EmployeeID = resignation.ApprovalPersonID,  // for alert Employee
                        IsChecked = false,
                        LIP = bm.LIP,
                        LMAC = bm.LMAC,
                        CreatedAt = DateTime.Now,
                        CreatedBy = bm.CreatedBy,
                    };
                    await alertForEmployeeRepository.AddAsync(empAlert);
                }

                #endregion



                

                return new CommonReturnViewModel { Success = true, Message = "Resign action completed successfully" };
            }
            catch (DbUpdateException ex)
            {
                return new CommonReturnViewModel { Success = false, Message = "Database error occurred while processing the increment action" };
            }
            catch (Exception ex)
            {
                return new CommonReturnViewModel { Success = false, Message = "An unexpected error occurred while processing the increment action: " + ex.Message };
            }
        }

        #endregion

        #endregion

        #region Helper Calcute Year
        public static string CalculateYearsOfService(DateOnly? joiningDate, DateTime resignationDate)
        {
            try
            {
                if (!joiningDate.HasValue)
                    return "0 Years";

                var joinDateTime = joiningDate.Value.ToDateTime(TimeOnly.MinValue);
                if (resignationDate < joinDateTime)
                    return "0 Years";

                int years = resignationDate.Year - joinDateTime.Year;
                int months = resignationDate.Month - joinDateTime.Month;
                int days = resignationDate.Day - joinDateTime.Day;

                if (days < 0)
                {
                    months--;
                    days += DateTime.DaysInMonth(resignationDate.Year, (resignationDate.Month == 1 ? 12 : resignationDate.Month - 1));
                }

                if (months < 0)
                {
                    years--;
                    months += 12;
                }

                return $"{years} Years {months} Months";
            }
            catch (Exception)
            {

                throw;
            }
            
        }



        #endregion


        #region Uni Tooltip

        public UniversalApprovalToolTip GetToolTipData(int id)
        {
            // Fetch all data synchronously to avoid context disposal issues
            var approvalHistories = _resignationsApprovalHistoryRepository.AllActive()
                .Include(e => e.ApprovalPerson)
                .Include(s => s.Status)
                .Where(e => e.ResignationID == id)
                .OrderBy(e => e.CreatedAt)
                .ToList(); // Synchronous execution

            var resignation = _resignationRepository.AllActive()
                .FirstOrDefault(r => r.ResignationID == id); // Synchronous execution

            // Get all required data for total stages calculation
            int totalStages = 1; // Default
            int currentStage = 1; // Default

            if (resignation?.EmployeeID != null)
            {
                var employee = _empOfficialRepository.AllActive()
                    .FirstOrDefault(e => e.EmployeeID == resignation.EmployeeID); // Synchronous

                if (employee != null)
                {
                    var approvalType = _approvalTypeRepository.AllActive()
                        .FirstOrDefault(a => a.ApprovalTypeName.ToLower() == "resignation approval"); // Synchronous

                    if (approvalType != null)
                    {
                        var approvalSettings = _approvalSettingRepository.AllActive()
                            .FirstOrDefault(a => a.ApprovalTypeID == approvalType.ApprovalTypeID
                                && a.OrganizationID == employee.OrganizationID
                                && a.OrganizationBranchID == employee.OrganizationBranchID); // Synchronous

                        if (approvalSettings != null)
                        {
                            if (approvalSettings.IsEnableThirdApproval) totalStages = 3;
                            else if (approvalSettings.IsEnableSecondApproval) totalStages = 2;
                            else totalStages = 1;
                        }
                    }
                }

                currentStage = Math.Min(resignation.ApprovalStep ?? 1, totalStages);
            }

            var stageDetails = approvalHistories.Select((history, index) => new ApproveStageDetails
            {
                approverStep = "Stage " + (index + 1).ToString(),
                statusName = history.Status?.StatusName ?? "",
                approvarPerson = history.ApprovalPerson != null
                    ? $"{history.ApprovalPerson.FirstName} {history.ApprovalPerson.LastName}"
                    : "Unknown",
                approvarNote = history.ApprovalPersonNote ?? "no remarks",
                approvedOrDeclineDate = history.CreatedAt?.ToString("dd/MM/yyyy hh:mm tt") ?? "-"
            }).ToList();

            return new UniversalApprovalToolTip
            {
                StageDetails = stageDetails,
                approvalDetails = $"{currentStage}/{totalStages}"
            };
        }

        
        #endregion

    }
}