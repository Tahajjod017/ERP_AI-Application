

using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.Employee.EmployeeSalary;
using GCTL.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace GCTL.Service.Employees.EmployeeSalary
{
    public class EmployeeSalaryService : IEmployeeSalaryService
    {
        private readonly IGenericRepository<GCTL.Data.Models.Employees> _employeeRepository;
        private readonly IGenericRepository<Grade> _gradeRepository;
        private readonly IGenericRepository<Currencies> _currencyRepository;
        private readonly IGenericRepository<PaymentPeriodTypes> _paymentPeriodTypeRepository;
        private readonly IGenericRepository<PaymentModes> _paymentModeRepository;
        private readonly IGenericRepository<EmployeeSalarySettings> _employeeSalaryRepository;
        private readonly IGenericRepository<EmployeeBasePaymentModes> _employeeBasePaymentRepository;

        public EmployeeSalaryService(IGenericRepository<Data.Models.Employees> employeeRepository, IGenericRepository<Grade> gradeRepository, IGenericRepository<Currencies> currencyRepository, IGenericRepository<PaymentPeriodTypes> paymentPeriodTypeRepository, IGenericRepository<PaymentModes> paymentModeRepository, IGenericRepository<EmployeeSalarySettings> employeeSalaryRepository, IGenericRepository<EmployeeBasePaymentModes> employeeBasePaymentRepository)
        {
            _employeeRepository = employeeRepository;
            _gradeRepository = gradeRepository;
            _currencyRepository = currencyRepository;
            _paymentPeriodTypeRepository = paymentPeriodTypeRepository;
            _paymentModeRepository = paymentModeRepository;
            _employeeSalaryRepository = employeeSalaryRepository;
            _employeeBasePaymentRepository = employeeBasePaymentRepository;
        }

        #region Get EmpDetails
        public async Task<EmployeeSalaryGetViewModel> GetEmployeeSalaryByEmployeeIdAsync(int employeeId)
        {
            var employeeSalaryInfo = await (
                from sal in _employeeSalaryRepository.AllActive()
                join emp in _employeeRepository.AllActive()
                    on sal.EmployeeID equals emp.EmployeeID into empGroup
                from emp in empGroup.DefaultIfEmpty()
                where sal.EmployeeID == employeeId
                select new
                {
                    sal,
                    emp
                }
            ).FirstOrDefaultAsync();

            if (employeeSalaryInfo != null)
            {
                var basePayments = await _employeeBasePaymentRepository.AllActive()
                    .Where(p => p.EmployeeID == employeeId)
                    .ToListAsync();

                var viewModel = new EmployeeSalaryGetViewModel
                {
                    EmployeePersonalId = employeeSalaryInfo.emp?.EmployeeID ?? 0,
                    PersonalPhone = employeeSalaryInfo.emp?.MobileNumber,
                    PersonalEmail = employeeSalaryInfo.emp?.Email,
                    EmployeeSalarySettingsID = employeeSalaryInfo.sal?.EmployeeSalarySettingsID,
                    BankName = employeeSalaryInfo.sal?.BankName,
                    BranchName = employeeSalaryInfo.sal?.BranchName,
                    AccountName = employeeSalaryInfo.sal?.AccountName,
                    AccountNo = employeeSalaryInfo.sal?.AccountNo,
                    Address = employeeSalaryInfo.sal?.Address,
                    ATMCardNo = employeeSalaryInfo.sal?.ATMCardNo,
                    RoutingNo = employeeSalaryInfo.sal?.RoutingNo,
                    SWIFTCode = employeeSalaryInfo.sal?.SWIFTCode,
                    IFSCCode = employeeSalaryInfo.sal?.IFSCCode,
                    bKashAccountNo = employeeSalaryInfo.sal?.bKashAccountNo,
                    RoketAccountNo = employeeSalaryInfo.sal?.RoketAccountNo,
                    NagodAccountNo = employeeSalaryInfo.sal?.NagodAccountNo,
                    EmployeeGID = employeeSalaryInfo.sal?.EmployeeGID,
                    GradeID = employeeSalaryInfo.sal?.GradeID,
                    Salary = employeeSalaryInfo.sal.Salary,
                    CurrencyID = employeeSalaryInfo.sal.CurrencyID,
                    PaymenPeriodTypeID = employeeSalaryInfo.sal.PaymenPeriodTypeID,
                    IsBenefitsEnabled = employeeSalaryInfo.sal?.IsBenefitsEnabled ?? false,
                    IsAllowanceEnabled = employeeSalaryInfo.sal?.IsAllowanceEnabled ?? false,
                    PaymentModeIds = basePayments
                        .Where(p => p.PaymentModeID.HasValue)
                        .Select(p => p.PaymentModeID.Value)
                        .ToList(),
                    PrimaryPaymentModeId = basePayments
                        .FirstOrDefault(p => p.IsPrimary)?.PaymentModeID,
                    PrimaryPaymentPercent = basePayments
                        .FirstOrDefault(p => p.IsPrimary)?.Percentage,
                    SecondaryPaymentModeId = basePayments
                        .FirstOrDefault(p => !p.IsPrimary)?.PaymentModeID
                };

                return viewModel;
            }

            var empPersonal = await _employeeRepository.AllActive().Where(e => e.EmployeeID == employeeId).Select(m => new EmployeeSalaryGetViewModel
            {
                EmployeePersonalId = m.EmployeeID ,
                PersonalPhone = m.MobileNumber,
                PersonalEmail = m.Email
            }).FirstOrDefaultAsync();
            
            return empPersonal;
        }

        public async Task<EmployeeSalaryPostViewModel> GetEmployeeSalaryByEmployeeIdPostAsync(int employeeId)
        {
            var employeeSalaryInfo = await(
                from sal in _employeeSalaryRepository.AllActive()
                join emp in _employeeRepository.AllActive()
                    on sal.EmployeeID equals emp.EmployeeID into empGroup
                from emp in empGroup.DefaultIfEmpty()
                where sal.EmployeeID == employeeId
                select new
                {
                    sal,
                    emp
                }
            ).FirstOrDefaultAsync();

            if (employeeSalaryInfo != null)
            {
                var basePayments = await _employeeBasePaymentRepository.AllActive()
                    .Where(p => p.EmployeeID == employeeId)
                    .ToListAsync();

                var viewModel = new EmployeeSalaryPostViewModel
                {
                    EmployeePersonalId = employeeSalaryInfo.emp?.EmployeeID ?? 0,
                    PersonalPhone = employeeSalaryInfo.emp?.MobileNumber,
                    PersonalEmail = employeeSalaryInfo.emp?.Email,
                    EmployeeSalarySettingsID = employeeSalaryInfo.sal?.EmployeeSalarySettingsID,
                    BankName = employeeSalaryInfo.sal?.BankName,
                    BranchName = employeeSalaryInfo.sal?.BranchName,
                    AccountName = employeeSalaryInfo.sal?.AccountName,
                    AccountNo = employeeSalaryInfo.sal?.AccountNo,
                    Address = employeeSalaryInfo.sal?.Address,
                    ATMCardNo = employeeSalaryInfo.sal?.ATMCardNo,
                    RoutingNo = employeeSalaryInfo.sal?.RoutingNo,
                    SWIFTCode = employeeSalaryInfo.sal?.SWIFTCode,
                    IFSCCode = employeeSalaryInfo.sal?.IFSCCode,
                    bKashAccountNo = employeeSalaryInfo.sal?.bKashAccountNo,
                    RoketAccountNo = employeeSalaryInfo.sal?.RoketAccountNo,
                    NagodAccountNo = employeeSalaryInfo.sal?.NagodAccountNo,
                    EmployeeGID = employeeSalaryInfo.sal?.EmployeeGID,
                    GradeID = employeeSalaryInfo.sal?.GradeID,
                    Salary = employeeSalaryInfo.sal.Salary,
                    CurrencyID = employeeSalaryInfo.sal.CurrencyID,
                    PaymenPeriodTypeID = employeeSalaryInfo.sal.PaymenPeriodTypeID,
                    IsBenefitsEnabled = employeeSalaryInfo.sal?.IsBenefitsEnabled ?? false,
                    IsAllowanceEnabled = employeeSalaryInfo.sal?.IsAllowanceEnabled ?? false,
                    PaymentModeIds = basePayments
                        .Where(p => p.PaymentModeID.HasValue)
                        .Select(p => p.PaymentModeID.Value)
                        .ToList(),
                    PrimaryPaymentModeId = basePayments
                        .FirstOrDefault(p => p.IsPrimary)?.PaymentModeID,
                    PrimaryPaymentPercent = basePayments
                        .FirstOrDefault(p => p.IsPrimary)?.Percentage,
                    SecondaryPaymentModeId = basePayments
                        .FirstOrDefault(p => !p.IsPrimary)?.PaymentModeID
                };

                return viewModel;
            }

            var empPersonal = await _employeeRepository.AllActive().Where(e => e.EmployeeID == employeeId).Select(m => new EmployeeSalaryPostViewModel
            {
                EmployeePersonalId = m.EmployeeID,
                PersonalPhone = m.MobileNumber,
                PersonalEmail = m.Email
            }).FirstOrDefaultAsync();

            return empPersonal;
        }

        #endregion

        #region SaveEmployeeSalary

        public async Task<CommonReturnViewModel> SaveEmployeeSalaryAsync(EmployeeSalaryPostViewModel model)
        {
            var result = new CommonReturnViewModel();

            try
            {
                if (model == null)
                {
                    result.Success = false;
                    result.Message = "Invalid input data.";
                    return result;
                }

                var existingSalary = await _employeeSalaryRepository.All()
                    .FirstOrDefaultAsync(s => s.EmployeeID == model.EmployeePersonalId);

                if (existingSalary == null)
                {
                    var entity = new EmployeeSalarySettings
                    {
                        EmployeeID = model.EmployeePersonalId,
                        BankName = model.BankName,
                        BranchName = model.BranchName,
                        AccountName = model.AccountName,
                        AccountNo = model.AccountNo,
                        Address = model.Address,
                        ATMCardNo = model.ATMCardNo,
                        RoutingNo = model.RoutingNo,
                        SWIFTCode = model.SWIFTCode,
                        IFSCCode = model.IFSCCode,
                        bKashAccountNo = model.bKashAccountNo,
                        RoketAccountNo = model.RoketAccountNo,
                        NagodAccountNo = model.NagodAccountNo,
                        EmployeeGID = model.EmployeeGID,
                        GradeID = model.GradeID == 0 ? null : model.GradeID,
                        Salary = model.Salary,
                        CurrencyID = model.CurrencyID == 0 ? null : model.CurrencyID,
                        PaymenPeriodTypeID = model.PaymenPeriodTypeID == 0 ? null : model.PaymenPeriodTypeID,
                        IsBenefitsEnabled = model.IsBenefitsEnabled,
                        IsAllowanceEnabled = model.IsAllowanceEnabled,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _employeeSalaryRepository.AddAsync(entity);

                    // Save Base Payments
                    if (model.PaymentModeIds != null && model.PaymentModeIds.Any())
                    {
                        var basePayments = model.PaymentModeIds.Select((paymentModeId, index) => new EmployeeBasePaymentModes
                        {
                            EmployeeID = model.EmployeePersonalId,
                            PaymentModeID = paymentModeId,
                            IsPrimary = paymentModeId == model.PrimaryPaymentModeId,
                            Percentage = paymentModeId == model.PrimaryPaymentModeId ? model.PrimaryPaymentPercent : null,
                            CreatedAt = DateTime.UtcNow
                        }).ToList();

                        await _employeeBasePaymentRepository.AddRangeAsync(basePayments);
                    }

                    result.Success = true;
                    result.Message = "Employee salary info saved successfully.";
                    result.Data = entity.EmployeeID;
                }
                else
                {
                    model.EmployeeSalarySettingsID = existingSalary.EmployeeSalarySettingsID;
                    var updateResult = await UpdateEmployeeSalaryAsync(model);
                    result = updateResult;
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"An error occurred: {ex.Message}";
            }

            return result;
        }

        #endregion

        #region UpdateEmployeeSalary

        public async Task<CommonReturnViewModel> UpdateEmployeeSalaryAsync(EmployeeSalaryPostViewModel model)
        {
            var result = new CommonReturnViewModel();

            try
            {
                if (model == null || model.EmployeeSalarySettingsID == null)
                {
                    result.Success = false;
                    result.Message = "Invalid input data.";
                    return result;
                }

                var entity = await _employeeSalaryRepository.AllActive()
                    .FirstOrDefaultAsync(s => s.EmployeeSalarySettingsID == model.EmployeeSalarySettingsID);

                if (entity == null)
                {
                    result.Success = false;
                    result.Message = "Employee salary info not found.";
                    return result;
                }

                entity.BankName = model.BankName;
                entity.BranchName = model.BranchName;
                entity.AccountName = model.AccountName;
                entity.AccountNo = model.AccountNo;
                entity.Address = model.Address;
                entity.ATMCardNo = model.ATMCardNo;
                entity.RoutingNo = model.RoutingNo;
                entity.SWIFTCode = model.SWIFTCode;
                entity.IFSCCode = model.IFSCCode;
                entity.bKashAccountNo = model.bKashAccountNo;
                entity.RoketAccountNo = model.RoketAccountNo;
                entity.NagodAccountNo = model.NagodAccountNo;
                entity.EmployeeGID = model.EmployeeGID;
                entity.GradeID = model.GradeID == 0 ? null : model.GradeID;
                entity.Salary = model.Salary;
                entity.CurrencyID = model.CurrencyID == 0 ? null : model.CurrencyID;
                entity.PaymenPeriodTypeID = model.PaymenPeriodTypeID == 0 ? null : model.PaymenPeriodTypeID;
                entity.IsBenefitsEnabled = model.IsBenefitsEnabled;
                entity.IsAllowanceEnabled = model.IsAllowanceEnabled;
                entity.UpdatedAt = DateTime.UtcNow;

                await _employeeSalaryRepository.UpdateAsync(entity);

                // Remove old base payments
                var oldPayments = await _employeeBasePaymentRepository.All()
                    .Where(p => p.EmployeeID == model.EmployeePersonalId)
                    .ToListAsync();
                await _employeeBasePaymentRepository.DeleteRangeAsync(oldPayments);

                // Add updated base payments
                if (model.PaymentModeIds != null && model.PaymentModeIds.Any())
                {
                    var basePayments = model.PaymentModeIds.Select((paymentModeId, index) => new EmployeeBasePaymentModes
                    {
                        EmployeeID = model.EmployeePersonalId,
                        PaymentModeID = paymentModeId,
                        IsPrimary = paymentModeId == model.PrimaryPaymentModeId,
                        Percentage = paymentModeId == model.PrimaryPaymentModeId ? model.PrimaryPaymentPercent : null,
                        CreatedAt = DateTime.UtcNow
                    }).ToList();

                    await _employeeBasePaymentRepository.AddRangeAsync(basePayments);
                }

                result.Success = true;
                result.Message = "Employee salary info updated successfully.";
                result.Data = model.EmployeePersonalId;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"An error occurred: {ex.Message}";
            }

            return result;
        }

        #endregion

    }
}
