using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.Finance.ThirdTabVM
{
    public class GetAllThirdTabVM : BaseViewModel
    {
        public int GroupID { get; set; }

        public int? ClassID { get; set; }

        public string? ClassName { get; set; }

        public string? GroupCode { get; set; }

        public string? GroupName { get; set; }

        public string? Description { get; set; }

        public int? BaseAccountID { get; set; }

        public string? BaseAccountName { get; set; }
    }
}
