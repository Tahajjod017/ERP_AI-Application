using GCTL.Data.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.MasterSetup.ActionTakens
{
    public class ActionTakenVM : BaseViewModel
    {
        public int ActionTakenID { get; set; }

        [Required(ErrorMessage = "{0} is Required!"), DisplayName("Action Taken Name")]
        public string ActionTakenName { get; set; }

        //public virtual ICollection<ActionTackenByCircle> ActionTackenByCircle { get; set; } = new List<ActionTackenByCircle>();

        //public virtual ICollection<ActionTackenByITIIU> ActionTackenByITIIU { get; set; } = new List<ActionTackenByITIIU>();
    }
}
