using Microsoft.EntityFrameworkCore;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.CRM;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GCTL.Service.CRM.AddTeam
{
    public class AddTeamService : AppService<LeadProjectTeams>, IAddTeamService
    {
        #region Repositories
        private readonly IUserInfoService _userInfoService;
        private readonly IGenericRepository<LeadProjectTeams> _teamsRepository;
        private readonly IGenericRepository<LeadProjectTeamMembers> _teamMembersRepository;
        private readonly IGenericRepository<Customers> _customerRepository;
        private readonly IGenericRepository<ReportContent> _reportContent;

        public AddTeamService(IGenericRepository<LeadProjectTeams> teamsRepository, IGenericRepository<LeadProjectTeamMembers> teamMembersRepository, IGenericRepository<Customers> employeesRepository, IUserInfoService userInfoService, IGenericRepository<EmployeeOfficeInfo> emplooyeeOfficeInfoRepository, IGenericRepository<Designations> designationRepository, IGenericRepository<ReportContent> reportContent, IGenericRepository<Customers> customerRepository) : base(teamsRepository)
        {
            _teamsRepository = teamsRepository;
            _teamMembersRepository = teamMembersRepository;
            _userInfoService = userInfoService;
            _reportContent = reportContent;
            _customerRepository = customerRepository;
        }
        #endregion


        //#region AddNewTeam
        //public async Task<bool> AddNewTeam(AddTeamVM model)
        //{
        //    await _teamsRepository.BeginTransactionAsync();
        //    try
        //    {
        //        LeadProjectTeams entity = new LeadProjectTeams();
        //        entity.LPTeamGID = model.GeneratedID;
        //        entity.LeadProjectTeamName = model.TeamName;
        //        entity.CreatedAt = DateTime.Now;
        //        entity.CreatedBy = model.CreatedBy;

        //        await _teamsRepository.AddAsync(entity);
        //        //await _userInfoService.ActionLogBVMAsync("Add Team", ActionName.DataAdd, null, entity, entity.LeadProjectTeamID, model);

        //        if (model.EmployeeId != null && model.EmployeeId.Any())
        //        {
        //            foreach (var employeeId in model.EmployeeId)
        //            {
        //                LeadProjectTeamMembers teamMembers = new LeadProjectTeamMembers();
        //                teamMembers.LeadProjectTeamID = entity.LeadProjectTeamID;
        //                teamMembers.EmployeeID = employeeId;
        //                teamMembers.CreatedAt = DateTime.Now;
        //                teamMembers.CreatedBy = model.CreatedBy;

        //                await _teamMembersRepository.AddAsync(teamMembers);
        //                //await _userInfoService.ActionLogBVMAsync("Add Team", ActionName.DataAdd, null, teamMembers, teamMembers.TeamMemberID, model);
        //            }
        //        }
        //        else
        //        {
        //            LeadProjectTeamMembers teamMembers = new LeadProjectTeamMembers();
        //            teamMembers.LeadProjectTeamID = entity.LeadProjectTeamID;
        //            teamMembers.CreatedAt = DateTime.Now;
        //            teamMembers.CreatedBy = model.CreatedBy;
        //            await _teamMembersRepository.AddAsync(teamMembers);
        //            //await _userInfoService.ActionLogBVMAsync("Add Team", ActionName.DataAdd, null, entity, entity.LeadProjectTeamID, model);
        //        }

        //        await _teamsRepository.CommitTransactionAsync();

        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        await _teamsRepository.RollbackTransactionAsync();
        //        return false;
        //    }
        //}
        //#endregion


        //#region UpdateNewTeam
        //public async Task<bool> UpdateNewTeam(AddTeamVM model)
        //{
        //    await _teamsRepository.BeginTransactionAsync();
        //    try
        //    {
        //        var entity = await _teamsRepository.GetByIdAsync(model.TeamID);
        //        if (entity == null)
        //        {
        //            return false;
        //        }

        //        var beforeEntity = JsonConvert.DeserializeObject<AddTeamVM>(JsonConvert.SerializeObject(entity));

        //        entity.LPTeamGID = model.GeneratedID;
        //        entity.LeadProjectTeamName = model.TeamName;
        //        entity.UpdatedAt = DateTime.Now;
        //        entity.UpdatedBy = model.UpdatedBy;
        //        entity.LIP = model.LIP;
        //        entity.LMAC = model.LMAC;

        //        await _teamsRepository.UpdateAsync(entity);
        //        var afterEntity = JsonConvert.DeserializeObject<AddTeamVM>(JsonConvert.SerializeObject(entity));
        //        //await _userInfoService.ActionLogBVMAsync("Add Team", ActionName.DataUpdated, beforeEntity, afterEntity, entity.LeadProjectTeamID, model);

        //        // Soft Remove existing team members
        //        var existingMembers = await _teamMembersRepository.AllActive().Where(x => x.LeadProjectTeamID == model.TeamID).ToListAsync();
        //        if (existingMembers != null && existingMembers.Any())
        //        {
        //            foreach (var member in existingMembers)
        //            {
        //                member.DeletedAt = DateTime.Now;
        //                member.DeletedBy = model.DeletedBy;
        //                member.UpdatedAt = DateTime.Now;
        //                member.UpdatedBy = model.UpdatedBy;
        //            }

        //            await _teamMembersRepository.UpdateRangeAsync(existingMembers);
        //        }

        //        if (model.EmployeeId != null && model.EmployeeId.Any())
        //        {
        //            foreach (var employeeId in model.EmployeeId)
        //            {
        //                // Find previous member to check IsTeamHead
        //                var previousMember = existingMembers.FirstOrDefault(x => x.EmployeeID == employeeId);
        //                var isTeamHead = previousMember?.IsTeamHead ?? false;

        //                // Check for a soft-deleted team member with the same LeadProjectTeamID and EmployeeID
        //                var existingSoftDeleted = await _teamMembersRepository.All().FirstOrDefaultAsync(tm => tm.LeadProjectTeamID == model.TeamID && tm.EmployeeID == employeeId && tm.DeletedAt != null);

        //                if (existingSoftDeleted != null)
        //                {
        //                    // Restore the soft-deleted team member
        //                    existingSoftDeleted.DeletedAt = null;
        //                    existingSoftDeleted.DeletedBy = null;
        //                    existingSoftDeleted.UpdatedAt = DateTime.Now;
        //                    existingSoftDeleted.UpdatedBy = model.UpdatedBy;
        //                    existingSoftDeleted.IsTeamHead = isTeamHead;

        //                    await _teamMembersRepository.UpdateAsync(existingSoftDeleted);
        //                    //await _userInfoService.ActionLogBVMAsync("Action Takens", ActionName.DataUpdated, null, existingSoftDeleted, existingSoftDeleted.TeamMemberID, model);
        //                }
        //                else
        //                {
        //                    // Add new team member
        //                    var newMember = new LeadProjectTeamMembers
        //                    {
        //                        LeadProjectTeamID = model.TeamID,
        //                        EmployeeID = employeeId,
        //                        IsTeamHead = isTeamHead,
        //                        CreatedAt = DateTime.Now,
        //                        CreatedBy = model.UpdatedBy
        //                    };

        //                    await _teamMembersRepository.AddAsync(newMember);
        //                    //await _userInfoService.ActionLogBVMAsync("Action Takens", ActionName.DataUpdated, null, newMember, newMember.TeamMemberID, model);
        //                }
        //            }
        //        }

        //        await _teamsRepository.CommitTransactionAsync();

        //        return true;
        //    }
        //    catch
        //    {
        //        await _teamsRepository.RollbackTransactionAsync();
        //        return false;
        //    }
        //}
        //#endregion


        //#region GetByTeamIdAsync
        //public async Task<List<LeadProjectTeamMembers>> GetByTeamIdAsync(int teamMemberId)
        //{
        //    return await _teamMembersRepository.AllActive().Where(tm => tm.LeadProjectTeamMemberID == teamMemberId).ToListAsync();
        //}
        //#endregion


        //#region DeleteNewTeam
        //public Task<bool> DeleteNewTeam(int id)
        //{
        //    throw new NotImplementedException();
        //}
        //#endregion


        //#region SoftDeleteAsync
        //public Task<bool> SoftDeleteAsync(BaseViewModel model, List<int> ids)
        //{
        //    throw new NotImplementedException();
        //}
        //#endregion


        //#region GetCustomers
        public async Task<IEnumerable<SelectListItem>> GetCustomers()
        {
            var result = await (from emp in _customerRepository.AllActive()

                                    //where !emp.EmployeeCode.StartsWith("DEV")

                                join empOi in _customerRepository.AllActive() on emp.CustomerID equals empOi.CustomerID into empOiGroup
                                from empOi in empOiGroup.DefaultIfEmpty()

                                    //join des in _designationRepository.AllActive() on empOi.DesignationID equals des.DesignationID into desGroup
                                    //from des in desGroup.DefaultIfEmpty()

                                select new SelectListItem
                                {
                                    Value = emp.CustomerID.ToString(),
                                    Text = $"{emp.FullName}"
                                }).ToListAsync();

            return result;
        }
        //#endregion


        //#region GetNewTeam
        //public async Task<AddTeamVM> GetNewTeam(int id)
        //{
        //    try
        //    {
        //        var data = await _teamsRepository.AllActive().Include(x => x.LeadProjectTeamMembers).FirstOrDefaultAsync(x => x.LeadProjectTeamID == id);
        //        if (data == null) return null;

        //        var employeeIds = data.LeadProjectTeamMembers?
        //            .Where(tm => tm.EmployeeID.HasValue && tm.DeletedAt == null && tm.DeletedBy == null)
        //            .Select(tm => tm.EmployeeID.Value)
        //            .ToList();

        //        return new AddTeamVM
        //        {
        //            GeneratedID = data.GeneratedID,
        //            TeamID = data.LeadProjectTeamID,
        //            TeamName = data.TeamName,
        //            EmployeeId = employeeIds ?? new List<int>() // Prevent null reference
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        throw;
        //    }
        //}
        //#endregion


        //#region GetAll
        //public async Task<IEnumerable<AddTeamVM>> GetAll()
        //{
        //    var teams = await _teamsRepository.AllActive()
        //        .Include(team => team.LeadProjectTeamMembers)
        //        .ThenInclude(x => x.Employee)
        //        .ToListAsync();

        //    var result = teams.Select(team =>
        //    {
        //        var activeMembers = team.LeadProjectTeamMembers
        //            .Where(tm => tm.DeletedAt == null)
        //            .OrderByDescending(tm => tm.IsTeamHead)
        //            .ToList();

        //        return new AddTeamVM
        //        {
        //            GeneratedID = team.GeneratedID,
        //            TeamID = team.LeadProjectTeamID,
        //            TeamName = team.TeamName,
        //            EmployeeId = activeMembers.Select(tm => tm.EmployeeID ?? 0).ToList(),
        //            TeamMembersVMs = activeMembers.Select(tm => new TeamMembersVM
        //            {
        //                LeadProjectTeamID = tm.LeadProjectTeamID,
        //                EmployeeID = tm.EmployeeID,
        //                EmployeeName = $"{tm.Employee.FirstName} {tm.Employee.LastName}",
        //                IsTeamHead = tm.IsTeamHead,
        //                CreatedAt = tm.CreatedAt,
        //                CreatedBy = tm.CreatedBy
        //            }).ToList()
        //        };
        //    }).ToList();

        //    return result;
        //}
        //#endregion


        //#region IsNameUniqueAsync
        //public async Task<bool> IsNameUniqueAsync(string name)
        //{
        //    var existingName = await _teamsRepository.FindAsync(b => b.LeadProjectTeamName == name);
        //    return !existingName.Any();
        //}
        //#endregion


        //#region IP & Mac Address
        //public string GetLocalIP()
        //{
        //    var host = Dns.GetHostEntry(Dns.GetHostName());
        //    foreach (var ip in host.AddressList)
        //    {
        //        if (ip.AddressFamily == AddressFamily.InterNetwork)
        //        {
        //            return ip.ToString();
        //        }
        //    }
        //    return string.Empty;
        //}

        //public string GetMacAddress()
        //{
        //    var nics = NetworkInterface.GetAllNetworkInterfaces();
        //    var macAddress = string.Empty;
        //    foreach (var adapter in nics)
        //    {
        //        if (adapter.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
        //        {
        //            macAddress = adapter.GetPhysicalAddress().ToString();
        //            break;
        //        }
        //    }
        //    return macAddress;
        //}
        //#endregion


        //#region GenerateNextCodeAsync
        //public async Task<string> GenerateNextCodeAsync()
        //{
        //    var entity = await _teamsRepository.GetAllAsync();
        //    var lastCode = entity.Max(b => b.GeneratedID);
        //    int nextCode = 1;
        //    if (!string.IsNullOrEmpty(lastCode))
        //    {
        //        int lastNumber = int.Parse(lastCode.TrimStart('0'));
        //        lastNumber++;
        //        nextCode = lastNumber;
        //    }
        //    return nextCode.ToString("D3");
        //}
        //#endregion


        //#region GetReportData
        //public async Task<List<AddTeamVM>> GetReportData()
        //{
        //    var query = _teamsRepository.AllActive().Include(x => x.LeadProjectTeamMembers).ThenInclude(x => x.Employee);

        //    var data = await query.Select(x => new AddTeamVM
        //    {
        //        LeadProjectTeamID = x.LeadProjectTeamID,
        //        GeneratedID = x.GeneratedID ?? "-",
        //        TeamName = x.TeamName ?? "-",
        //        TeamMembersVMs = x.LeadProjectTeamMembers.Select(tm => new TeamMembersVM
        //        {
        //            TeamMemberID = tm.TeamMemberID,
        //            EmployeeID = tm.EmployeeID,
        //            EmployeeCode = tm.Employee.EmployeeCode,
        //            EmployeeName = $"{tm.Employee.FirstName} {tm.Employee.LastName}" ?? "-",
        //            IsTeamHead = tm.IsTeamHead
        //        }).ToList(),
        //    }).ToListAsync();

        //    return data;
        //}
        //#endregion


        //#region GenerateExcelReportAsync
        //public async Task<byte[]> GenerateExcelReportAsync(IEnumerable<AddTeamVM> data)
        //{
        //    var header = await _reportContent.AllActive().FirstOrDefaultAsync();

        //    using var workbook = new XLWorkbook();
        //    var worksheet = workbook.Worksheets.Add("AddTeam");

        //    // Header Lines (Dynamic from DB)
        //    worksheet.Cell("A1").Value = header?.Title ?? "";
        //    worksheet.Cell("A2").Value = header?.SubTitle1 ?? "";
        //    worksheet.Cell("A3").Value = header?.SubTitle2 ?? "";
        //    worksheet.Cell("A4").Value = header?.Address ?? "";

        //    for (int i = 1; i <= 4; i++)
        //    {
        //        var range = worksheet.Range($"A{i}:E{i}");
        //        range.Merge();
        //        range.Style.Font.Bold = true;
        //        range.Style.Font.FontSize = 14;
        //        range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        //    }

        //    // Blank row (A5)
        //    worksheet.Range("A5:E5").Merge();

        //    // Header (Row 6)
        //    worksheet.Cell(6, 1).Value = "Team ID";
        //    worksheet.Cell(6, 2).Value = "Team Name";
        //    worksheet.Cell(6, 3).Value = "Employee ID";
        //    worksheet.Cell(6, 4).Value = "Employee Name";
        //    worksheet.Cell(6, 5).Value = "Is Team Head";

        //    worksheet.Range("A6:E6").Style.Font.Bold = true;

        //    int currentRow = 7;

        //    foreach (var team in data)
        //    {
        //        int startRow = currentRow;
        //        int memberCount = team.TeamMembersVMs?.Count ?? 0;

        //        if (memberCount > 0)
        //        {
        //            foreach (var member in team.TeamMembersVMs)
        //            {
        //                worksheet.Cell(currentRow, 3).Value = member.EmployeeCode;
        //                worksheet.Cell(currentRow, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

        //                worksheet.Cell(currentRow, 4).Value = member.EmployeeName;
        //                worksheet.Cell(currentRow, 5).Value = (bool)member.IsTeamHead ? "Team Head" : "-";
        //                currentRow++;
        //            }

        //            // Merge LeadProjectTeamID and TeamName cells vertically
        //            worksheet.Range(startRow, 1, currentRow - 1, 1).Merge().Value = team.LeadProjectTeamID;
        //            worksheet.Range(startRow, 2, currentRow - 1, 2).Merge().Value = team.TeamName;

        //            // Center align vertically
        //            worksheet.Range(startRow, 1, currentRow - 1, 2).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        //        }
        //        else
        //        {
        //            worksheet.Cell(currentRow, 1).Value = team.LeadProjectTeamID;
        //            worksheet.Cell(currentRow, 2).Value = team.TeamName;
        //            worksheet.Cell(currentRow, 3).Value = "-";
        //            worksheet.Cell(currentRow, 4).Value = "-";
        //            worksheet.Cell(currentRow, 5).Value = "-";
        //            currentRow++;
        //        }
        //    }


        //    // Adjust column widths
        //    worksheet.Column(1).Width = 10;
        //    worksheet.Column(2).Width = 30;
        //    worksheet.Column(3).Width = 15;
        //    worksheet.Column(4).Width = 30;
        //    worksheet.Column(5).Width = 15;

        //    using var stream = new MemoryStream();
        //    workbook.SaveAs(stream);
        //    return stream.ToArray();
        //}
        //#endregion
    }
}
