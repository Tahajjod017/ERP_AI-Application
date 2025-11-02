using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.MasterSetup.OrganizationTypes
{
    public class OrganizationTypeVM : BaseViewModel
    {

        public int Id { get; set; }

        public string TypeName { get; set; }

        public string UseFor { get; set; }
    }
}
