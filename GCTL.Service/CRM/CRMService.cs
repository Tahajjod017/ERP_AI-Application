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
                select new LeadsTableVM
                {
                    LeadId = lead.LeadID,
                    LeadName = lead.LeadName,
                    Phone = address.Phone,
                    Email = address.Email,
                    ContactName = address.FirstName + " " + address.LastName,
                    CompanyName ="Google",
                    Status = "Active",
                }
            ).ToListAsync();

            return query;
        }
    }
}
