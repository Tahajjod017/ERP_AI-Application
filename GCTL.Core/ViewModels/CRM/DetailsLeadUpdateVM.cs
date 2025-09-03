using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.CRM
{
    public class DetailsLeadUpdateVM : BaseViewModel
    {
        public int LeadID { get; set; }
        public string FieldName { get; set; }
        public int FieldValue { get; set; }
    }
}