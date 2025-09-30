using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.Finance.AddMainAccountVM
{
    public class GetByIdAddMainAccountVM : BaseViewModel
    {
        public int MainAccountID { get; set; }

        public int? GroupID { get; set; }

        public string? GroupName { get; set; }

        public string? MainAccountCode { get; set; }

        public string? MainAccountName { get; set; }

        public string? Description { get; set; }
    }
}
