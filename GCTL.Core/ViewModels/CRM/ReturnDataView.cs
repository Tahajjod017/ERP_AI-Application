using GCTL.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.CRM
{
    public class ReturnDataView<T>
    {
        public bool? success { get; set; }
        public string? message{ get; set; }
        public List<T>? data { get; set; } = new List<T>();
       

        public int? totalItem { get; set; }
        public int? totalNowItem { get; set; }
        public int? totalSearchItem { get; set; }
    }
}

