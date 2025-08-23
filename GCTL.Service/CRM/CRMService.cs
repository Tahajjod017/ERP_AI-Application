using GCTL.Core.Repository;
using GCTL.Core.ViewModels.CRM;
using GCTL.Data.Models;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.CRM
{
    public class CRMService : ICRMService
    {
        private readonly IGenericRepository<Leads> _leadsGenericRepository;
        private readonly AppDbContext _context;
        public CRMService(IGenericRepository<Leads> leadsGenericRepository, AppDbContext context)
        {
            _context = context;
            _leadsGenericRepository = leadsGenericRepository;
        }
        public async Task<List<LeadsTableVM>> GetLeads(string customerType)
        {
            var query = await (
                from lead in _leadsGenericRepository.AllActive()
                join indDddr in _context.IndividualAddresses
                    on lead.CustomerID equals indDddr.IndividualAddressID
                join address in _context.Addresses
                    on indDddr.AddressID equals address.AddressID
                join individual in _context.Individuals
                    on indDddr.IndividualID equals individual.IndividualID
                join leadStatus in _context.LeadStatuses
                    on lead.LeadStatusID equals leadStatus.LeadStatusID
                join leadSource in _context.LeadSources
                    on lead.LeadSourceID equals leadSource.LeadSourceID
                join leadOwner in _context.Employees
                    on lead.LeadOwnerID equals leadOwner.EmployeeID
                orderby lead.LeadID descending
                select new LeadsTableVM
                {
                    LeadId = lead.LeadID,
                    LeadStatus = leadStatus.LeadStatusName,
                    LeadSourceName = leadSource.LeadSourceName,
                    LeadOwnerName = leadOwner.FirstName + " " + leadOwner.LastName,
                    ApproximateDealValue = lead.ApproximateDealValue,
                    ProbabilityPercentage = lead.ProbabilityPercentage,

                    LeadName = lead.LeadName,
                    Phone = address.Phone,
                    Email = address.Email,
                    ContactName = address.FirstName + " " + address.LastName,
                    Status = "Active",
                }
            ).ToListAsync();

            return query;
        }
    }
}
