using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.MasterSetup.Designations
{
    public class DesignationVM : BaseViewModel
    {
        public int DesignationID { get; set; }
        public int? OrganizationID { get; set; }

        public string DesignationName { get; set; }
        public string? OrganizationName { get; set; }

    }
}
