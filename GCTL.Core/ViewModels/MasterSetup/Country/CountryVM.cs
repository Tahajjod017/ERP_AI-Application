using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.MasterSetup.Country
{
    public class CountryVM : BaseViewModel
    {
        public int CountryID { get; set; }

        public string? CountryCode { get; set; }

        public string CountryName { get; set; }
    }
}
