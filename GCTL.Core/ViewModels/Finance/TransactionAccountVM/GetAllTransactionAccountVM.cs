using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.Finance.TransactionAccountVM
{
    public class GetAllTransactionAccountVM : BaseViewModel
    {
        public int TrxAccID { get; set; }

        public int? MainAccountID { get; set; }

        public string? MainAccountName { get; set; }

        public int? GroupID { get; set; }

        public string? GroupName { get; set; }

        public int? ClassID { get; set; }

        public string? ClassName { get; set; }

        public int? SubAccountID { get; set; }

        public string? SubAccountName { get; set; }

        public string? TrxAccCode { get; set; }

        public string? TrxAccName { get; set; }

        public bool IsActive { get; set; }

        public string? Description { get; set; }
    }
}
