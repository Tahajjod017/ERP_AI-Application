using GCTL.Core.Repository;
using GCTL.Core.ViewModels.POS.Purchase.PurchaseOrder;
using GCTL.Data.Models;
using GCTL.Service.Language;
using GCTL.Service.POS.Purchase.PurchaseOrder;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GCTL_App.Controllers.POS.Purchase
{
    public class PurchaseOrderController : BaseController
    {
        private readonly IGenericRepository<Products> _productRepository;
        private readonly IGenericRepository<Suppliers> _supplierRepository;
        private readonly IGenericRepository<PurOrderBaseSAddresses> _addressRepository;
        private readonly IGenericRepository<PurOrderBaseSAddresses> _shippingAddressRepository;

        private readonly IPurchaseOrder _purchaseOrderService;

        public PurchaseOrderController(
            ITranslateService translateService,
            IUserProfileService userProfileService,
            IGenericRepository<Products> productRepository,
            IPurchaseOrder purchaseOrderService,
            IGenericRepository<Suppliers> supplierRepository,
            IGenericRepository<PurOrderBaseSAddresses> addressRepository,
            IGenericRepository<PurOrderBaseSAddresses> shippingAddressRepository)
            : base(translateService, userProfileService)
        {
            _productRepository = productRepository;
            _purchaseOrderService = purchaseOrderService;
            _supplierRepository = supplierRepository;
            _addressRepository = addressRepository;
            _shippingAddressRepository = shippingAddressRepository;
        }

        #region STATIC DATA
        private static readonly List<SupplierDto> StaticSuppliers = new()
        {
            new SupplierDto
            {
                Id = 1,
                CompanyName = "ABC Suppliers Ltd.",
                ContactName = "John Doe",
                Email = "john@abc.com",
                Phone = "(+880) 01737172631",
                AddressLine1 = "235 Mirpur, Road # 205",
                AddressLine2 = "Dhaka 1216, Bangladesh",
                TaxNumber = "10236254"
            },
            new SupplierDto
            {
                Id = 2,
                CompanyName = "XYZ Trading",
                ContactName = "Jane Smith",
                Email = "jane@xyz.com",
                Phone = "(+880) 01911223344",
                AddressLine1 = "House 12, Road 5",
                AddressLine2 = "Banani, Dhaka",
                TaxNumber = "98765432"
            }
        };
        #endregion

        // ==============================
        // GET: /PurchaseOrder/Index
        // ==============================
        public IActionResult Index()
        {
            SetSmartPageCode(90259200);

            ViewBag.Products = new SelectList(_productRepository.AllActive().ToList(), "ProductID", "ProductName");

            var data = new PurchaseOrderViewModel();
            return View(data);
        }

        // ==============================
        // GET: /PurchaseOrder/Create
        // ==============================
        [HttpGet]
        public IActionResult Create(int? selectedId = null)
        {
            var vm = new PurchaseOrderViewModel
            {
                Id = null,
                PurchaseDate = DateTime.Today,
                DueDate = DateTime.Today.AddDays(30),
                POID = GeneratePONumber(),
                OtherReference = "",
                SelectedSupplierId = selectedId,

                Items = new List<PurchaseOrderItem>
                {
                    new PurchaseOrderItem
                    {
                        SL = 1,
                        ProductId = null,
                        Quantity = null,
                        UnitPrice = null
                    }
                },

                TaxPercent = 0m,
                Note = ""
            };

            return View("Index", vm);
        }

        // ==============================
        // POST: Save Purchase Order (AJAX Version)
        // ==============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Save(PurchaseOrderViewModel vm)
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

                return Json(new { success = false, errors, message = messages });
            }

            var result = _purchaseOrderService.SaveAsync(vm).Result;

            return Json(new
            {
                success = result.Success,
                message = result.Message,
                purchaseOrderId = result.Data
            });
        }

        // ==============================
        // AJAX: Get All Suppliers
        // ==============================
        [HttpGet]
        public JsonResult GetAllSuppliers()
        {
            var result = _supplierRepository.AllActive()
                .Select(s => new
                {
                    Id = s.SupplierID,
                    CompanyName = s.FullName,
                    ContactName = "TODO",
                    Email = "TODO",
                    Phone = "TODO",
                    AddressLine1 = "TODO",
                    AddressLine2 = "TODO",
                    TaxNumber = "TODO"
                }).ToList();

            return Json(result);
        }

        // ==============================
        // AJAX: Get All Products
        // ==============================
        [HttpGet]
        public JsonResult GetProducts()
        {
            var result = _productRepository.AllActive()
             .ToList() // materialize first
             .Select(p => new
             {
                 id = p.ProductID,
                 name = p.ProductName,
                 price = p.ProductPricing.FirstOrDefault()?.SellingPriceExclVAT ?? 0m
             }).ToList();

            return Json(result);
        }

        // ==============================
        // AJAX: Search Suppliers
        // ==============================
        [HttpGet]
        public JsonResult SearchSuppliers(string term)
        {
            var filtered = string.IsNullOrWhiteSpace(term)
                ? StaticSuppliers
                : StaticSuppliers.Where(s =>
                    s.CompanyName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    s.ContactName.Contains(term, StringComparison.OrdinalIgnoreCase))
                .ToList();

            var result = filtered.Select(s => new
            {
                s.Id,
                s.CompanyName,
                s.ContactName,
                s.Email,
                s.Phone,
                s.AddressLine1,
                s.AddressLine2,
                s.TaxNumber
            });

            return Json(result);
        }

        // ==============================
        // AJAX: Add New Supplier
        // ==============================
        [HttpPost]
        public JsonResult AddSupplier([FromBody] SupplierDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.CompanyName))
                return Json(new { error = "Invalid supplier data" });

            dto.Id = StaticSuppliers.Count > 0 ? StaticSuppliers.Max(s => s.Id) + 1 : 1;
            StaticSuppliers.Add(dto);

            return Json(dto);
        }

        // ==============================
        // Helper: Generate PO Number
        // ==============================
        private string GeneratePONumber()
        {
            return "PO-" + DateTime.Now.ToString("yyyyMMdd-HHmmss");
        }

        [HttpGet]
        public IActionResult GetNextPurchaseOrderNumber()
        {
            var result = _purchaseOrderService.GetNextPOCode().Result;
            return Ok(result);
        }

        [HttpGet]
        public JsonResult GetAddresses()
        {
            var result = _addressRepository.AllActive()
                .Select(a => new
                {
                    Id = a.PurOrderBaseSAddressID,
                    FullName = a.FirstName + " " + a.LastName,
                    FullAddress = a.FullAddress,
                    City = a.City,
                    State = a.State,
                    PostalCode = a.PostalCode,
                    Phone = a.Phone,
                    Email = a.Email
                }).ToList();

            return Json(result);
        }

        [HttpPost]
        public async Task<JsonResult> AddAddress([FromBody] AddressDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.FullAddress))
                return Json(new { error = "Invalid address data" });

            var address = new PurOrderBaseSAddresses
            {
                FirstName = dto.FullName?.Split(' ').FirstOrDefault(),
                LastName = dto.FullName?.Split(' ').Skip(1).FirstOrDefault(),
                FullAddress = dto.FullAddress,
                City = dto.City,
                State = dto.State,
                PostalCode = dto.PostalCode,
                Phone = dto.Phone,
                Email = dto.Email,
                CreatedAt = DateTime.Now,
                CreatedBy = 1 // Replace with actual user ID
            };

            await _addressRepository.AddAsync(address);
             
            dto.Id = address.PurOrderBaseSAddressID;
            return Json(dto);
        }



        // ==============================
        // AJAX: Get Shipping Addresses
        // ==============================
        [HttpGet]
        public JsonResult GetShippingAddresses()
        {
            var result = _shippingAddressRepository.AllActive()
                .Select(a => new
                {
                    Id = a.PurOrderBaseSAddressID,
                    FullName = a.FirstName + " " + a.LastName,
                    FullAddress = a.FullAddress,
                    City = a.City,
                    State = a.State,
                    PostalCode = a.PostalCode,
                    Phone = a.Phone,
                    Email = a.Email
                }).ToList();

            return Json(result);
        }

        // ==============================
        // AJAX: Search Shipping Addresses
        // ==============================
        [HttpGet]
        public JsonResult SearchShippingAddresses(string term)
        {
            var query = _shippingAddressRepository.AllActive();

            var filtered = string.IsNullOrWhiteSpace(term)
                ? query.ToList()
                : query.Where(a =>
                    (a.FirstName + " " + a.LastName).Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    a.FullAddress.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    a.City.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    (a.Email != null && a.Email.Contains(term, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            var result = filtered.Select(a => new
            {
                Id = a.PurOrderBaseSAddressID,
                FullName = a.FirstName + " " + a.LastName,
                FullAddress = a.FullAddress,
                City = a.City,
                State = a.State,
                PostalCode = a.PostalCode,
                Phone = a.Phone,
                Email = a.Email
            });

            return Json(result);
        }

        // ==============================
        // AJAX: Add New Shipping Address
        // ==============================
        [HttpPost]
        public JsonResult AddShippingAddress([FromBody] ShippingAddressDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.FullAddress))
                return Json(new { error = "Full Address is required" });

            var address = new PurOrderBaseSAddresses
            {
                FirstName = dto.FullName?.Split(' ').FirstOrDefault(),
                LastName = dto.FullName?.Split(' ').Skip(1).FirstOrDefault(),
                FullAddress = dto.FullAddress,
                City = dto.City,
                State = dto.State,
                PostalCode = dto.PostalCode,
                Phone = dto.Phone,
                Email = dto.Email,
                CreatedAt = DateTime.Now,
                //CreatedBy =, // Implement this method
                
            };

            _shippingAddressRepository.AddAsync(address);
            

            dto.Id = address.PurOrderBaseSAddressID;
            return Json(dto);
        }

        // ==============================
        // AJAX: Get Shipping Address by ID
        // ==============================
        [HttpGet]
        public async Task<JsonResult> GetShippingAddress(int id)
        {
            var address = await _shippingAddressRepository.GetByIdAsync(id);

            if (address == null)
                return Json(new { error = "Address not found" });

            var result = new
            {
                Id = address.PurOrderBaseSAddressID,
                FullName = address.FirstName + " " + address.LastName,
                FullAddress = address.FullAddress,
                City = address.City,
                State = address.State,
                PostalCode = address.PostalCode,
                Phone = address.Phone,
                Email = address.Email
            };

            return Json(result);
        }



    }
}
