using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AdminSettingsVM
{
    public class CompanySettingsVM:BaseViewModel
    {
        public int OrganizationID { get; set; }

        public string? OrganizationName { get; set; }   
        public string? EmailAddress { get; set; }

        public string? Phone { get; set; }

        public string? Fax { get; set; }

        public string? WebAddress { get; set; }

        public string? LogoLink { get; set; }

        public string? FaviconLink { get; set; }

        public string? Address { get; set; }

        public int? CountryID { get; set; }

        public string? Street { get; set; }

        public string? City { get; set; }

        public string? PostCode { get; set; }

    }
}
