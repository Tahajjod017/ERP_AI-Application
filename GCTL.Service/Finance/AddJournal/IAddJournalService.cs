using GCTL.Core.Helpers;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.Finance.AddJournalVM;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.Finance.AddJournal
{
    public interface IAddJournalService
    {
        //Task<GetByIdAddJournalVM> GetByIdAsync(int id);
        Task<PaginationService<Journals, GetAllAddJournalVM>.PaginationResult<GetAllAddJournalVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "JournalID", string sortOrder = "desc");
        Task<GetByIdAddJournalVM> GetJournalDetailsByIdAsync(int id);
        Task<CommonReturnViewModel> AddAsync(CreateAddJournalVM model);
        //Task<CommonReturnViewModel> UpdateAsync(UpdateAddJournalVM model);
        //Task<CommonReturnViewModel> SoftDeleteAsync(DeleteRequestVM requestVM);
        Task<bool> IsCodeUniqueAsync(string code, int? excludeId = null);
        Task<string> GenerateSixDigitCodeAsync();
        Task<GetByPostingRuleIdVM> GetDataByPostingRuleID(int id);
    }
}
