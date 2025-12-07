using System.Threading.Tasks;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.POS.Product.SingleProduct;
using GCTL.Data.Models;
using GCTL.Service.Language;
using GCTL.Service.POS.Product;
using GCTL.Service.POS.Product.ServiceProduct;
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
        private readonly IServiceProduct _serviceProductService;



        public SingleProductController(ITranslateService translateService, IUserProfileService userProfileService, IGenericRepository<Products> productRepository, IGenericRepository<ProductCategories> productCategoryRepository, IGenericRepository<ProductSubCategories> productSubCategoryRepository, IGenericRepository<ProductBrands> productBrandRepository, IGenericRepository<UnitTypes> unitRepository, IGenericRepository<Classes> assetTypeRepository, ISingleProduct singleProductService, IGenericRepository<WarrantyTypes> warrantyRepository, IGenericRepository<ProductTypes> productTypeRepository, IGenericRepository<CustomerGroup> customerGroupRepository, IGenericRepository<CalculationTypes> calculationTypesRepository, IGenericRepository<AttributeNames> attributeNamesRepository, IGenericRepository<AttributeValues> attributeValuesRepository, IAttributeProduct attributeProductService, IServiceProduct serviceProductService) : base(translateService, userProfileService)
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
            _serviceProductService = serviceProductService;
        }
        #endregion

        #region Index
        public IActionResult Index()
        {
            SetSmartPageCode(1897000);

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

        #region SKU

        public IActionResult GetSKUByCategory(int categoryId)
        {
            // Example: fetch SKU based on categoryId from DB
            var sku = _productRepository.AllActive()
                              .Where(p => p.ProductCategoryID == categoryId)
                              .Select(p => p.SKU)
                              .FirstOrDefault();

            return Json(sku); // return SKU as JSON
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


        #region Servixc


        [HttpPost]
        public async Task<IActionResult> AddService(ServiceViewModel model)
        {
            if (model.ServiceSelectService == null)
            {
                return Json(new { success = false, message = "Enter Service name Please!" });

            }

            var result = await _serviceProductService.AddServiceAsync(model);   


            // TODO: Save to database here
            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetServiceList(int page = 1, int pageSize = 10, string search = "", string sort = "")
        {
            var allData = await _productRepository.AllActive().Where(e=>e.ProductTypeID == 3).Select(e=> new ServiceViewModel
            {
                ServiceSelectService = e.ProductName,
                ServiceHourlyRate = e.ServiceHourlyRate,
                ServiceDailyRate = e.ServiceDailyRate,
                ServicePerJobRate = e.ServicePerJobRate,
                ServicePerMeterRate = e.ServicePerMeterRate
            }).ToListAsync();

            

            var query = allData.AsQueryable();

            // Search Filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(x => x.ServiceSelectService.ToLower().Contains(search));
            }

            // Sorting
            query = sort switch
            {
                "nameAsc" => query.OrderBy(x => x.ServiceSelectService),
                "nameDesc" => query.OrderByDescending(x => x.ServiceSelectService),
                "hourlyAsc" => query.OrderBy(x => x.ServiceHourlyRate),
                "hourlyDesc" => query.OrderByDescending(x => x.ServiceHourlyRate),
                _ => query.OrderBy(x => x.ServiceSelectService)
            };

            // Get Total Count BEFORE Paging (Critical!)
            int total = query.Count();

            // Apply Paging
            var pagedData = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Optional: Select only needed fields (performance)
            var result = pagedData.Select(x => new
            {
                serviceSelectService = x.ServiceSelectService,
                serviceHourlyRate = x.ServiceHourlyRate,
                serviceDailyRate = x.ServiceDailyRate,
                servicePerJobRate = x.ServicePerJobRate,
                servicePerMeterRate = x.ServicePerMeterRate
            });

            return Json(new
            {
                success = true,
                data = result,
                total = total,
                page = page,
                pageSize = pageSize,
                totalPages = (int)Math.Ceiling(total / (double)pageSize)
            });
        }



        #endregion

    }
}
