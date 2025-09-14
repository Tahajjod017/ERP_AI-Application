using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.APIViewModels
{
    public class PunchDataVM : BaseViewModel
    {
        public string Time { get; set; }
        public string Label { get; set; }
        public string Icon { get; set; }
        public bool NotPunched { get; set; }
        public bool Deletable { get; set; }
    }
}
