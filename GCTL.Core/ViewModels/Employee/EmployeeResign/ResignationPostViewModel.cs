using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.Employee.EmployeeResign
{
    public class ResignationPostViewModel : BaseViewModel
    {
   
        public int? Id { get; set; } // null for create
       

        [Required(ErrorMessage = "Company is required")]
        public int CompanyId { get; set; }

        [Required(ErrorMessage = "Employee is required")]
        public int EmployeeId { get; set; }

        [Required(ErrorMessage = "Notice Date is required")]
        public string? NoticeDate { get; set; }

        [Required(ErrorMessage = "Resignation Date is required")]
        public string? ResignationDate { get; set; }

        [Required(ErrorMessage = "Reason is required")]
        [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
        public string Reason { get; set; }
    }
}
