using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.Finance.SecondTabVM
{
    public class GetAllSecondTabVM : BaseViewModel
    {
        public int ClassID { get; set; }

        public string? ClassCode { get; set; }

        public string? ClassName { get; set; }

        public string? Description { get; set; }

        public int? BaseAccountID { get; set; }

        public string? BaseAccountName { get; set; }
    }
}
