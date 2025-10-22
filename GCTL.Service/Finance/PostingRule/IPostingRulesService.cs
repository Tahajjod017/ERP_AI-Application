using GCTL.Core.ViewModels.Finance.PostingRulesVM;
using GCTL.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.Finance.PostingRule
{
    public interface IPostingRulesService
    {
        Task<List<MenuTab>> GetBodyTabsAsync();
        Task<bool> AddAsync(CreatePostingRulesVM model);
        Task<bool> IsNameUniqueAsync(string name, int? excludeId = null);
        Task<bool> IsCodeUniqueAsync(string code, int? excludeId = null);
        Task<string> GenerateThreeDigitCodeAsync();
    }
}
