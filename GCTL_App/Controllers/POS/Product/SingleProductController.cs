using GCTL.Core.Repository;
using GCTL.Core.ViewModels.POS.Product.SingleProduct;
using GCTL.Data.Models;
using GCTL.Service.Language;
using GCTL.Service.POS.Product;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GCTL_App.Controllers.POS.Product
{
    public class SingleProductController : BaseController
    {
        private readonly IGenericRepository<Products> _productRepository;
        private readonly IGenericRepository<ProductCategories> _productCategoryRepository;
        private readonly IGenericRepository<ProductSubCategories> _productSubCategoryRepository;
        private readonly IGenericRepository<ProductBrands> _productBrandRepository;
        private readonly IGenericRepository<UnitTypes> _unitRepository;
        private readonly IGenericRepository<Classes> _assetTypeRepository;
        private readonly ISingleProduct _singleProductService;



        public SingleProductController(ITranslateService translateService, IUserProfileService userProfileService, IGenericRepository<Products> productRepository, IGenericRepository<ProductCategories> productCategoryRepository, IGenericRepository<ProductSubCategories> productSubCategoryRepository, IGenericRepository<ProductBrands> productBrandRepository, IGenericRepository<UnitTypes> unitRepository, IGenericRepository<Classes> assetTypeRepository, ISingleProduct singleProductService) : base(translateService, userProfileService)
        {
            _productRepository = productRepository;
            _productCategoryRepository = productCategoryRepository;
            _productSubCategoryRepository = productSubCategoryRepository;
            _productBrandRepository = productBrandRepository;
            _unitRepository = unitRepository;
            _assetTypeRepository = assetTypeRepository;
            _singleProductService = singleProductService;
        }

        public IActionResult Index()
        {
            var model = new SingleProductPageViewModel();

            ViewBag.CategoryDD = new SelectList(_productCategoryRepository.AllActive().Select(e => new { id = e.ProductCategoryID, name = e.ProductCategoryName }).ToList(), "id", "name");
            ViewBag.SubCategoryDD = new SelectList(_productSubCategoryRepository.AllActive().Select(e => new { id = e.ProductSubCategoryID, name = e.ProductSubCategoryName }).ToList(), "id", "name");
            ViewBag.BrandDD = new SelectList(_productBrandRepository.AllActive().Select(e => new { id = e.ProductBrandID, name = e.ProductBrandName }).ToList(), "id", "name");
            ViewBag.UnitDD = new SelectList(_unitRepository.AllActive().Select(e => new { id = e.UnitTypeID, name = e.UnitTypeName }).ToList(), "id", "name");
            ViewBag.AssetTypeDD = new SelectList(_assetTypeRepository.AllActive().Select(e => new { id = e.ClassID, name = e.ClassName }).ToList(), "id", "name");

            ViewBag.VatDD = new SelectList(new List<SelectListItem>
            {
                new SelectListItem { Text = "8%", Value = "8" },
                new SelectListItem { Text = "12%", Value = "12" },
                new SelectListItem { Text = "13%", Value = "13" },
                new SelectListItem { Text = "15%", Value = "15" }
            }, "Value", "Text");


            ViewBag.WarrantyDD = new SelectList(new List<SelectListItem>
            {
                new SelectListItem { Text = "Replacement Warranty", Value = "Replacement Warranty" },
                new SelectListItem { Text = "On-Site Warranty", Value = "On-Site Warranty" },
                new SelectListItem { Text = "Accidental Protection Plan", Value = "Accidental Protection Plan" }
            }, "Value", "Text");


            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetAutoSKU()
        {
            string autoSKU = await _singleProductService.GetNextSKU();
            return Json(new { sku = autoSKU });
        }


        [HttpPost]
        public async Task<IActionResult> AddSingleProduct(SingleProductViewModel model)
        {
            try
            {
               

                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                            .Where(x => x.Value.Errors.Count > 0)
                            .ToDictionary(
                                kvp => kvp.Key,
                                kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToList()
                            );

                    var messages = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .SelectMany(x => x.Value.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();


                    return Json(new { success = false, errors = errors, message = messages });
                }


                var dtat = await _singleProductService.AddProductAsync(model);

                return Json(new
                {
                    success = true,
                    message = "Product added successfully",
                    redirectUrl = Url.Action("Index", "SingleProduct"),
                    model = model
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

    }
}
