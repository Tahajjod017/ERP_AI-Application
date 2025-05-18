using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels
{
    public class CommonReturnViewModel
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
        public List<string> Errors { get; set; } // A list of error messages, if any

        public CommonReturnViewModel()
        {
            Errors = new List<string>(); // Initialize the Errors list to avoid null reference
        }
    }
}
