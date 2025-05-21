using GCTL.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.Helpers
{
    public class DeleteRequestVM : BaseViewModel
    {
        public List<int> Ids { get; set; } = new List<int>();
    }

}
