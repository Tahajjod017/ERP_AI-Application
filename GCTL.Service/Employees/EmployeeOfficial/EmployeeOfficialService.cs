using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GCTL.Core.Repository;
using GCTL.Core.ServiceExtensions;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.Employee.EmployeeOfficial;
using GCTL.Core.ViewModels.Employee.EmployeePersonal;
using GCTL.Data.Models;
using Microsoft.EntityFrameworkCore;
using static Dapper.SqlMapper;

namespace GCTL.Service.Employees.EmployeeOfficial
{
    public class EmployeeOfficialService : IEmployeeOfficialService
    {

        private readonly IGenericRepository<GCTL.Data.Models.Employees> _employeePersonalRepository;
        private readonly IGenericRepository<GCTL.Data.Models.EmployeeOfficeInfo> _employeeOfficialRepository;

        public EmployeeOfficialService(IGenericRepository<Data.Models.Employees> employeePersonalRepository, IGenericRepository<EmployeeOfficeInfo> employeeOfficialRepository)
        {
            _employeePersonalRepository = employeePersonalRepository;
            _employeeOfficialRepository = employeeOfficialRepository;
        }


        #region Validation


        public async Task<CommonReturnViewModel> CheckValidEmployeeInfo(EmployeeOfficialPostViewModel model)
        {
            var result = new CommonReturnViewModel();

            await Task.Delay(1); 

            result.Success = true;


            //if (

            //    string.IsNullOrEmpty(model.OfficeEmail) ||
            //    string.IsNullOrEmpty(model.OfficePhone) ||
            //    string.IsNullOrEmpty(model.AppointmentLetterNo) ||
            //    string.IsNullOrEmpty(model.AttendanceId)


            //   )
            //{
            //    result.Success = false;
            //    result.Message = "Please fill out the form.";
            //}
            if (!IsValidEmail(model.OfficeEmail))
            {
                result.Success = false;
                result.Message = "Please enter a valid email address.";
            }
            
            else if (CheckDuplicateFields(model, out string duplicateMessage))
            {
                result.Success = false;
                result.Message = duplicateMessage;
            }
            else
            {
                result.Success = true;
                result.Message = "Employee information is valid.";
            }

            return result;

        }

        private bool CheckDuplicateFields(EmployeeOfficialPostViewModel model, out string duplicateMessage)
        {
            List<string> duplicateFields = new List<string>();

            var allEmployees = _employeeOfficialRepository.AllActive();

            var emp = _employeeOfficialRepository.AllActive().FirstOrDefault(e => e.EmployeeID == model.EmployeePersonalId);

            if (emp != null)
            {
                allEmployees = allEmployees.Where(e => e.EmployeeOfficeInfoID != emp.EmployeeOfficeInfoID);
            }

            if (model.EmployeeOfficeInfoID > 0)
            {
                allEmployees = allEmployees.Where(e => e.EmployeeOfficeInfoID != model.EmployeeOfficeInfoID);
            }

            if (model.EmployeeOfficeInfoID != null || model.EmployeeOfficeInfoID != 0)
            {
                allEmployees = allEmployees.Where(e => e.EmployeeOfficeInfoID != model.EmployeeOfficeInfoID);
            }

            if (!string.IsNullOrWhiteSpace(model.OfficeEmail) &&
                allEmployees.Any(e => e.OfficeEmail == model.OfficeEmail))
            {
                duplicateFields.Add("Office Email");
            }

            if (!string.IsNullOrWhiteSpace(model.OfficePhone) &&
                allEmployees.Any(e => e.OfficePhone == model.OfficePhone))
            {
                duplicateFields.Add("Mobile Number");
            }

            if (!string.IsNullOrWhiteSpace(model.AttendanceId) &&
                allEmployees.Any(e => e.AttendanceId == model.AttendanceId))
            {
                duplicateFields.Add("Attendance Id");
            }

            if (!string.IsNullOrWhiteSpace(model.AppointmentLetterNo) &&
                allEmployees.Any(e => e.AppointmentLetterNo == model.AppointmentLetterNo))
            {
                duplicateFields.Add("Appointment Letter No");
            }

           

            if (duplicateFields.Any())
            {
                duplicateMessage = $"{string.Join(", ", duplicateFields)} already exist.";
                return true;
            }

            duplicateMessage = null;
            return false;
        }


        private bool IsValidEmail(string email)
        {
            if (email != null)
            {
                var emailRegex = new Regex(@"^[\w\.-]+@[\w\.-]+\.[a-zA-Z]{2,}$");
                return emailRegex.IsMatch(email);
            }
            return true;
            
        }

        #endregion


        #region SaveEmployeeOfficialInfo

        public async Task<CommonReturnViewModel> SaveEmployeeOfficialInfo(EmployeeOfficialPostViewModel model)
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

                var chk = await _employeeOfficialRepository.All().FirstOrDefaultAsync(e => e.EmployeeID == model.EmployeePersonalId);

                if (chk == null)
                {
                    var entity = new EmployeeOfficeInfo
                    {
                        EmployeeID = model.EmployeePersonalId,
                        EmployeeOfficeId = model.EmployeeOfficeId,
                        OrganizationID = model.OrganizationID,
                        OrganizationBranchID = model.OrganizationBranchID,
                        DepartmentID = model.DepartmentID,
                        DesignationID = model.DesignationID,
                        EmployeeTypeID = model.EmployeeTypeID,
                        EmploymentNatureID = model.EmploymentNatureID,
                        SeniorSupervisorId = model.SeniorSupervisorId,
                        ImmediateSupervisorId = model.ImmediateSupervisorId,
                        HeadOfDepartmentId = model.HeadOfDepartmentId,
                        OfficePhone = model.OfficePhone,
                        OfficeEmail = model.OfficeEmail,
                        AttendanceId = model.AttendanceId,
                        EmploymentStatusId = model.EmploymentStatusId,
                        AppointmentLetterNo = model.AppointmentLetterNo,
                        AppointmentLetterIssueDate = model.AppointmentLetterIssueDate,
                        JoiningDate = model.JoiningDate,
                        ProvisionPeriodStartDate = model.ProvisionPeriodStartDate,
                        ProvisionPeriod = model.ProvisionPeriod,
                        ProvisionPeriodTtimeTypeID = model.ProvisionPeriodTtimeTypeID,
                        ConfirmationDate = model.ConfirmationDate,
                        ConfirmationLetterNo = model.ConfirmationLetterNo,
                        ContractEndDate = model.ContractEndDate,
                        CreatedAt = DateTime.UtcNow,
                    };

                    if (entity.DepartmentID == 0) entity.DepartmentID = null;
                    if (entity.DesignationID == 0) entity.DesignationID = null;
                    if (entity.EmployeeTypeID == 0) entity.EmployeeTypeID = null;
                    if (entity.EmploymentNatureID == 0) entity.EmploymentNatureID = null;
                    if (entity.SeniorSupervisorId == 0) entity.SeniorSupervisorId = null;
                    if (entity.ImmediateSupervisorId == 0) entity.ImmediateSupervisorId = null;
                    if (entity.HeadOfDepartmentId == 0) entity.HeadOfDepartmentId = null;
                    if (entity.EmploymentStatusId == 0) entity.EmploymentStatusId = null;
                    if (entity.ProvisionPeriodTtimeTypeID == 0) entity.ProvisionPeriodTtimeTypeID = null;

                    //foreach (var property in typeof(EmployeeOfficeInfo).GetProperties())
                    //{
                    //    if (property.PropertyType == typeof(int?))
                    //    {
                    //        var value = (int?)property.GetValue(entity);
                    //        if (value == 0)
                    //        {
                    //            property.SetValue(entity, null);
                    //        }
                    //    }
                    //}


                    await _employeeOfficialRepository.AddAsync(entity);

                    result.Success = true;
                    result.Message = "Employee official info saved successfully.";
                    result.Data = entity.EmployeeOfficeInfoID;
                }
                else
                {
                    model.EmployeeOfficeInfoID = chk.EmployeeOfficeInfoID;
                    var a = await UpdateEmployeeOfficialInfo(model);
                    result.Success = a.Success;
                    result.Message = a.Message;
                    result.Data = a.Data;
                }

                

            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"An error occurred: {ex.Message}";
            }

            return result;
        }


        public async Task<CommonReturnViewModel> UpdateEmployeeOfficialInfo(EmployeeOfficialPostViewModel model)
        {
            var result = new CommonReturnViewModel();

            try
            {
                if (model == null || model.EmployeeOfficeInfoID == null)
                
                {
                    result.Success = false;
                    result.Message = "Invalid input data.";
                    return result;
                }


                var entity = await _employeeOfficialRepository.AllActive().FirstOrDefaultAsync(e => e.EmployeeOfficeInfoID == model.EmployeeOfficeInfoID);
                if (entity == null)
                {
                    result.Success = false;
                    result.Message = "Employee office info not found.";
                    return result;
                }

                entity.EmployeeOfficeId = model.EmployeeOfficeId;
                entity.OrganizationID = model.OrganizationID;
                entity.OrganizationBranchID = model.OrganizationBranchID;
                entity.DepartmentID = model.DepartmentID;
                entity.DesignationID = model.DesignationID;
                entity.EmployeeTypeID = model.EmployeeTypeID;
                entity.EmploymentNatureID = model.EmploymentNatureID;
                entity.SeniorSupervisorId = model.SeniorSupervisorId;
                entity.ImmediateSupervisorId = model.ImmediateSupervisorId;
                entity.HeadOfDepartmentId = model.HeadOfDepartmentId;
                entity.OfficePhone = model.OfficePhone;
                entity.OfficeEmail = model.OfficeEmail;
                entity.AttendanceId = model.AttendanceId;
                entity.EmploymentStatusId = model.EmploymentStatusId;
                entity.AppointmentLetterNo = model.AppointmentLetterNo;
                entity.AppointmentLetterIssueDate = model.AppointmentLetterIssueDate;
                entity.JoiningDate = model.JoiningDate;
                entity.ProvisionPeriodStartDate = model.ProvisionPeriodStartDate;
                entity.ProvisionPeriod = model.ProvisionPeriod;
                entity.ProvisionPeriodTtimeTypeID = model.ProvisionPeriodTtimeTypeID;
                entity.ConfirmationDate = model.ConfirmationDate;
                entity.ConfirmationLetterNo = model.ConfirmationLetterNo;
                entity.ContractEndDate = model.ContractEndDate;
                entity.UpdatedAt = DateTime.UtcNow;

                if (entity.DepartmentID == 0) entity.DepartmentID = null;
                if (entity.DesignationID == 0) entity.DesignationID = null;
                if (entity.EmployeeTypeID == 0) entity.EmployeeTypeID = null;
                if (entity.EmploymentNatureID == 0) entity.EmploymentNatureID = null;
                if (entity.SeniorSupervisorId == 0) entity.SeniorSupervisorId = null;
                if (entity.ImmediateSupervisorId == 0) entity.ImmediateSupervisorId = null;
                if (entity.HeadOfDepartmentId == 0) entity.HeadOfDepartmentId = null;
                if (entity.EmploymentStatusId == 0) entity.EmploymentStatusId = null;
                if (entity.ProvisionPeriodTtimeTypeID == 0) entity.ProvisionPeriodTtimeTypeID = null;

                //foreach (var property in typeof(EmployeeOfficeInfo).GetProperties())
                //{
                //    if (property.PropertyType == typeof(int?))
                //    {
                //        var value = (int?)property.GetValue(entity);
                //        if (value == 0)
                //        {
                //            property.SetValue(entity, null);
                //        }
                //    }
                //}

                await _employeeOfficialRepository.UpdateAsync(entity);

                result.Success = true;
                result.Message = "Employee official info updated successfully.";
                result.Data = entity.EmployeeID;

            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"An error occurred: {ex.Message}";
            }

            return result;
        }

        #endregion


        #region Get EmpByID

        public async Task< EmployeeOfficialPostViewModel> GetEmployeeOfficalDetails(int id)
        {
            var empPersonal = await _employeePersonalRepository.AllActive().FirstOrDefaultAsync(e => e.EmployeeID == id);
            var empOfficial =await  _employeeOfficialRepository.AllActive().FirstOrDefaultAsync(e => e.EmployeeID == id);

            EmployeeOfficialPostViewModel model = new EmployeeOfficialPostViewModel();

            if (empPersonal != null)
            {
                model.EmployeePersonalId = empPersonal.EmployeeID;
                model.PersonalEmail = empPersonal.Email;
                model.PersonalPhone = empPersonal.MobileNumber;
            }

            if (empOfficial != null)
            {
                model.EmployeeOfficeId = empOfficial.EmployeeOfficeId ?? string.Empty;
                model.EmployeeOfficeInfoID = empOfficial.EmployeeOfficeInfoID;
                model.OrganizationID = (int)empOfficial.OrganizationID;
                model.OrganizationBranchID = empOfficial.OrganizationBranchID;
                model.DepartmentID = empOfficial.DepartmentID;
                model.DesignationID = empOfficial.DesignationID;
                model.EmployeeTypeID = empOfficial.EmployeeTypeID;
                model.EmploymentNatureID = empOfficial.EmploymentNatureID;
                model.SeniorSupervisorId = empOfficial.SeniorSupervisorId;
                model.ImmediateSupervisorId = empOfficial.ImmediateSupervisorId;
                model.HeadOfDepartmentId = empOfficial.HeadOfDepartmentId;
                model.OfficePhone = empOfficial.OfficePhone ?? string.Empty;
                model.OfficeEmail = empOfficial.OfficeEmail ?? string.Empty;
                model.AttendanceId = empOfficial.AttendanceId ?? string.Empty;
                model.EmploymentStatusId = (int)empOfficial.EmploymentStatusId;
                model.AppointmentLetterNo = empOfficial.AppointmentLetterNo ?? string.Empty;
                model.AppointmentLetterIssueDate = empOfficial.AppointmentLetterIssueDate;
                model.JoiningDate = empOfficial.JoiningDate;
                model.ProvisionPeriodStartDate = empOfficial.ProvisionPeriodStartDate;
                model.ProvisionPeriod = empOfficial.ProvisionPeriod;
                model.ProvisionPeriodTtimeTypeID = empOfficial.ProvisionPeriodTtimeTypeID;
                model.ConfirmationDate = empOfficial.ConfirmationDate;
                model.ConfirmationLetterNo = empOfficial.ConfirmationLetterNo ?? string.Empty;
                model.ContractEndDate = empOfficial.ContractEndDate;
            }

            return model;
        }

        #endregion

        #region GetFull Emploffice 

        public async Task<EmployeeOfficialGetViewModel> GetFullEmployeeOfficalDetails(int id)
        {
            try
            {
                var empPersonal = await _employeePersonalRepository.AllActive().FirstOrDefaultAsync(e => e.EmployeeID == id);
                var empOfficial = await _employeeOfficialRepository.AllActive()
                    .Include(e => e.Organization).Include(e => e.OrganizationBranch).Include(w => w.Department)
                    .Include(e => e.Designation).Include(e => e.EmployeeType).Include(w => w.EmploymentNature)
                    .Include(e => e.SeniorSupervisor).Include(e => e.ImmediateSupervisor).Include(w => w.HeadOfDepartment)
                    .Include(e => e.EmploymentStatus).Include(e => e.ProvisionPeriodTtimeType)
                    .FirstOrDefaultAsync(e => e.EmployeeID == id);

                EmployeeOfficialGetViewModel model = new EmployeeOfficialGetViewModel();

                if (empPersonal != null)
                {
                    model.EmployeePersonalId = empPersonal.EmployeeID;
                    model.PersonalEmail = empPersonal.Email ?? "";
                    model.PersonalPhone = empPersonal.MobileNumber ?? "";
                }

                //if (empOfficial != null)
                //{
                    model.EmployeeOfficeId = empOfficial?.EmployeeOfficeId ?? "";
                    model.EmployeeOfficeInfoID = empOfficial?.EmployeeOfficeInfoID;
                    model.OrganizationID = empOfficial?.OrganizationID;
                    model.OrganizationName = empOfficial?.Organization?.OrganizationName ?? "";

                    model.OrganizationBranchID = empOfficial?.OrganizationBranchID;
                    model.OrganizationBranchName = empOfficial?.OrganizationBranch?.OrganizationBranchName ?? "";

                    model.DepartmentID = empOfficial?.DepartmentID;
                    model.DepartmentName = empOfficial?.Department?.DepartmentName ?? "";

                    model.DesignationID = empOfficial?.DesignationID;
                    model.DesignationName = empOfficial?.Designation?.DesignationName ?? "";

                    model.EmployeeTypeID = empOfficial?.EmployeeTypeID;
                    model.EmployeeTypeName = empOfficial?.EmployeeType?.EmployeeTypeName ?? "";

                    model.EmploymentNatureID = empOfficial?.EmploymentNatureID;
                    model.EmploymentNatureName = empOfficial?.EmploymentNature?.EmploymentNatureName ?? "";

                    model.SeniorSupervisorId = empOfficial?.SeniorSupervisorId;
                    model.SeniorSupervisorName = empOfficial?.SeniorSupervisor != null
                        ? $"{empOfficial?.SeniorSupervisor.FirstName ?? ""} {empOfficial?.SeniorSupervisor.LastName ?? ""}".Trim()
                        : "";

                    model.ImmediateSupervisorId = empOfficial?.ImmediateSupervisorId;
                    model.ImmediateSupervisorName = empOfficial?.ImmediateSupervisor != null
                        ? $"{empOfficial?.ImmediateSupervisor.FirstName ?? ""} {empOfficial?.ImmediateSupervisor.LastName ?? ""}".Trim()
                        : "";

                    model.HeadOfDepartmentId = empOfficial?.HeadOfDepartmentId;
                    model.HeadOfDepartmentName = empOfficial?.HeadOfDepartment != null
                        ? $"{empOfficial?.HeadOfDepartment.FirstName ?? ""} {empOfficial?.HeadOfDepartment.LastName ?? ""}".Trim()
                        : "";

                    model.OfficePhone = empOfficial?.OfficePhone ?? "";
                    model.OfficeEmail = empOfficial?.OfficeEmail ?? "";
                    model.AttendanceId = empOfficial?.AttendanceId ?? "";

                    model.EmploymentStatusId = empOfficial?.EmploymentStatusId;
                    model.EmploymentStatusName = empOfficial?.EmploymentStatus?.StatusName ?? "";

                    model.AppointmentLetterNo = empOfficial?.AppointmentLetterNo ?? "";
                    model.AppointmentLetterIssueDate = empOfficial?.AppointmentLetterIssueDate ?? default;
                    model.JoiningDate = empOfficial?.JoiningDate ?? default;

                    model.ProvisionPeriodStartDate = empOfficial?.ProvisionPeriodStartDate ?? default;
                    model.ProvisionPeriod = empOfficial?.ProvisionPeriod ?? 0;
                    model.ProvisionPeriodTtimeTypeID = empOfficial?.ProvisionPeriodTtimeTypeID;
                    model.ProvisionPeriodTtimeTypeName = empOfficial?.ProvisionPeriodTtimeType?.ProvisionPeriodTtimeTypeName ?? "";

                    model.ConfirmationDate = empOfficial?.ConfirmationDate ?? default;
                    model.ConfirmationLetterNo = empOfficial?.ConfirmationLetterNo ?? "";
                    model.ContractEndDate = empOfficial?.ContractEndDate ?? default;
               // }



                return model;
            }
            catch (Exception)
            {

                throw;
            }
            
        }

        public async Task<IEnumerable<EmployeeOfficialGetViewModel>> GetAllEmployeeOfficialDetailsByCompanyAsync( int compId)
        {
            try
            {
                var empPersonals = await _employeePersonalRepository.AllActive().ToListAsync();
                var empOfficials = await _employeeOfficialRepository.AllActive()
                    .Include(e => e.Organization)
                    .Include(e => e.OrganizationBranch)
                    .Include(w => w.Department)
                    .Include(e => e.Designation)
                    .Include(e => e.EmployeeType)
                    .Include(w => w.EmploymentNature)
                    .Include(e => e.SeniorSupervisor)
                    .Include(e => e.ImmediateSupervisor)
                    .Include(w => w.HeadOfDepartment)
                    .Include(e => e.EmploymentStatus)
                    .Include(e => e.ProvisionPeriodTtimeType)
                    .Where(e => e.OrganizationID == compId)
                    .ToListAsync();

                var result = from official in empOfficials
                             join personal in empPersonals on official.EmployeeID equals personal.EmployeeID into personalGroup
                             from personal in personalGroup.DefaultIfEmpty()
                             select new EmployeeOfficialGetViewModel
                             {
                                 EmployeePersonalId = personal?.EmployeeID ?? 0,
                                 PersonalEmail = personal?.Email ?? string.Empty,
                                 PersonalPhone = personal?.MobileNumber ?? string.Empty,

                                 EmployeeOfficeId = official.EmployeeOfficeId ?? string.Empty,
                                 EmployeeOfficeInfoID = official.EmployeeOfficeInfoID,
                                 OrganizationID = official.OrganizationID,
                                 OrganizationName = official.Organization?.OrganizationName ?? string.Empty,

                                 OrganizationBranchID = official.OrganizationBranchID,
                                 OrganizationBranchName = official.OrganizationBranch?.OrganizationBranchName ?? string.Empty,

                                 DepartmentID = official.DepartmentID,
                                 DepartmentName = official.Department?.DepartmentName ?? string.Empty,

                                 DesignationID = official.DesignationID,
                                 DesignationName = official.Designation?.DesignationName ?? string.Empty,

                                 EmployeeTypeID = official.EmployeeTypeID,
                                 EmployeeTypeName = official.EmployeeType?.EmployeeTypeName ?? string.Empty,

                                 EmploymentNatureID = official.EmploymentNatureID,
                                 EmploymentNatureName = official.EmploymentNature?.EmploymentNatureName ?? string.Empty,

                                 SeniorSupervisorId = official.SeniorSupervisorId,
                                 SeniorSupervisorName = official.SeniorSupervisor != null
                                     ? $"{official.SeniorSupervisor.FirstName} {official.SeniorSupervisor.LastName}"
                                     : string.Empty,

                                 ImmediateSupervisorId = official.ImmediateSupervisorId,
                                 ImmediateSupervisorName = official.ImmediateSupervisor != null
                                     ? $"{official.ImmediateSupervisor.FirstName} {official.ImmediateSupervisor.LastName}"
                                     : string.Empty,

                                 HeadOfDepartmentId = official.HeadOfDepartmentId,
                                 HeadOfDepartmentName = official.HeadOfDepartment != null
                                     ? $"{official.HeadOfDepartment.FirstName} {official.HeadOfDepartment.LastName}"
                                     : string.Empty,

                                 OfficePhone = official.OfficePhone ?? string.Empty,
                                 OfficeEmail = official.OfficeEmail ?? string.Empty,
                                 AttendanceId = official.AttendanceId ?? string.Empty,

                                 EmploymentStatusId = official.EmploymentStatusId,
                                 EmploymentStatusName = official.EmploymentStatus?.StatusName ?? string.Empty,

                                 AppointmentLetterNo = official.AppointmentLetterNo ?? string.Empty,
                                 AppointmentLetterIssueDate = official.AppointmentLetterIssueDate,
                                 JoiningDate = official.JoiningDate,

                                 ProvisionPeriodStartDate = official.ProvisionPeriodStartDate,
                                 ProvisionPeriod = official.ProvisionPeriod,
                                 ProvisionPeriodTtimeTypeID = official.ProvisionPeriodTtimeTypeID,
                                 ProvisionPeriodTtimeTypeName = official.ProvisionPeriodTtimeType?.ProvisionPeriodTtimeTypeName ?? string.Empty,

                                 ConfirmationDate = official.ConfirmationDate,
                                 ConfirmationLetterNo = official.ConfirmationLetterNo ?? string.Empty,
                                 ContractEndDate = official.ContractEndDate
                             };

                return result.ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

    }
}
