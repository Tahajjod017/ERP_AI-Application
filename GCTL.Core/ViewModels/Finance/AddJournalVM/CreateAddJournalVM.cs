using GCTL.Core.ViewModels.Finance.JournalDetailsVM;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.Finance.AddJournalVM
{
    public class CreateAddJournalVM : BaseViewModel
    {
        public int JournalID { get; set; }

        [Required(ErrorMessage = "{0} is Required!"), Display(Name = "Journal Type")]
        public int? JournalTypeID { get; set; }

        [Required(ErrorMessage = "{0} is Required!"), Display(Name = "Journal Code")]
        public string JournalCode { get; set; }

        [Required(ErrorMessage = "{0} is Required!"), Display(Name = "Scenario Type")]
        public int? PostingRuleID { get; set; }

        [Required(ErrorMessage = "{0} is Requird!"), Display(Name = "Transaction Year")]
        public int? FinancialYearID { get; set; }

        [Required(ErrorMessage = "{0} is Required!"), Display(Name = "Date")]
        public DateTime? JournalDate { get; set; }
        
        public string? Note { get; set; }

        [Display(Name = "Attach File")]
        public IFormFile? FileLink { get; set; }

        [Required(ErrorMessage = "Journal details is Required!")]
        public IList<CreateJournalDetailsVM?> CreateJournalDetailsVMs { get; set; } = new List<CreateJournalDetailsVM?>();
    }
}
