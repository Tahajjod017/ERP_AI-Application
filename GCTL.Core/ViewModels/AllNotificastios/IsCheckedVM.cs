using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AllNotificastios
{
    public class IsCheckedVM:BaseViewModel
    {
        public bool ? IsChecked {  get; set; }
        public int AlertForEmployeeID { get; set; }
    }
}
