using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AttendanceManagement.LeaveManagements.LeaveRequest
{
    public class UploadSickDocVM
    {
        public int LeaveApplicationID { get; set; }
        public int? LeaveTypeID { get; set; }

        public int SickLeaveDocumentID { get; set; }

        [Required(ErrorMessage ="Please Upload Document")]
        public IFormFile DocumentPath { get; set; }
        public string? DocumentPathdata { get; set; }
    }
}
