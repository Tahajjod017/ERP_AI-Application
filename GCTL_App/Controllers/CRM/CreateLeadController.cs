using GCTL.Core.Repository;
using GCTL.Core.ViewModels.CRM;
using GCTL.Core.ViewModels.MasterSetup.ServiceType;
using GCTL.Data.Models;
using GCTL.Service.CRM.LeadCreate;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol;
using SkiaSharp;


namespace GCTL_App.Controllers.CRM
{
    public class CreateLeadController : BaseController
    {
        #region CTOR
        private readonly IGenericRepository<Services> _serviceTypeRepository;
        private readonly IGenericRepository<LeadSources> _leadSourceTypeRepository;
        private readonly IGenericRepository<LeadStatuses> _leadStatusesTypeRepository;
        private readonly IGenericRepository<Customers> _customersRepository;
        private readonly ILeadCreateService _leadCreateService;
        private readonly IGenericRepository<IndividualAddresses> _individualAddressesRepository;
        private readonly AppDbContext _context;

        #endregion
        public CreateLeadController(AppDbContext context,ITranslateService translateService, IUserProfileService userProfileService, IGenericRepository<Services> serviceTypeRepository, IGenericRepository<LeadSources> leadSourceTypeRepository, IGenericRepository<Customers> customersRepository, IGenericRepository<IndividualAddresses> individualAddressesRepository, IGenericRepository<LeadStatuses> leadStatusesTypeRepository = null, ILeadCreateService leadCreateService = null) : base(translateService, userProfileService)
        {
            _serviceTypeRepository = serviceTypeRepository;
            _leadSourceTypeRepository = leadSourceTypeRepository;
            _leadStatusesTypeRepository = leadStatusesTypeRepository;
            _customersRepository = customersRepository;
            _leadCreateService = leadCreateService;
            _individualAddressesRepository = individualAddressesRepository;
            _context = context;
        }

        public async Task<IActionResult> index()
        {
            SetSmartPageCode(600100);

            ViewBag.ServiceDD = new SelectList(_serviceTypeRepository.AllActive().Select(e => new { e.ServiceID, e.ServiceName }), "ServiceID", "ServiceName");
            ViewBag.LeadSourceDD = new SelectList(_leadSourceTypeRepository.AllActive().Select(e => new { e.LeadSourceID, e.LeadSourceName }), "LeadSourceID", "LeadSourceName");
            ViewBag.LeadStatusDD = new SelectList(_leadStatusesTypeRepository.AllActive().Select(e => new { e.LeadStatusID, e.LeadStatusName }), "LeadStatusID", "LeadStatusName");


            return View();
        }


        [HttpGet]
        public IActionResult GetCustomerList()
        {
            var customers = _individualAddressesRepository.AllActive()
                .OrderBy(n => n.Individual.FirstName + n.Individual.LastName)
                .Select(n => new
                {
                    CustomerId = n.Individual.IndividualID,
                    FullName = n.Individual.FirstName + " " + n.Individual.LastName
                })
                .ToList();

            return Json(customers);
        }
        [HttpPost]
        public async Task<IActionResult> GetCustomerInfo([FromBody]  int id)
        {
            var customerObj = await (from add in _context.IndividualAddresses
                                     join ind in _context.Individuals
                                     on add.IndividualID equals ind.IndividualID
                                     join address in _context.Addresses on add.AddressID equals address.AddressID
                                     join country in _context.Country on address.CountryID equals country.CountryID
                                     where add.IndividualAddressID == id
                                     select new
                                     {
                                         ind.FirstName,
                                         ind.LastName,
                                         address.FullAddress,
                                         address.Street,
                                         address.City,
                                         address.Additionaladdress,
                                         address.State,
                                         address.PostalCode,
                                         country.CountryName,
                                         address.Latitude,
                                         address.Longitude,
                                         address.Phone,
                                         address.OtherPhone,
                                         address.Email


                                     }).FirstOrDefaultAsync();


            return Json(new { data = customerObj });
        }


        [HttpPost]
        public async Task<IActionResult> createPerson([FromBody] CustomerVM customerVM)
        {
            if (ModelState.IsValid)
            {
                if (customerVM.Customers[0].PrimaryID == 0)
                {
                    var result = await _leadCreateService.SaveLead(customerVM);


                    return Json(new { success = true, message = "Saved successfully" });
                }
            }
            return Json(new { MessageContent = "Error" });

        }

    }
}
