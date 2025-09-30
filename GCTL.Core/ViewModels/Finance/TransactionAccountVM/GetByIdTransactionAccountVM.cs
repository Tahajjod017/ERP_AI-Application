using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.Finance.TransactionAccountVM
{
    public class GetByIdTransactionAccountVM : BaseViewModel
    {
        public int TrxAccID { get; set; }

        public int? SubAccountID { get; set; }

        public string? TrxAccCode { get; set; }

        public string? TrxAccName { get; set; }

        public bool IsActive { get; set; }

        public string? Description { get; set; }
    }
}
