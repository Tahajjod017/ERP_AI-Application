using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.AttendanceManagement.LeaveManagements.LeaveRequest;
using GCTL.Core.ViewModels.AttendanceManagement.LeaveManagements.LeaveSettings;
using GCTL.Core.ViewModels.MasterSetup.Statuses;
using GCTL.Core.ViewModels.PayrollManagements.PayrollPolicy;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.AttendanceManagement.LeaveManagements;
using GCTL.Service.Pagination;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.PayRollManagements.PayRollPolicy
{
    public class EmployeeBenefitsService : AppService<EmployeeBenefits>, IEmployeeBenefitsService
    {
        private readonly IGenericRepository<EmployeeBenefits> empBenefits;
        private readonly IGenericRepository<Organization> organization;
        private readonly IGenericRepository<SalaryTypes> salaryTypes;
        private readonly IUserInfoService userInfoService;
        public EmployeeBenefitsService(IGenericRepository<EmployeeBenefits> empBenefits, IGenericRepository<Organization> organization, IGenericRepository<SalaryTypes> salaryTypes, IUserInfoService userInfoService) : base(empBenefits)
        {
            this.empBenefits = empBenefits;
            this.organization = organization;
            this.salaryTypes = salaryTypes;
            this.userInfoService = userInfoService;
        }



        #region Get All Dataum

        public async Task<PaginationService<EmployeeBenefits, PayRollEmpBenefitsGetAllVM>.PaginationResult<PayRollEmpBenefitsGetAllVM>> GetAllTableAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string currentSortColumn = "", string currentSortOrder = "", int? organizationId = null)
        {
            try
            {

                //var employeeId = await appDb.Users.Where(u => u.Id == userId).Select(e => e.EmployeeId).FirstOrDefaultAsync();
                //var roleName = await(from user in appDb.Users
                //                     join userRole in appDb.UserRoles on user.Id equals userRole.UserId
                //                     join role in appDb.Roles on userRole.RoleId equals role.Id
                //                     where user.Id == userId
                //                     select role.Name).FirstOrDefaultAsync();

                // 🔹 Step 3: Base query with includes
                var query = empBenefits.AllActive().Include(x => x.YearlyEndBonusType).Include(x => x.FastivalBonusOnSalaryType).Include(x => x.ProvidentFundOnSalaryType).Include(x => x.Organization).OrderByDescending(x => x.EmployeeBenefitID).AsQueryable();
                if (query == null)
                {
                    throw new InvalidOperationException("query source is null.");
                }

                // 🔹 Step 4: Filter if not SuperAdmin
                //if (string.IsNullOrEmpty(roleName) || !string.Equals(roleName, "SuperAdmin", StringComparison.OrdinalIgnoreCase))
                //{
                //    query = query.Where(x => x.EmployeeID == employeeId);
                //}
                //
                //
               
                if (organizationId.HasValue)
                {
                   
                    query = query.Where(x => x.OrganizationID==organizationId);
                }
                Expression<Func<EmployeeBenefits, object>> orderByExpression = currentSortColumn?.ToLower() switch
                {
                    "employeename" => x => x.Organization.OrganizationName,
                    "leavetype" => x => x.HealthInsurance,
                  
                    _ => x => x.EmployeeBenefitID
                };

                query = currentSortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(orderByExpression)
                    : query.OrderBy(orderByExpression);

              

                // For approver Step
                
                //
                var result = await PaginationService<EmployeeBenefits, PayRollEmpBenefitsGetAllVM>.GetPaginatedData(


                    query,
                    pageNumber,
                    pageSize,
                    searchTerm,

                    currentSortColumn,
                    currentSortOrder,

                   term => b =>
                      EF.Functions.Like(b.EmployeeBenefitID.ToString(), $"%{term}%") ||
                      EF.Functions.Like(b.Organization.OrganizationName, $"%{term}%") ,


                    b => new PayRollEmpBenefitsGetAllVM
                    {
                        EmployeeBenefitID = b.EmployeeBenefitID,
                        HealthInsurance = b.HealthInsurance,
                        OrganizationName =b.Organization.OrganizationName ?? string.Empty,
                        ProvidentFundOnSalaryTypeName=b.ProvidentFundOnSalaryType.SalaryTypeName ?? string.Empty,
                        FastivalBonusOnSalaryTypeName=b.FastivalBonusOnSalaryType.SalaryTypeName ?? string.Empty,
                        YearlyEndBonusTypeName=b.YearlyEndBonusType.YearlyEndBonusTypeName ?? string.Empty,
                        PerformanceBonus=b.PerformanceBonus,
                        FastivalBonusRate=b.FastivalBonusRate,
                    });

                return result;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error : {ex.Message}");

                return new PaginationService<EmployeeBenefits, PayRollEmpBenefitsGetAllVM>.PaginationResult<PayRollEmpBenefitsGetAllVM>
                {
                    Data = new List<PayRollEmpBenefitsGetAllVM>(),
                    TotalCount = 0
                };
            }
        }

       
        #endregion


        #region Save Data
        public async Task<CommonReturnViewModel> SaveEmployeeBenefits(PayRollEmpBenefitsSaveVM entityVM)
        {
            await empBenefits.BeginTransactionAsync();
            try
            {
                if (entityVM == null)
                {
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = "Data Not Found"
                    };
                }
                var entity = new EmployeeBenefits
                {
                    OrganizationID = entityVM.OrganizationID,
                    IsHealthInsuranceEnabled = entityVM.IsHealthInsuranceEnabled,
                    IsFastivalBonusEnabled = entityVM.IsFastivalBonusEnabled,
                    PerformanceBonus = entityVM.PerformanceBonus,
                    FastivalBonusRate = entityVM.FastivalBonusRate,
                    FastivalBonusOnSalaryTypeID = entityVM.FastivalBonusOnSalaryTypeID,
                    IsProvidentFundEnabled = entityVM.IsProvidentFundEnabled,
                    ProvidentFundEmployeeContrebution = entityVM.ProvidentFundEmployeeContrebution,
                    ProvidentFundOrganizationContrebution = entityVM.ProvidentFundOrganizationContrebution,
                    ProvidentFundOnSalaryTypeID = entityVM.ProvidentFundOnSalaryTypeID,
                    ProvidentFundMinimumServiceYear = (int?)entityVM.ProvidentFundMinimumServiceYear,
                    HealthInsurance = entityVM.HealthInsurance,
                    IsPerformanceBonusEnabled = entityVM.IsPerformanceBonusEnabled,
                    IsYearEndBonusEnabled = entityVM.IsYearEndBonusEnabled,
                    YearlyEndBonusTypeID = entityVM.YearlyEndBonusTypeID,
                    FastivalBonusMinimumServiceInMonth= (int?)entityVM.FastivalBonusMinimumServiceInMonth,
                    CreatedAt = DateTime.Now,
                    CreatedBy = entityVM.CreatedBy,
                    LIP = entityVM.LIP,
                    LMAC = entityVM.LMAC,
                };
                await empBenefits.AddAsync(entity);
                await empBenefits.CommitTransactionAsync();
                return new CommonReturnViewModel
                {
                    Success = true,
                    Message = "Save Succssfully"
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }


        #endregion

        #region Update  Datum
        public async Task<CommonReturnViewModel> UpdateEmployeeBenefits(PayRollEmpBenefitsSaveVM entityVM)
        {
            try
            {
                var existingEntity = await empBenefits.GetByIdAsync(entityVM.EmployeeBenefitID);

                if (existingEntity == null)
                {
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = "Employee Benefits record not found."
                    };
                }

                // Map incoming ViewModel to the existing entity
                existingEntity.OrganizationID = entityVM.OrganizationID;
                existingEntity.IsHealthInsuranceEnabled = entityVM.IsHealthInsuranceEnabled;
                existingEntity.IsFastivalBonusEnabled = entityVM.IsFastivalBonusEnabled;
                existingEntity.PerformanceBonus = entityVM.PerformanceBonus;
                existingEntity.FastivalBonusRate = entityVM.FastivalBonusRate;
                existingEntity.FastivalBonusOnSalaryTypeID = entityVM.FastivalBonusOnSalaryTypeID;
                existingEntity.IsProvidentFundEnabled = entityVM.IsProvidentFundEnabled;
                existingEntity.ProvidentFundEmployeeContrebution = entityVM.ProvidentFundEmployeeContrebution;
                existingEntity.ProvidentFundOrganizationContrebution = entityVM.ProvidentFundOrganizationContrebution;
                existingEntity.ProvidentFundOnSalaryTypeID = entityVM.ProvidentFundOnSalaryTypeID;
                existingEntity.ProvidentFundMinimumServiceYear = (int?)entityVM.ProvidentFundMinimumServiceYear;
                existingEntity.HealthInsurance = entityVM.HealthInsurance;
                existingEntity.IsPerformanceBonusEnabled = entityVM.IsPerformanceBonusEnabled;
                existingEntity.IsYearEndBonusEnabled = entityVM.IsYearEndBonusEnabled;
                existingEntity.YearlyEndBonusTypeID = entityVM.YearlyEndBonusTypeID;
                existingEntity.FastivalBonusMinimumServiceInMonth = (int?)entityVM.FastivalBonusMinimumServiceInMonth;

                // Save changes
                await empBenefits.UpdateAsync(existingEntity);

                return new CommonReturnViewModel
                {
                    Success = true,
                    Message = "Employee Benefits updated successfully."
                };
            }
            catch (Exception ex)
            {
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = $"Error while updating Employee Benefits: {ex.Message}"
                };
            }
        }

        #endregion
        #region Get By Id
        public async Task<CommonReturnViewModel> GetById(int employeeBenefitID)
        {
            try
            {
                var entity = await empBenefits.GetByIdAsync(employeeBenefitID);
                var resutl = new PayRollEmpBenefitsGetByID
                {
                    EmployeeBenefitID = entity.EmployeeBenefitID,
                    OrganizationID = entity.OrganizationID,
                    IsHealthInsuranceEnabled = entity.IsHealthInsuranceEnabled,
                    IsFastivalBonusEnabled = entity.IsFastivalBonusEnabled,
                    PerformanceBonus = entity.PerformanceBonus,
                    FastivalBonusRate = entity.FastivalBonusRate,
                    FastivalBonusOnSalaryTypeID = entity.FastivalBonusOnSalaryTypeID,
                    IsProvidentFundEnabled = entity.IsProvidentFundEnabled,
                    ProvidentFundEmployeeContrebution = entity.ProvidentFundEmployeeContrebution,
                    ProvidentFundOrganizationContrebution = entity.ProvidentFundOrganizationContrebution,
                    ProvidentFundOnSalaryTypeID = entity.ProvidentFundOnSalaryTypeID,
                    ProvidentFundMinimumServiceYear = entity.ProvidentFundMinimumServiceYear,
                    HealthInsurance = entity.HealthInsurance,
                    IsPerformanceBonusEnabled = entity.IsPerformanceBonusEnabled,
                    IsYearEndBonusEnabled = entity.IsYearEndBonusEnabled,
                    YearlyEndBonusTypeID = entity.YearlyEndBonusTypeID,
                    FastivalBonusMinimumServiceInMonth = entity.FastivalBonusMinimumServiceInMonth,


                };

                return new CommonReturnViewModel
                {
                    Success= true,
                    Data=resutl,
                };
            }
            catch (Exception)
            {
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message="Employee Benefits Does not find"
                };
            }
        }
        #endregion
        #region Delete Data 

        public async Task<CommonReturnViewModel> SoftDeletePayRollEmpRequest(DeleteRequestVM deleteRequestVM)
        {
            await empBenefits.BeginTransactionAsync();
            try
            {
                var data = await empBenefits.FindAsync(x => deleteRequestVM.Ids.Contains(x.EmployeeBenefitID));
                if (data == null || data.Count == 0)
                {
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = "No data found to delete."
                    };
                }

                var beforeEntity = JsonConvert.DeserializeObject<List<PayRollEmpBenefitsSaveVM>>(
             JsonConvert.SerializeObject(data, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }));
                var targetIds = data.Select(x => (int?)x.EmployeeBenefitID).ToList();
                foreach (var item in data)
                {
                    item.DeletedAt = DateTime.Now;
                    item.LIP = deleteRequestVM.LIP;
                    item.LMAC = deleteRequestVM.LMAC;
                    item.DeletedBy = deleteRequestVM.DeletedBy ?? null;
                }

                await empBenefits.UpdateRangeAsync(data);
                await userInfoService.ActionLogDeleteAsync("PayRoll Employee Benefits Settigs", ActionName.DataDeleted, null, beforeEntity, targetIds, deleteRequestVM);
                await empBenefits.CommitTransactionAsync();

                return new CommonReturnViewModel
                {
                    Success = true,
                    Message = $"Deleted Successfully."

                };
            }
            catch (Exception ex)
            {
                await empBenefits.RollbackTransactionAsync();
                throw new Exception("Error occurred during the deletion of data.", ex);
            }
        }

        
        #endregion
    }

}

