using GCTL.Core.ViewModels.Employee.EmployeeStatusManagement.Promotion;
using System.Text;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using GCTL.Core.Repository;
using GCTL.Data.Models;
using GCTL.Service.Employees.EmployeeStatus.Promotion;

namespace GCTL_App.Controllers.Employees.EmployeeStatusManagement.PromotionController
{
    public class PromotionApproveController : BaseController
    {

        private readonly IGenericRepository<GCTL.Data.Models.Employees> _employeeRepository;
        private readonly IGenericRepository<EmployeeOfficeInfo> _employeeOffiRepository;
        private readonly IGenericRepository<Organization> _organizationRepository;
        private readonly IGenericRepository<Departments> _departmentRepository;
        private readonly IGenericRepository<Designations> _designationRepository;
        private readonly IGenericRepository<EmployeeCareerChanges> _empCarrerRepository;
        private readonly IGenericRepository<EmployeeActionTypes> _empActionRepository;
        private readonly IPromotionService _promotionService;

        public PromotionApproveController(ITranslateService translateService, IUserProfileService userProfileService, IGenericRepository<GCTL.Data.Models.Employees> employeeRepository, IGenericRepository<EmployeeOfficeInfo> employeeOffiRepository, IGenericRepository<Organization> organizationRepository, IGenericRepository<Departments> departmentRepository, IGenericRepository<Designations> designationRepository, IGenericRepository<EmployeeCareerChanges> empCarrerRepository, IGenericRepository<EmployeeActionTypes> empActionRepository, IPromotionService promotionService) : base(translateService, userProfileService)
        {
            _employeeRepository = employeeRepository;
            _employeeOffiRepository = employeeOffiRepository;
            _organizationRepository = organizationRepository;
            _departmentRepository = departmentRepository;
            _designationRepository = designationRepository;
            _empCarrerRepository = empCarrerRepository;
            _empActionRepository = empActionRepository;
            _promotionService = promotionService;
        }

        public IActionResult Index()
        {
            SetSmartPageCode(121900);

            return View();
        }

        #region Static Data for Testing
        private static readonly List<PromotionApproveViewModel> PendingPromotions = new List<PromotionApproveViewModel>
        {
            new PromotionApproveViewModel
            {
                Id = 1,
                EmployeeName = "Hasan Shikder",
                Department = "Development",
                CurrentPosition = "Junior Developer",
                ProposedPosition = "Senior Developer",
                CurrentSalary = "৳45,000",
                ProposedSalary = "৳55,000",
                EffectiveDate = "01 Jan 2024",
                YearsOfExperience = "3 Years",
                Justification = "Employee has consistently exceeded performance targets and demonstrated leadership qualities. Has successfully led 3 major projects and mentored junior developers.",
                AvatarUrl = "../../assets/img/users/user-01.jpg",
                Status = "Pending"
            },
            new PromotionApproveViewModel
            {
                Id = 2,
                EmployeeName = "Momi Mia",
                Department = "Administration",
                CurrentPosition = "Office Staff",
                ProposedPosition = "Senior Officer",
                CurrentSalary = "৳25,000",
                ProposedSalary = "৳32,000",
                EffectiveDate = "15 Jan 2024",
                YearsOfExperience = "2 Years",
                Justification = "Outstanding administrative support and improved office efficiency.",
                AvatarUrl = "../../assets/img/users/user-01.jpg",
                Status = "Pending"
            },
            new PromotionApproveViewModel
            {
                Id = 3,
                EmployeeName = "Nojibul Hasan",
                Department = "IT Support",
                CurrentPosition = "IT Support",
                ProposedPosition = "Team Lead",
                CurrentSalary = "৳35,000",
                ProposedSalary = "৳45,000",
                EffectiveDate = "19 Feb 2024",
                YearsOfExperience = "4 Years",
                Justification = "Proven technical expertise and leadership in IT support team.",
                AvatarUrl = "../../assets/img/users/user-01.jpg",
                Status = "Pending"
            },
            new PromotionApproveViewModel
            {
                Id = 4,
                EmployeeName = "Aminul Islam",
                Department = "Sales",
                CurrentPosition = "Senior Executive",
                ProposedPosition = "Manager",
                CurrentSalary = "৳55,000",
                ProposedSalary = "৳68,000",
                EffectiveDate = "01 Sep 2024",
                YearsOfExperience = "5 Years",
                Justification = "Exceptional sales performance and client relationship management.",
                AvatarUrl = "../../assets/img/users/user-01.jpg",
                Status = "Pending"
            },
            new PromotionApproveViewModel
            {
                Id = 5,
                EmployeeName = "Faruk Hasan",
                Department = "Finance",
                CurrentPosition = "Assistant Manager",
                ProposedPosition = "Finance Manager",
                CurrentSalary = "৳75,000",
                ProposedSalary = "৳95,000",
                EffectiveDate = "25 Dec 2024",
                YearsOfExperience = "6 Years",
                Justification = "Strategic financial planning and cost optimization achievements.",
                AvatarUrl = "../../assets/img/users/user-01.jpg",
                Status = "Pending"
            }
        };

        private static readonly List<PromotionApproveViewModel> ApprovedPromotions = new List<PromotionApproveViewModel>
        {
            new PromotionApproveViewModel
            {
                Id = 6,
                EmployeeName = "Galib",
                Department = "Developer",
                CurrentPosition = "Developer",
                ProposedPosition = "Senior Developer",
                CurrentSalary = "৳50,000",
                ProposedSalary = "৳60,000",
                EffectiveDate = "01 Dec 2023",
                YearsOfExperience = "4 Years",
                Justification = "Led critical development projects with high efficiency.",
                AvatarUrl = "../../assets/img/users/user-01.jpg",
                Status = "Approved"
            },
            new PromotionApproveViewModel
            {
                Id = 7,
                EmployeeName = "Korim",
                Department = "Finance",
                CurrentPosition = "Analyst",
                ProposedPosition = "Senior Analyst",
                CurrentSalary = "৳40,000",
                ProposedSalary = "৳50,000",
                EffectiveDate = "15 Nov 2023",
                YearsOfExperience = "3 Years",
                Justification = "Improved financial reporting accuracy.",
                AvatarUrl = "../../assets/img/users/user-01.jpg",
                Status = "Approved"
            }
        };
        #endregion

        [HttpGet]
        public IActionResult GetPromotionCards()
        {
            var cards = new
            {
                SeniorCount = 85,
                SeniorPending = 12,
                TeamLeadCount = 42,
                TeamLeadPending = 6,
                ManagerCount = 28,
                ManagerPending = 8,
                DeptHeadCount = 15,
                DeptHeadPending = 3
            };
            return Ok(cards);
        }


        [HttpPost]
        public async Task<IActionResult> GetPendingPromotions([FromForm] PromotionFilterModel filter)
        {
            try
            {
                var imgLink = GetEmployeePictureURL(true);

                var result = await _promotionService.GetFilteredPromotionsAsync(filter, imgLink);
                return Ok(result);
            }
            catch (Exception ex)
            {
               
                return StatusCode(500, "An error occurred while processing your request");
            }
        }

        //[HttpPost]
        //public IActionResult GetPendingPromotions([FromForm] PromotionFilterModel filter)
        //{
        //       //var promotions = PendingPromotions.AsQueryable();
        //    var promotions = _promotionService.GetAllPromotionPendingList().Result;

        //    // Apply filters
        //    if (!string.IsNullOrEmpty(filter.PromotionType))
        //    {
        //        promotions = promotions.Where(p => p.ProposedPosition.Contains(filter.PromotionType, StringComparison.OrdinalIgnoreCase));
        //    }
        //    if (!string.IsNullOrEmpty(filter.Status))
        //    {
        //        promotions = promotions.Where(p => p.Status == filter.Status);
        //    }
        //    if (!string.IsNullOrEmpty(filter.DateRange))
        //    {
        //        var dates = filter.DateRange.Split(" to ");
        //        if (dates.Length == 2 && DateTime.TryParse(dates[0], out var startDate) && DateTime.TryParse(dates[1], out var endDate))
        //        {
        //            promotions = promotions.Where(p => DateTime.Parse(p.EffectiveDate) >= startDate && DateTime.Parse(p.EffectiveDate) <= endDate);
        //        }
        //    }

        //    // Apply sorting
        //    if (!string.IsNullOrEmpty(filter.SortBy))
        //    {
        //        switch (filter.SortBy)
        //        {
        //            case "Recently Added":
        //                promotions = promotions.OrderByDescending(p => p.Id);
        //                break;
        //            case "Ascending":
        //                promotions = promotions.OrderBy(p => p.EmployeeName);
        //                break;
        //            case "Descending":
        //                promotions = promotions.OrderByDescending(p => p.EmployeeName);
        //                break;
        //            case "Last Month":
        //                promotions = promotions.Where(p => DateTime.Parse(p.EffectiveDate) >= DateTime.Now.AddMonths(-1));
        //                break;
        //            case "Last 7 days":
        //                promotions = promotions.Where(p => DateTime.Parse(p.EffectiveDate) >= DateTime.Now.AddDays(-7));
        //                break;
        //        }
        //    }

        //    // Apply sorting
        //    if (!string.IsNullOrEmpty(filter.SortColumn))
        //    {
        //        bool isAscending = filter.SortDirection?.ToLower() != "desc";
        //        switch (filter.SortColumn)
        //        {
        //            case "employeeName":
        //                promotions = isAscending ? promotions.OrderBy(p => p.EmployeeName) : promotions.OrderByDescending(p => p.EmployeeName);
        //                break;
        //            case "currentPosition":
        //                promotions = isAscending ? promotions.OrderBy(p => p.CurrentPosition) : promotions.OrderByDescending(p => p.CurrentPosition);
        //                break;
        //            case "proposedPosition":
        //                promotions = isAscending ? promotions.OrderBy(p => p.ProposedPosition) : promotions.OrderByDescending(p => p.ProposedPosition);
        //                break;
        //            case "currentSalary":
        //                promotions = isAscending ? promotions.OrderBy(p => p.CurrentSalary) : promotions.OrderByDescending(p => p.CurrentSalary);
        //                break;
        //            case "proposedSalary":
        //                promotions = isAscending ? promotions.OrderBy(p => p.ProposedSalary) : promotions.OrderByDescending(p => p.ProposedSalary);
        //                break;
        //            case "effectiveDate":
        //                promotions = isAscending ? promotions.OrderBy(p => DateTime.Parse(p.EffectiveDate)) : promotions.OrderByDescending(p => DateTime.Parse(p.EffectiveDate));
        //                break;
        //            default:
        //                promotions = promotions.OrderBy(p => p.Id);
        //                break;
        //        }
        //    }

        //    // Apply pagination
        //    int page = filter.Page > 0 ? filter.Page : 1;
        //    int pageSize = filter.PageSize > 0 ? filter.PageSize : 10;
        //    var totalItems = promotions.Count();
        //    var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
        //    var paginatedPromotions = promotions.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        //    //return Ok(new { TotalPages = totalPages, Promotions = paginatedPromotions });
        //    return Ok(new { TotalPages = totalPages, TotalItems = totalItems, Promotions = paginatedPromotions });
        //}


        [HttpPost]
        public async Task<IActionResult> GetApprovedPromotions([FromForm] PromotionFilterModel filter)
        {
            try
            {
                var imgLink = GetEmployeePictureURL(true);
                var result = await _promotionService.GetFilteredApprovePromotionsAsync(filter , imgLink);
                return Ok(result);
            }
            catch (Exception ex)
            {

                return StatusCode(500, "An error occurred while processing your request");
            }

            //var promotions = ApprovedPromotions.AsQueryable();

            //// Apply filters
            //if (!string.IsNullOrEmpty(filter.Department))
            //{
            //    promotions = promotions.Where(p => p.Department.Contains(filter.Department, StringComparison.OrdinalIgnoreCase));
            //}
            //if (!string.IsNullOrEmpty(filter.Employee))
            //{
            //    promotions = promotions.Where(p => p.EmployeeName.Contains(filter.Employee, StringComparison.OrdinalIgnoreCase));
            //}
            //if (!string.IsNullOrEmpty(filter.PromotionType))
            //{
            //    promotions = promotions.Where(p => p.ProposedPosition.Contains(filter.PromotionType, StringComparison.OrdinalIgnoreCase));
            //}
            //if (!string.IsNullOrEmpty(filter.DateRange))
            //{
            //    var dates = filter.DateRange.Split(" to ");
            //    if (dates.Length == 2 && DateTime.TryParse(dates[0], out var startDate) && DateTime.TryParse(dates[1], out var endDate))
            //    {
            //        promotions = promotions.Where(p => DateTime.Parse(p.EffectiveDate) >= startDate && DateTime.Parse(p.EffectiveDate) <= endDate);
            //    }
            //}

            //// Apply sorting
            //if (!string.IsNullOrEmpty(filter.SortBy))
            //{
            //    switch (filter.SortBy)
            //    {
            //        case "Recently Added":
            //            promotions = promotions.OrderByDescending(p => p.Id);
            //            break;
            //        case "Ascending":
            //            promotions = promotions.OrderBy(p => p.EmployeeName);
            //            break;
            //        case "Descending":
            //            promotions = promotions.OrderByDescending(p => p.EmployeeName);
            //            break;
            //        case "Last Month":
            //            promotions = promotions.Where(p => DateTime.Parse(p.EffectiveDate) >= DateTime.Now.AddMonths(-1));
            //            break;
            //        case "Last 7 days":
            //            promotions = promotions.Where(p => DateTime.Parse(p.EffectiveDate) >= DateTime.Now.AddDays(-7));
            //            break;
            //    }
            //}

            //// Apply sorting
            //if (!string.IsNullOrEmpty(filter.SortColumn))
            //{
            //    bool isAscending = filter.SortDirection?.ToLower() != "desc";
            //    switch (filter.SortColumn)
            //    {
            //        case "employeeName":
            //            promotions = isAscending ? promotions.OrderBy(p => p.EmployeeName) : promotions.OrderByDescending(p => p.EmployeeName);
            //            break;
            //        case "currentPosition":
            //            promotions = isAscending ? promotions.OrderBy(p => p.CurrentPosition) : promotions.OrderByDescending(p => p.CurrentPosition);
            //            break;
            //        case "proposedPosition":
            //            promotions = isAscending ? promotions.OrderBy(p => p.ProposedPosition) : promotions.OrderByDescending(p => p.ProposedPosition);
            //            break;
            //        case "currentSalary":
            //            promotions = isAscending ? promotions.OrderBy(p => p.CurrentSalary) : promotions.OrderByDescending(p => p.CurrentSalary);
            //            break;
            //        case "proposedSalary":
            //            promotions = isAscending ? promotions.OrderBy(p => p.ProposedSalary) : promotions.OrderByDescending(p => p.ProposedSalary);
            //            break;
            //        case "effectiveDate":
            //            promotions = isAscending ? promotions.OrderBy(p => DateTime.Parse(p.EffectiveDate)) : promotions.OrderByDescending(p => DateTime.Parse(p.EffectiveDate));
            //            break;
            //        default:
            //            promotions = promotions.OrderBy(p => p.Id);
            //            break;
            //    }
            //}

            //// Apply pagination
            //int page = filter.Page > 0 ? filter.Page : 1;
            //int pageSize = filter.PageSize > 0 ? filter.PageSize : 10;
            //var totalItems = promotions.Count();
            //var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            //var paginatedPromotions = promotions.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ////return Ok(new { TotalPages = totalPages, Promotions = paginatedPromotions });
            //return Ok(new { TotalPages = totalPages, TotalItems = totalItems, Promotions = paginatedPromotions });
        }

        
        [HttpGet]
        public async Task<IActionResult> GetPromotionDetails(int id)
        {

            PromotionApproveViewModel promotion = await _promotionService.GetPendingPromotionDetailsByID(id);

            //var promotion = PendingPromotions.Concat(ApprovedPromotions).FirstOrDefault(p => p.Id == id);
            if (promotion == null)
            {
                return NotFound();
            }
            return Ok(promotion);
        }

        
        [HttpPost]
        public async Task< IActionResult> PerformPromotionAction([FromForm] PromotionActionModel action1)
        {
           
            var result = await _promotionService.ApprovePromotionAsync(action1);
            return Ok(result);
        }

        // POST: api/promotions/export
        [HttpPost]
        public IActionResult ExportPromotions([FromForm] string format)
        {
            var promotions = PendingPromotions.Concat(ApprovedPromotions).ToList();
            byte[] fileBytes;
            string contentType;
            string fileName;

            if (format == "pdf")
            {
                // Simulate PDF generation (in a real app, use a library like iTextSharp or PdfSharp)
                var sb = new StringBuilder();
                sb.AppendLine("%PDF-1.4");
                sb.AppendLine("1 0 obj");
                sb.AppendLine("<< /Type /Catalog /Pages 2 0 R >>");
                sb.AppendLine("endobj");
                sb.AppendLine("2 0 obj");
                sb.AppendLine("<< /Type /Pages /Kids [3 0 R] /Count 1 >>");
                sb.AppendLine("endobj");
                sb.AppendLine("3 0 obj");
                sb.AppendLine("<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Contents 4 0 R >>");
                sb.AppendLine("endobj");
                sb.AppendLine("4 0 obj");
                sb.AppendLine("<< /Length 44 >>");
                sb.AppendLine("stream");
                sb.AppendLine("BT /F1 12 Tf 100 700 Td (Promotions Report) Tj ET");
                sb.AppendLine("endstream");
                sb.AppendLine("endobj");
                fileBytes = Encoding.UTF8.GetBytes(sb.ToString());
                contentType = "application/pdf";
                fileName = "promotions.pdf";
            }
            else if (format == "excel")
            {
                // Simulate Excel generation (in a real app, use a library like EPPlus)
                var sb = new StringBuilder();
                sb.AppendLine("Employee Name,Department,Current Position,Proposed Position,Current Salary,Proposed Salary,Effective Date,Status");
                foreach (var p in promotions)
                {
                    sb.AppendLine($"{p.EmployeeName},{p.Department},{p.CurrentPosition},{p.ProposedPosition},{p.CurrentSalary},{p.ProposedSalary},{p.EffectiveDate},{p.Status}");
                }
                fileBytes = Encoding.UTF8.GetBytes(sb.ToString());
                contentType = "text/csv";
                fileName = "promotions.csv";
            }
            else
            {
                return BadRequest("Invalid format");
            }

            return File(fileBytes, contentType, fileName);
        }
    }
}
