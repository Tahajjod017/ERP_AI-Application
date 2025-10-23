using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.Finance.PostingRulesVM;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.Finance.PostingRule
{
    public interface IPostingRulesService
    {
        Task<GetByIdPostingRulesVM> GetByIdAsync(int id);
        Task<PaginationService<PostingRules, GetAllPostingRulesVM>.PaginationResult<GetAllPostingRulesVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "PostingRuleID", string sortOrder = "desc");
        Task<bool> AddAsync(CreatePostingRulesVM model);
        Task<CommonReturnViewModel> UpdateAsync(UpdatePostingRulesVM model);
        Task<bool> IsNameUniqueAsync(string name, int? excludeId = null);
        Task<bool> IsCodeUniqueAsync(string code, int? excludeId = null);
        Task<string> GenerateThreeDigitCodeAsync();
        Task<List<MenuTab>> GetBodyTabsAsync();
    }
}
