using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.FieldServices
{
    public class CreateJobVM
    {
        public int CreateJobID { get; set; }
        [Display(Name = "Customer Name")]
        public int? CustomerID { get; set; }
        [Display(Name = "Customer Name")]
        public string JobTitle { get; set; }
        [Display(Name = "Job Type")]
        public int? JobID { get; set; }
        [Display(Name = "Team Members")]
        public List<int>? TeamMembers { get; set; }
        [Display(Name = "Start Date Timee")]
        public string? StartDate { get; set; }
        [Display(Name = "End Date Time")]
        public string? EndDate { get; set; }
        [Display(Name = "Status")]
        public int StatusID { get; set; }
        [Display(Name = "Job Location Same as Customer Location")]
        public string? JobLocation { get; set; }
        [Display(Name = "Note")]
        public string? Note { get; set; }
        [Display(Name = "File Upload")]
        public IFormFile? FileLink { get; set; }
    }
}
