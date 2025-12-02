using GCTL.Core.Repository;
using GCTL.Core.ViewModels.POS.Product.SingleProduct;
using GCTL.Data.Models;
using GCTL.Service.Language;
using GCTL.Service.POS.Product;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SkiaSharp;

namespace GCTL_App.Controllers.POS.Product
{
    public class SingleProductController : BaseController
    {
        #region CTOR

        private readonly IGenericRepository<Products> _productRepository;
        private readonly IGenericRepository<AttributeNames> _attributeNamesRepository;
        private readonly IGenericRepository<AttributeValues> _attributeValuesRepository;
        private readonly IGenericRepository<WarrantyTypes> _warrantyRepository;
        private readonly IGenericRepository<ProductTypes> _productTypeRepository;
        private readonly IGenericRepository<ProductCategories> _productCategoryRepository;
        private readonly IGenericRepository<ProductSubCategories> _productSubCategoryRepository;
        private readonly IGenericRepository<ProductBrands> _productBrandRepository;
        private readonly IGenericRepository<UnitTypes> _unitRepository;
        private readonly IGenericRepository<CustomerGroup> _customerGroupRepository;
        private readonly IGenericRepository<CalculationTypes> _calculationTypesRepository;
        private readonly IGenericRepository<Classes> _assetTypeRepository;
        private readonly ISingleProduct _singleProductService;
        private readonly IAttributeProduct _attributeProductService;



        public SingleProductController(ITranslateService translateService, IUserProfileService userProfileService, IGenericRepository<Products> productRepository, IGenericRepository<ProductCategories> productCategoryRepository, IGenericRepository<ProductSubCategories> productSubCategoryRepository, IGenericRepository<ProductBrands> productBrandRepository, IGenericRepository<UnitTypes> unitRepository, IGenericRepository<Classes> assetTypeRepository, ISingleProduct singleProductService, IGenericRepository<WarrantyTypes> warrantyRepository, IGenericRepository<ProductTypes> productTypeRepository, IGenericRepository<CustomerGroup> customerGroupRepository, IGenericRepository<CalculationTypes> calculationTypesRepository, IGenericRepository<AttributeNames> attributeNamesRepository, IGenericRepository<AttributeValues> attributeValuesRepository, IAttributeProduct attributeProductService) : base(translateService, userProfileService)
        {
            _productRepository = productRepository;
            _productCategoryRepository = productCategoryRepository;
            _productSubCategoryRepository = productSubCategoryRepository;
            _productBrandRepository = productBrandRepository;
            _unitRepository = unitRepository;
            _assetTypeRepository = assetTypeRepository;
            _singleProductService = singleProductService;
            _warrantyRepository = warrantyRepository;
            _productTypeRepository = productTypeRepository;
            _customerGroupRepository = customerGroupRepository;
            _calculationTypesRepository = calculationTypesRepository;
            _attributeNamesRepository = attributeNamesRepository;
            _attributeValuesRepository = attributeValuesRepository;
            _attributeProductService = attributeProductService;
        }
        #endregion

        #region Index
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

            var ProductTypeDD = new[] { "single", "attribute" };

            var matchedTypes = _productTypeRepository.AllActive().AsEnumerable()
                .Where(t => !string.IsNullOrEmpty(t.ProductTypeName) &&
                            ProductTypeDD.Any(x => t.ProductTypeName.Contains(x, StringComparison.OrdinalIgnoreCase)))
                .Select(t => new { id = t.ProductTypeID, name = t.ProductTypeName })
                .ToList();

            ViewBag.ProductTypeDD = new SelectList(matchedTypes, "id", "name");
            ViewBag.WarrantyDD = new SelectList(_warrantyRepository.AllActive().Select(e => new { id = e.WarrantyTypeID, name = e.WarrantyTypeName }).ToList(), "id", "name");
            ViewBag.CustomerGroupDD = new SelectList(_customerGroupRepository.AllActive().Select(e => new { id = e.CustomerGroupID, name = e.CustomerGroupName }).ToList(), "id", "name");
            ViewBag.CalculationTypesDD = new SelectList(_calculationTypesRepository.AllActive().Select(e => new { id = e.CalculationTypeID, name = e.CalculationTypeName }).ToList(), "id", "name");


            var attributeNames = _attributeNamesRepository.AllActive()
                                .Where(a => a.IsActive == true && a.DeletedAt == null)
                                .ToList();

            // Load attribute values (linked to each name)
            var attributeValues = _attributeValuesRepository.AllActive()
                                          .Where(v => v.DeletedAt == null)
                                          .ToList();

            ViewBag.AttributeNames = attributeNames;
            ViewBag.AttributeValues = attributeValues;


            return View(model);
        }
        #endregion

        #region Single Prosuct

        [HttpGet]
        public async Task<IActionResult> GetAutoSKU()
        {
            string autoSKU = await _singleProductService.GetNextSKU();
            return Json(new { sku = autoSKU });
        }

        [HttpGet]
        public async Task<IActionResult> GetDD()
        {
            var customer = await _customerGroupRepository.AllActive().Select(e => new { id = e.CustomerGroupID, name = e.CustomerGroupName }).ToListAsync();
            var calculationTypes = await _calculationTypesRepository.AllActive().Select(e => new { id = e.CalculationTypeID, name = e.CalculationTypeName }).ToListAsync();
            return Json(new { customer = customer , calculationTypes = calculationTypes });
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

                return Ok(dtat);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        #endregion


        #region ATD prduct


        [HttpPost]
        public async Task<IActionResult> AddAttributeProduct(AttrProductAddViewModel model, List<IFormFile> AttrProductImages, [FromForm] string AttrSelectedValuesJson)   // <-- always present
        {

            if (!string.IsNullOrWhiteSpace(AttrSelectedValuesJson))
            {
                model.AttrSelectedValues = JsonConvert.DeserializeObject<
                    Dictionary<string, List<AttributeValueDto>>>(AttrSelectedValuesJson)
                    ?? new();
            }
            else
            {
                model.AttrSelectedValues = new Dictionary<string, List<AttributeValueDto>>();
            }

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

            

            var result = await _attributeProductService.AddAttrProductAsync(model);

            return Json(result);
        }

        #endregion

    }
}
