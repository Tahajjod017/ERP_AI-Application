using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.Finance.BaseAccountVM
{
    public class GetByIdBaseAccountVM : BaseViewModel
    {
        public int BaseAccountID { get; set; }

        public string? BaseAccountCode { get; set; }

        public string? BaseAccountName { get; set; }

        public string? Description { get; set; }
    }
}
