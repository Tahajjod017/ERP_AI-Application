using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.CRM;
using GCTL.Core.ViewModels.CRM.AddTeam;
using GCTL.Core.ViewModels.MasterSetup.Grade;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.Pagination;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;


namespace GCTL.Service.CRM.AddTeam
{
    public class AddTeamService : AppService<LeadProjectTeams>, IAddTeamService
    {
        #region Repositories
        private readonly IUserInfoService _userInfoService;
        private readonly IGenericRepository<LeadProjectTeams> _teamsRepository;
        private readonly IGenericRepository<LeadProjectTeamMembers> _teamMembersRepository;
        private readonly IGenericRepository<GCTL.Data.Models.Employees> _employeeRepository;



        public AddTeamService(IGenericRepository<LeadProjectTeams> teamsRepository, IGenericRepository<LeadProjectTeamMembers> teamMembersRepository, IGenericRepository<Customers> employeesRepository, IUserInfoService userInfoService, IGenericRepository<EmployeeOfficeInfo> emplooyeeOfficeInfoRepository, IGenericRepository<Designations> designationRepository, IGenericRepository<ReportContent> reportContent, IGenericRepository<Customers> customerRepository, IGenericRepository<Data.Models.Employees> employeeRepository) : base(teamsRepository)
        {
            _teamsRepository = teamsRepository;
            _teamMembersRepository = teamMembersRepository;
            _userInfoService = userInfoService;
            _employeeRepository = employeeRepository;
        }
        #endregion

        #region GetLastIndexNumber
        //public async Task<int> GetLastIndexNumber()
        //{
        //    int lastID = await _teamsRepository.All().OrderByDescending(t => t.LeadProjectTeamID).Select(t => t.LeadProjectTeamID).FirstOrDefaultAsync() + 1;

        //    return lastID;
        //}
        #endregion

        #region getEmployeeList
        [HttpGet]
        public async Task<ReturnDataView<SelectListItem>> GetEmployees(string search = "", int page = 1, int pageSize = 25)
        {
            var skip = (page - 1) * pageSize;

            var employeesQuery = _employeeRepository.AllActive().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var pattern = $"%{search}%";
                employeesQuery = employeesQuery
                    .Where(e => EF.Functions.Like(e.FirstName + " " + e.LastName, pattern));
            }

            var employees = await employeesQuery
                .OrderBy(e => e.FirstName).ThenBy(e => e.LastName)
                .Skip(skip)
                .Take(pageSize)
                .Select(e => new SelectListItem
                {
                    Value = e.EmployeeID.ToString(),
                    Text = e.FirstName + " " + e.LastName
                })
                .ToListAsync();

            return new ReturnDataView<SelectListItem>
            {
                data = employees,
                success = true,
                totalNowItem = employees.Count,
                totalItem = await employeesQuery.CountAsync()
            };
        }
        #endregion

        #region CreateTeam
        public async Task<ReturnView> CreateTeam(CreateTeamVM createTeamVM)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(createTeamVM.TeamName))
                {
                    return new ReturnView()
                    {
                        Success = false,
                        Message = "please enter a valid team name"
                    };
                }

                var isUnique = await _teamsRepository.FirstOrDefaultAsync(u=> u.LeadProjectTeamName.ToLower() == createTeamVM.TeamName.ToLower());

                if (isUnique != null)
                {
                    return new ReturnView()
                    {
                        Success = false,
                        Message = "This team name already exist"
                    };
                }

                string lastGID = await _teamsRepository.All().OrderByDescending(t => t.LPTeamGID).Select(t => t.LPTeamGID).FirstOrDefaultAsync() ?? "0";
                int lastNumber = 0;
                int.TryParse(lastGID, out lastNumber);
                int newNumber = lastNumber + 1;

                string newGID = newNumber.ToString("D3");

                var LeadProjectTeams = new LeadProjectTeams()
                {
                    LeadProjectTeamName = createTeamVM.TeamName,
                    LPTeamGID = newGID,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = createTeamVM.CreatedBy
                };

                await _teamsRepository.AddAsync(LeadProjectTeams);

                if(createTeamVM.EmployeeIds != null && createTeamVM.EmployeeIds.Any())
                {
                    var members = createTeamVM.EmployeeIds.Select(id => new LeadProjectTeamMembers
                    {
                        LeadProjectTeamID = LeadProjectTeams.LeadProjectTeamID,
                        EmployeeID = id,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = createTeamVM.CreatedBy,
                    }).ToList();

                    await _teamMembersRepository.AddRangeAsync(members);
                }
                return new ReturnView()
                {
                    Success = true,
                    Message = "Team Created"
                };
            }
            catch (Exception ex) {
                return new ReturnView()
                {
                    Success = false,
                    Message = "Something goes to wrong"
                };
            }
        }
        #endregion

        #region GetAllAsync
        public async Task<List<TeamViewVM>> GetTeamListAsync(
         int pageNumber = 1,
         int pageSize = 10,
         string? searchTerm = null,
         string sortColumn = "CreatedAt",
         string sortOrder = "asc")
        {
            // Base query with eager-loading team members and their employees
            var query = _teamsRepository.AllActive()
                        .Include(t => t.LeadProjectTeamMembers)
                        .ThenInclude(m => m.Employee)
                        .AsQueryable();

            // Search by team name
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(t => EF.Functions.Like(t.LeadProjectTeamName, $"%{searchTerm}%"));
            }

            // Sorting
            query = (sortColumn, sortOrder.ToLower()) switch
            {
                ("CreatedAt", "desc") => query.OrderByDescending(t => t.CreatedAt),
                ("CreatedAt", "asc") => query.OrderBy(t => t.CreatedAt),
                _ => query.OrderBy(t => t.CreatedAt)
            };
            query = query.Skip((pageNumber - 1) * pageSize)
                         .Take(pageSize);

            var result = await query
                .Select(t => new TeamViewVM
                {
                    TeamID = t.LeadProjectTeamID,
                    TeamGID = t.LPTeamGID,
                    TeamName = t.LeadProjectTeamName,
                    TeamDetails = t.LeadProjectTeamMembers
                            .Select(m => new TeamDetailsItemVM
                            {
                                TeamMemberName = $"{m.Employee.FirstName} {m.Employee.LastName}",
                                IsTeamHead = m.IsTeamHead,
                            })
                            .ToList()
                })
                .ToListAsync();

            return result;
        }

        #endregion
        
        #region Get Indivudial Team Details
        // for add Team page
        public async Task<TeamEditVM> IndivudialIteamDetails(int id)
        {
            try
            {
                var result = await _teamsRepository.AllActive()
                    .Include(t => t.LeadProjectTeamMembers)
                    .ThenInclude(m => m.Employee)
                    .Where(t => t.LeadProjectTeamID == id)
                    .Select(t => new TeamEditVM
                    {
                        TeamID = t.LeadProjectTeamID,
                        TeamName = t.LeadProjectTeamName,
                        TeamMembersInfo = t.LeadProjectTeamMembers.Select(t => new PertialEditVM
                        {
                            TeamMemberID = t.Employee.EmployeeID,
                            TeamMemberName = $"{t.Employee.FirstName} {t.Employee.LastName}"
                        }).ToList()
                    })
                    .FirstOrDefaultAsync();
                return result;
            }
            catch (Exception ex)
            {
                return new TeamEditVM { };
            }
        }
        #endregion

        #region Get Single Team Information 
        //for team details page
        public async Task<TeamDetailsVM> GetIndividualTeamDetails(int id)
        {
            try
            {
                var result = await _teamsRepository.AllActive()
                    .Include(t => t.LeadProjectTeamMembers)
                    .ThenInclude(m => m.Employee)
                    .Where(t => t.LeadProjectTeamID == id)
                    .Select(t => new TeamDetailsVM
                    {
                        TeamID = t.LeadProjectTeamID,
                        TeamGID = t.LPTeamGID,
                        TeamName = t.LeadProjectTeamName,
                        MemberDetails = t.LeadProjectTeamMembers
                            .Select(m => new TeamMemberDetails
                            {
                                TeamMemberName = $"{m.Employee.FirstName} {m.Employee.LastName}",
                                EmployeeID = m.EmployeeID,
                                Designation = "HRM",
                                //Designation = m.Employee.Designation,
                                MobileNumber = m.Employee.MobileNumber,
                                IsTeamHead = m.IsTeamHead,
                                profileImage = "/img/user.png"
                            })
                            .ToList()
                    })
                    .FirstOrDefaultAsync();

                return result;
            }
            catch (Exception ex)
            {
                // optionally log ex
                return null;
            }
        }

        #endregion

        #region Set Team Head
        public async Task<ReturnView> SetTeamHead(TeamHeadVM teamHeadVM)
        {
            try
            {
                if (teamHeadVM.TeamID == 0 || teamHeadVM.EmployeeID == 0) return new ReturnView { Message = "Team ID or Employee ID is null", Success = false, };

                var exitingObj = await _teamMembersRepository.AllActive().Where(t => t.LeadProjectTeamID == teamHeadVM.TeamID && t.EmployeeID == teamHeadVM.EmployeeID).FirstOrDefaultAsync();
                if (exitingObj == null) { return new ReturnView { Message = "Team not exist", Success = false, }; }

                var teamMembers = await _teamMembersRepository.AllActive().Where(t => t.LeadProjectTeamID == teamHeadVM.TeamID).ToListAsync();

                foreach(var member in teamMembers)
                {
                    member.IsTeamHead = false;
                    member.LMAC = teamHeadVM.LMAC;
                    member.LIP = teamHeadVM.LIP;
                    member.UpdatedAt = DateTime.UtcNow;
                    member.UpdatedBy = teamHeadVM.UpdatedBy;
                }

                exitingObj.IsTeamHead = true;
                exitingObj.LMAC = teamHeadVM.LMAC;
                exitingObj.LIP = teamHeadVM.LIP;
                exitingObj.UpdatedAt = DateTime.UtcNow;
                exitingObj.UpdatedBy = teamHeadVM.UpdatedBy;

                await _teamMembersRepository.UpdateAsync(exitingObj);

                return new ReturnView { Message = "Team Head updated", Success = true, };
            }
            catch (Exception ex)
            {
                return new ReturnView { Message = "Something went to wrong", Success = false, };
            }
            
        }
        #endregion


        //public async Task<List<CommonSelectVM>> GetEmployeesByIds(List<int> ids)
        //{
        //    return await _employeeRepository.All()
        //        .Where(e => ids.Contains(e.EmployeeID))
        //        .Select(e => new CommonSelectVM
        //        {
        //            Id = e.EmployeeID,
        //            Name = e.FirstName + e.LastName,
        //        })
        //        .ToListAsync();
        //}

    }
}

