using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.Finance.AddSubAccountVM
{
    public class GetByIdAddSubAccountVM : BaseViewModel
    {
        public int SubAccountID { get; set; }

        public int? MainAccountID { get; set; }

        public int? ClassID { get; set; }

        public int? GroupID { get; set; }

        public string? MainAccountName { get; set; }

        public string? SubAccountCode { get; set; }

        public string? SubAccountName { get; set; }

        public string? Description { get; set; }
    }
}
