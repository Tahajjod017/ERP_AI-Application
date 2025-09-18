using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.Helpers.CommonSelectMasterDropDown
{
    public class CommonDropDownService : ICommonDroDownService
    {
        private readonly IGenericRepository<Organization> organization;

        public CommonDropDownService(IGenericRepository<Organization> organization)
        {
            this.organization = organization;
        }

        public async Task<List<CommonDropDownVM>> GetAllOrganizationsAsync()
        {
            try
            {
                var orgaList = await organization.AllActive()
                    .Select(x => new CommonDropDownVM
                    {
                        Id = x.OrganizationID,
                        Name = x.OrganizationName
                    }).ToListAsync();
                return orgaList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        
    }
}
