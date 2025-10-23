using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AdminSettingsVM
{
    public class CompanySettingsVM:BaseViewModel
    {
        public int OrganizationID { get; set; }

        [Required(ErrorMessage = "OrganizationName is required")]
        public string? OrganizationName { get; set; }   
        public string? EmailAddress { get; set; }

        public string? Phone { get; set; }

        public string? Fax { get; set; }

        public string? WebAddress { get; set; }

        public IFormFile? LogoLinkIform { get; set; }

        public IFormFile? FaviconLinkIform { get; set; }

        //These will store the file paths after upload
        public string? LogoLink { get; set; } 
        public string? FaviconLink { get; set; } 


        public string? Address { get; set; }

        public int? CountryID { get; set; }
        public string? CountryName { get; set; }

        public string? Street { get; set; }

        public string? City { get; set; }

        public string? PostCode { get; set; }
        public string? Latitude { get; set; }

        public string? Longitude { get; set; }


    }
}
