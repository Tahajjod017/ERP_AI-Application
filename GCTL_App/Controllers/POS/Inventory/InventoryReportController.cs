using GCTL.Core.Repository;
using GCTL.Data.Models;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GCTL_App.Controllers.POS.Inventory
{
    public class InventoryReportController : BaseController
    {
        private readonly IGenericRepository<Locations> _locationRepository;
        private readonly IGenericRepository<Products> _productRepository;
        private readonly IGenericRepository<ProductTypes> _productTypeRepository;
        public InventoryReportController(ITranslateService translateService, IUserProfileService userProfileService, IGenericRepository<Locations> locationRepository, IGenericRepository<Products> productRepository, IGenericRepository<ProductTypes> productTypeRepository) : base(translateService, userProfileService)
        {
            _locationRepository = locationRepository;
            _productRepository = productRepository;
            _productTypeRepository = productTypeRepository;
        }

        public IActionResult Index()
        {
            ViewBag.Locations = new SelectList(_locationRepository.AllActive()
                .Select(l => new { Id = l.LocationID, Name = l.LocationName }).ToList(),
                "Id", "Name");

            ViewBag.ProductTypes = new SelectList(_productTypeRepository.AllActive()
                .Select(p => new { Id = p.ProductTypeID, Name = p.ProductTypeName }).ToList(),
                "Id", "Name");

            return View(); 
        }
    }
}
