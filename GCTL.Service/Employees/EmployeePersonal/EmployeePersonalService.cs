using GCTL.Core.Repository;
using GCTL.Core.ServiceExtensions;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.CRM;
using GCTL.Core.ViewModels.Employee.EmployeePersonal;
using GCTL.Data.Models;
using GCTL.Service.Finance.TransactionAccount;
using GCTL.Service.ImageFileHandler;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace GCTL.Service.Employees.EmployeePersonal
{
    public class EmployeePersonalService : IEmployeePersonalService
    {
        private readonly IGenericRepository<GCTL.Data.Models.Employees> _employeePersonalRepository;
        private readonly IGenericRepository<GCTL.Data.Models.EmployeeOfficeInfo> _employeeOfficialRepository;
        private readonly IGenericRepository<Genders> _genderRepository;
        private readonly IGenericRepository<Religions> _religionRepository;
        private readonly IGenericRepository<BloodGroup> _bloodGroupRepository;
        private readonly IGenericRepository<BloodGroup> _natioanlityRepository;
        private readonly IGenericRepository<MaritalStatus> _maritalSatausRepository;
        private readonly IGenericRepository<Country> _countryRepository;
        private readonly IGenericRepository<Statuses> _statusRepository;
        private readonly IImageFileHandlerService _imageFileHandlerService;

        #region Added by Md. Rakib Hasan
        private readonly IGenericRepository<Heads> _heads;
        private readonly IGenericRepository<HeadDetails> _headDetails;
        private readonly IGenericRepository<TransactionAccounts> _transactionAccounts;
        private readonly IGenericRepository<SubAccounts> _subAccounts;
        private readonly IGenericRepository<MainAccounts> _mainAccounts;
        private readonly ITransactionAccountService _transactionAccountService;
        #endregion

        public EmployeePersonalService(IGenericRepository<GCTL.Data.Models.Employees> employeePersonalRepository, IGenericRepository<Country> countryRepository, IImageFileHandlerService imageFileHandlerService, IGenericRepository<EmployeeOfficeInfo> employeeOfficialRepository, IGenericRepository<Genders> genderRepository, IGenericRepository<Religions> religionRepository, IGenericRepository<BloodGroup> bloodGroupRepository, IGenericRepository<BloodGroup> natioanlityRepository, IGenericRepository<MaritalStatus> maritalSatausRepository, IGenericRepository<Statuses> statusRepository, IGenericRepository<Heads> heads, IGenericRepository<HeadDetails> headDetails, IGenericRepository<TransactionAccounts> transactionAccounts, IGenericRepository<SubAccounts> subAccounts, IGenericRepository<MainAccounts> mainAccounts, ITransactionAccountService transactionAccountService)
        {
            _employeePersonalRepository = employeePersonalRepository;
            _countryRepository = countryRepository;
            _imageFileHandlerService = imageFileHandlerService;
            _employeeOfficialRepository = employeeOfficialRepository;
            _genderRepository = genderRepository;
            _religionRepository = religionRepository;
            _bloodGroupRepository = bloodGroupRepository;
            _natioanlityRepository = natioanlityRepository;
            _maritalSatausRepository = maritalSatausRepository;
            _statusRepository = statusRepository;
            _heads = heads;
            _headDetails = headDetails;
            _transactionAccounts = transactionAccounts;
            _subAccounts = subAccounts;
            _mainAccounts = mainAccounts;
            _transactionAccountService = transactionAccountService;
        }

        #region Save Method
        public async Task<CommonReturnViewModel> SaveEmployeePersonalInfo(EmployeePersonalPostViewModel model)
        {
            var result = new CommonReturnViewModel();

            await _employeeOfficialRepository.BeginTransactionAsync();


            try
            {
                if (model == null)
                {
                    await _employeeOfficialRepository.RollbackTransactionAsync();
                    result.Success = false;
                    result.Message = "Invalid input data.";
                    return result;
                }

                int nationalityId = 0;

                if (model.Nationality != null)
                {
                     nationalityId = _countryRepository.All().Where(e => e.CountryName == model.Nationality).Select(e => e.CountryID).FirstOrDefault();
                }


                GCTL.Data.Models.Employees employee;

                string EmployeeImageFileName = "";
                string EmployeeSignatureFileName = "";

                if (model.EmployeePicture != null)
                {
                     EmployeeImageFileName = await _imageFileHandlerService.SaveFileAsync(model.EmployeePicture, "media/employee/images", true);
                }

                if (model.Signature != null)
                {
                     EmployeeSignatureFileName = await _imageFileHandlerService.SaveFileAsync(model.Signature, "media/employee/signatures");
                }
                
                


                if (model.EmployeeId == 0 || model.EmployeeId == null)
                {
                    #region Added by Md. Rakib Hasan
                    string schemaName = "dbo";
                    string tableName = "Employees";
                    int subAccId = 14;

                    var headDetail = await _headDetails.FirstOrDefaultAsync(hd => hd.SchemaName == schemaName && hd.TableName == tableName);

                    if (headDetail == null)
                    {
                        headDetail = new HeadDetails();
                        headDetail.SchemaName = schemaName;
                        headDetail.TableName = tableName;

                        headDetail.LIP = model.LIP;
                        headDetail.LMAC = model.LMAC;
                        headDetail.CreatedAt = DateTime.UtcNow;
                        headDetail.CreatedBy = model.CreatedBy;

                        await _headDetails.AddAsync(headDetail);
                    }

                    var head = await _heads.FirstOrDefaultAsync(h => h.HeadDetailID == headDetail.HeadDetailID);

                    if (head == null)
                    {
                        head = new Heads();
                        head.HeadDetailID = headDetail.HeadDetailID;

                        head.LIP = model.LIP;
                        head.LMAC = model.LMAC;
                        head.CreatedAt = DateTime.UtcNow;
                        head.CreatedBy = model.CreatedBy;

                        await _heads.AddAsync(head);
                    }

                    var headID = head.HeadID;

                    var subAccDetails = await _subAccounts.AllActive().FirstOrDefaultAsync(x => x.SubAccountID == subAccId);

                    if (subAccDetails == null)
                    {
                        await _employeeOfficialRepository.RollbackTransactionAsync();
                        result.Success = false;
                        result.Message = "Invalid input data.";
                        return result;
                    }

                    var generatedTrxAccCode = await _transactionAccountService.GenerateNextCodeAsync((int)subAccDetails.SubAccountID);

                    TransactionAccounts trxAccount = new TransactionAccounts();
                    trxAccount.SubAccountID = subAccDetails.SubAccountID;

                    trxAccount.TrxAccCode = generatedTrxAccCode;

                    trxAccount.TrxAccName = subAccDetails.SubAccountName;
                    trxAccount.IsActive = true;
                    trxAccount.Description = "Customer transaction account";
                    trxAccount.Head = head;

                    trxAccount.LIP = model.LIP;
                    trxAccount.LMAC = model.LMAC;
                    trxAccount.CreatedAt = DateTime.UtcNow;
                    trxAccount.CreatedBy = model.CreatedBy;

                    await _transactionAccounts.AddAsync(trxAccount);
                    #endregion

                    // New employee
                    employee = new GCTL.Data.Models.Employees
                    {
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        FatherName = model.FatherName,
                        MotherName = model.MotherName,
                        MobileNumber = model.PersonalMobile,
                        Email = model.PersonalEmail,
                        TIN = model.TinNo,

                        DateOfBirth =  model.DateOfBirth.ToDateOnly(),
                        AboutEmployee = model.AboutEmployee,
                        NID = model.NationalId,
                        State = model.State,
                        City = model.City,
                        HouseNo = model.HouseNo,
                        RoadNo = model.RoadNo,
                        PostalCode = model.PostalCode,
                        EmployeeImageFileName = EmployeeImageFileName,
                        EmployeeSignatureFileName = EmployeeSignatureFileName,
                        MaritalStatusID = Convert.ToInt32(model.MaritalStatus),
                        GenderID = Convert.ToInt32(model.Gender),
                        BloodGroupID = Convert.ToInt32(model.BloodGroup),
                        NationalityID = nationalityId,
                        ReligionID = Convert.ToInt32(model.Religion),
                        CountryID = model.Country,

                        EmployeeCode = model.EmployeeCode,
                        HeadID = headID, // Added by Md. Rakib Hasan

                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = model.CreatedBy
                    };


                    if (employee.BloodGroupID == 0) employee.BloodGroupID = null;
                    if (employee.GenderID == 0) employee.GenderID = null;
                    if (employee.MaritalStatusID == 0) employee.MaritalStatusID = null;
                    if (employee.NationalityID == 0) employee.NationalityID = null;
                    if (employee.ReligionID == 0) employee.ReligionID = null;
                    if (employee.CountryID == 0) employee.CountryID = null;



                    await _employeePersonalRepository.AddAsync(employee);

                    var activeStatus = await _statusRepository.AllActive().Where(e => e.StatusName.ToLower() == "active").FirstOrDefaultAsync();

                    var empOffice = new EmployeeOfficeInfo()
                    {
                        EmploymentStatusId = activeStatus != null ? activeStatus.StatusID : null,
                        EmployeeID = employee != null ?  employee.EmployeeID : null,


                    };
                    await _employeeOfficialRepository.AddAsync(empOffice);
                }
                else
                {
                    DateTime? nullableDateTime = DateTime.Now;
                    DateOnly? dateOnly = nullableDateTime.ToDateOnly();

                    employee = await _employeePersonalRepository.AllActive().FirstOrDefaultAsync(e=>e.EmployeeID == model.EmployeeId);

                    if (employee == null)
                    {
                        await _employeeOfficialRepository.RollbackTransactionAsync();

                        result.Success = false;
                        result.Message = "Employee not found.";
                        return result;
                    }

                    employee.FirstName = model.FirstName;
                    employee.LastName = model.LastName;
                    employee.FatherName = model.FatherName;
                    employee.MotherName = model.MotherName;
                    employee.MobileNumber = model.PersonalMobile;
                    employee.Email = model.PersonalEmail;
                    employee.TIN = model.TinNo;
                    employee.DateOfBirth = model.DateOfBirth.ToDateOnly();
                    employee.AboutEmployee = model.AboutEmployee;
                    employee.NID = model.NationalId;
                    employee.State = model.State;
                    employee.City = model.City;
                    employee.HouseNo = model.HouseNo;
                    employee.RoadNo = model.RoadNo;
                    employee.PostalCode = model.PostalCode;
                    

                    employee.MaritalStatusID = Convert.ToInt32(model.MaritalStatus);
                    employee.GenderID = Convert.ToInt32(model.Gender);
                    employee.BloodGroupID = Convert.ToInt32(model.BloodGroup);
                    employee.NationalityID = Convert.ToInt32(nationalityId);
                    employee.ReligionID = Convert.ToInt32(model.Religion);
                    employee.CountryID = Convert.ToInt32(model.Country);
                    employee.UpdatedAt = DateTime.UtcNow;
                    employee.UpdatedBy = model.UpdatedBy;


                    if (employee.BloodGroupID == 0) employee.BloodGroupID = null;
                    if (employee.GenderID == 0) employee.GenderID = null;
                    if (employee.MaritalStatusID == 0) employee.MaritalStatusID = null;
                    if (employee.NationalityID == 0) employee.NationalityID = null;
                    if (employee.ReligionID == 0) employee.ReligionID = null;
                    if (employee.CountryID == 0) employee.CountryID = null;

                    if (model.EmployeePicture != null)
                    {
                        employee.EmployeeImageFileName = EmployeeImageFileName;
                    }

                    if (model.Signature != null)
                    {
                        employee.EmployeeSignatureFileName = EmployeeSignatureFileName;
                    }

                   

                    await _employeePersonalRepository.UpdateAsync(employee);

                }


                await _employeeOfficialRepository.CommitTransactionAsync();

                result.Success = true;
                result.Message = "Employee personal information saved successfully.";
                result.Data = employee.EmployeeID;
                return result;
            }
            catch (Exception ex)
            {
                await _employeeOfficialRepository.RollbackTransactionAsync();

                // Ideally log the error (e.g., with ILogger)
                result.Success = false;
                result.Message = $"An error occurred: {ex.Message}";
                return result;
            }
        }


        private async Task<string> SaveFileAsync(IFormFile file, string folderName)
        {
            if (file == null || file.Length == 0)
                return null;

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", folderName);
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return relative path or full URL depending on preference
            //return Path.Combine(folderName, fileName).Replace("\\", "/");
            return fileName;
        }



        #endregion


        #region Validation


        public async Task<CommonReturnViewModel> CheckValidEmployeeInfo(EmployeePersonalPostViewModel model)
        {
            var result = new CommonReturnViewModel();


            result.Success = true;


            if (
                
                string.IsNullOrEmpty(model.FirstName) ||
                string.IsNullOrEmpty(model.LastName) ||
               
                string.IsNullOrEmpty(model.PersonalMobile) ||
                string.IsNullOrEmpty(model.PersonalEmail)
               

               )
            {
                result.Success = false;
                result.Message = "Please fill out the form.";
            }
            else if (!IsValidEmail(model.PersonalEmail))
            {
                result.Success = false;
                result.Message = "Please enter a valid email address.";
            }

            //else if(model.EmployeePicture.Length > 1048576) // 1MB
            //{
            //    result.Success = false;
            //    result.Message = "Employee Picture size should not exceed 1MB.";
            //}
            //else if (model.Signature.Length > 1048576) // 1MB
            //{
            //    result.Success = false;
            //    result.Message = "Signature size should not exceed 1MB.";
            //}
            //else if(model.EmployeePicture.ContentType != "image/jpeg" && model.EmployeePicture.ContentType != "image/png")
            //{
            //    result.Success = false;
            //    result.Message = "Employee Picture should be in JPEG or PNG format.";
            //}
            //else if (model.Signature.ContentType != "image/jpeg" && model.Signature.ContentType != "image/png")
            //{
            //    result.Success = false;
            //    result.Message = "Signature should be in JPEG or PNG format.";
            //}


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

        private bool CheckDuplicateFields(EmployeePersonalPostViewModel model, out string duplicateMessage)
        {
            List<string> duplicateFields = new List<string>();

            var allEmployees = _employeePersonalRepository.AllActive();

            if (model.EmployeeId > 0)
            {
                allEmployees = allEmployees.Where(e => e.EmployeeID != model.EmployeeId);
            }

            if (model.EmployeeId != null || model.EmployeeId != 0)
            {
                allEmployees = allEmployees.Where(e => e.EmployeeID != model.EmployeeId);
            }

            if (!string.IsNullOrWhiteSpace(model.PersonalEmail) &&
                allEmployees.Any(e => e.Email == model.PersonalEmail))
            {
                duplicateFields.Add("Personal UserEmail");
            }

            if (!string.IsNullOrWhiteSpace(model.PersonalMobile) &&
                allEmployees.Any(e => e.MobileNumber == model.PersonalMobile))
            {
                duplicateFields.Add("Mobile Number");
            }

            if (!string.IsNullOrWhiteSpace(model.TinNo) &&
                allEmployees.Any(e => e.TIN == model.TinNo))
            {
                duplicateFields.Add("TIN");
            }

            if (!string.IsNullOrWhiteSpace(model.NationalId) &&
                allEmployees.Any(e => e.NID == model.NationalId))
            {
                duplicateFields.Add("National ID");
            }

            if (!string.IsNullOrWhiteSpace(model.BirthCertificateNo) &&
                allEmployees.Any(e => e.BirthCertificateNo == model.BirthCertificateNo))
            {
                duplicateFields.Add("Birth Certificate No");
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
            var emailRegex = new Regex(@"^[\w\.-]+@[\w\.-]+\.[a-zA-Z]{2,}$");
            return emailRegex.IsMatch(email);
        }



        #endregion


        #region Get Employee Personal Info by Id

        public async Task<EmployeePersonalGetViewModel> GetEmployeePersonalById(int id)
        {
            var employee = await _employeePersonalRepository.AllActive().Include(e=>e.Country).Include(t=>t.Gender)
                .Include(r=>r.Religion).Include(q=>q.BloodGroup).Include(n=>n.Nationality).Include(m=>m.MaritalStatus)
                .Where(e => e.EmployeeID == id)
                .Select(e => new EmployeePersonalGetViewModel
                {
                    FirstName = e.FirstName ?? "",
                    LastName = e.LastName ?? "",
                    MobileNumber = e.MobileNumber ?? "",
                    Email = e.Email ?? "",
                    FatherName = e.FatherName ?? "",
                    MotherName = e.MotherName ?? "",
                    GenderID = e.GenderID,
                    TIN = e.TIN ?? "",
                    ReligionID = e.ReligionID,
                    DateOfBirth = e.DateOfBirth,
                    BirthCertificateNo = e.BirthCertificateNo ?? "",
                    BloodGroupID = e.BloodGroupID,
                    NationalityID = e.NationalityID,
                  
                    NID = e.NID ?? "",
                    MaritalStatusID = e.MaritalStatusID,
                    AboutEmployee = e.AboutEmployee ?? "",
                    CountryID = e.CountryID,
                    State = e.State ?? "",
                    City = e.City ?? "",
                    HouseNo = e.HouseNo ?? "",
                    RoadNo = e.RoadNo ?? "",
                    PostalCode = e.PostalCode ?? "",
                    EmployeeID = e.EmployeeID,
                    EmployeeImageFileName = e.EmployeeImageFileName ?? "",
                    EmployeeSignatureFileName = e.EmployeeSignatureFileName ?? "",
                    HasUser = e.HasUser,
                    EmployeeCode = e.EmployeeCode ?? "",

                    BloodGroupName = e.BloodGroup != null ? e.BloodGroup.BloodGroupName : "",
                    NationalityName = e.Nationality != null ? e.Nationality.CountryName : "",
                    CountryName = e.Country != null ? e.Country.CountryName : "",
                    Nationality = e.Country != null ? e.Country.CountryName : "",
                    GenderName = e.Gender != null ? e.Gender.GenderName : "",
                    ReligionName = e.Religion != null ? e.Religion.ReligionName : "",
                    MaritalStatusName = e.MaritalStatus != null ? e.MaritalStatus.MaritalStatusName : "",

                })
                .FirstOrDefaultAsync();

            if (employee == null)
            {
                throw new KeyNotFoundException($"Employee with ID {id} not found.");
            }

            return employee;
        }

        #endregion

        #region Get All Employee Personal Info by company


        public async Task<IEnumerable<EmployeePersonalGetViewModel>> GetAllEmployeePersonalByCompanyAsync(int compId)
        {
            try
            {
                // Fetch all active employee personal records
                var employees = await _employeePersonalRepository.AllActive()
                    .Include(e => e.Country)
                    .Include(e => e.Gender)
                    .Include(e => e.Religion)
                    .Include(e => e.BloodGroup)
                    .Include(e => e.Nationality)
                    .Include(e => e.MaritalStatus)
                    .ToListAsync();

                // Fetch all active employee office info records
                var officeInfos = await _employeeOfficialRepository.AllActive().ToListAsync();

                // Perform the join
                var result = from e in employees
                             join o in officeInfos on e.EmployeeID equals o.EmployeeID into joined
                             from office in joined.DefaultIfEmpty()
                             where office != null && office.OrganizationID == compId
                             select new EmployeePersonalGetViewModel
                             {
                                 FirstName = e.FirstName,
                                 LastName = e.LastName,
                                 MobileNumber = e.MobileNumber,
                                 Email = e.Email,
                                 FatherName = e.FatherName,
                                 MotherName = e.MotherName,
                                 GenderID = e.GenderID,
                                 GenderName = e.Gender?.GenderName,
                                 TIN = e.TIN,
                                 ReligionID = e.ReligionID,
                                 ReligionName = e.Religion?.ReligionName,
                                 DateOfBirth = e.DateOfBirth,
                                 BirthCertificateNo = e.BirthCertificateNo,
                                 BloodGroupID = e.BloodGroupID,
                                 BloodGroupName = e.BloodGroup?.BloodGroupName,
                                 NationalityID = e.NationalityID,
                                 NationalityName = e.Nationality?.CountryName,
                                 Nationality = e.Country?.CountryName,
                                 NID = e.NID,
                                 MaritalStatusID = e.MaritalStatusID,
                                 MaritalStatusName = e.MaritalStatus?.MaritalStatusName,
                                 AboutEmployee = e.AboutEmployee,
                                 CountryID = e.CountryID,
                                 CountryName = e.Country?.CountryName,
                                 State = e.State,
                                 City = e.City,
                                 HouseNo = e.HouseNo,
                                 RoadNo = e.RoadNo,
                                 PostalCode = e.PostalCode,
                                 EmployeeID = e.EmployeeID,
                                 EmployeeImageFileName = e.EmployeeImageFileName,
                                 EmployeeSignatureFileName = e.EmployeeSignatureFileName,
                                 HasUser = e.HasUser,
                                 EmployeeCode = e.EmployeeCode,

                                 
                             };

                return result.ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion



        #region GetEmployees
        public async Task<PaginatedResult<CommonSelectVM>> GetEmployees(string search, int page = 1, int pageSize = 50, bool hasEmployeePermission = false, int? empId = null)
        {
            var query = _employeePersonalRepository.AllActive().AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x => x.FirstName.Contains(search) || x.LastName.Contains(search));
            }

            if (!hasEmployeePermission && empId != null)
            {
                query = query.Where(x => x.EmployeeID == empId);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(x => x.FirstName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new CommonSelectVM
                {
                    Id = x.EmployeeID,
                    Name = $"{x.FirstName} {x.LastName}"
                })
                .ToListAsync();

            return new PaginatedResult<CommonSelectVM>
            {
                Items = items,
                HasMore = (page * pageSize) < totalCount
            };
        }

        public async Task<CommonSelectVM> GetEmployeeById(int id)
        {
            var employee = await _employeePersonalRepository.AllActive()
                .AsNoTracking()
                .Where(x => x.EmployeeID == id)
                .Select(x => new CommonSelectVM
                {
                    Id = x.EmployeeID,
                    Name = $"{x.FirstName} {x.LastName}"
                })
                .FirstOrDefaultAsync();
            if (employee != null)
            {
                return employee;
            }
            else
            {
               return new CommonSelectVM();
            }
            
        }
        #endregion


    }
}
