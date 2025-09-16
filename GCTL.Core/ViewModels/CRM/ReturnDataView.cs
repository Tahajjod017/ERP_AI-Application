using GCTL.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.CRM
{
    public class ReturnDataView
    {
        public string? success { get; set; }
        public string? message{ get; set; }
        public List<LeadDetailsDTO>? data { get; set; } = new List<LeadDetailsDTO>();
       

        public int? totalItem { get; set; }
        public int? totalNowItem { get; set; }
        public int? totalSearchItem { get; set; }
    }
}

