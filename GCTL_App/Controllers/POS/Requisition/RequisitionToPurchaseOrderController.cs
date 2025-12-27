using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.POS.Purchase.PurchaseOrder;
using GCTL.Core.ViewModels.POS.Requsition.RequisitionToPurchaseOrder;
using GCTL.Data.Models;
using GCTL.Service.Language;
using GCTL.Service.POS.Purchase.PurchaseOrder;
using GCTL.Service.POS.Requsition.RequisitionToPurchaseOrder;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GCTL_App.Controllers.POS.Requisition
{
    public class RequisitionToPurchaseOrderController : BaseController
    {
        private readonly IRequisitionToPurchaseOrderService _requisitionToPOService;
        private readonly IPurchaseOrder _poService;

        private readonly IGenericRepository<ProductTypes> _productTypesRepository;
        private readonly IGenericRepository<Suppliers> _supplierRepository;
        private readonly IGenericRepository<PurOrderBaseSAddresses> _addressRepository;
        private readonly IGenericRepository<Statuses> _statusRepository;

        public RequisitionToPurchaseOrderController(
            ITranslateService translateService,
            IUserProfileService userProfileService,
            IRequisitionToPurchaseOrderService requisitionToPOService,
            IGenericRepository<ProductTypes> productTypesRepository,
            IGenericRepository<Suppliers> supplierRepository,
            IGenericRepository<PurOrderBaseSAddresses> addressRepository,
            IGenericRepository<Statuses> statusRepository,
            IPurchaseOrder poService)
            : base(translateService, userProfileService)
        {
            _requisitionToPOService = requisitionToPOService;
            _productTypesRepository = productTypesRepository;
            _supplierRepository = supplierRepository;
            _addressRepository = addressRepository;
            _statusRepository = statusRepository;
            _poService = poService;
        }

        #region Index
        public IActionResult Index()
        {
            ViewBag.ProductTypes = new SelectList(_productTypesRepository.AllActive()
                .Select(e => new { Id = e.ProductTypeID, Name = e.ProductTypeName }).ToList(),
                "Id", "Name");

            ViewBag.Suppliers = new SelectList(_supplierRepository.AllActive()
                .Select(s => new { Id = s.SupplierID, Name = s.FullName }).ToList(),
                "Id", "Name");

            ViewBag.Statuses = new SelectList(_statusRepository.AllActive()
                .Select(s => new { Id = s.StatusID, Name = s.StatusName }).ToList(),
                "Id", "Name");

            return View();
        }
        #endregion

        #region Get Approved Requisitions
        [HttpGet]
        public async Task<IActionResult> GetApprovedRequisitions(int page = 1, int pageSize = 10, string search = "", string sortColumn = "RequisitionId", string sortDirection = "asc", int? productTypeId = null, string? fromDate = null, string? toDate = null)
        {
            int? empID = await GetCurrentEmployeeIdAsync();
            int? orgID = await GetCurrentOrganizationIdAsync();

            var result = await _requisitionToPOService.GetApprovedRequisitionsAsync( orgID, page, pageSize, search, sortColumn, sortDirection, productTypeId, fromDate, toDate);

            return Json(new
            {
                data = result.Items,
                totalRecords = result.TotalRecords,
                page = page,
                pageSize = pageSize
            });
        }
        #endregion

        #region Get Requisition Details
        [HttpGet]
        public async Task<IActionResult> GetRequisitionDetails(int id)
        {
            try
            {
                var details = await _requisitionToPOService.GetRequisitionDetailsForPOAsync(id);

                if (details == null)
                    return NotFound(new { message = "Requisition not found" });

                return Ok(new { success = true, data = details });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", detail = ex.Message });
            }
        }
        #endregion

        #region Get Suppliers
        [HttpGet]
        public JsonResult GetSuppliers()
        {
            var suppliers = _supplierRepository.AllActive()
                .Select(s => new
                {
                    Id = s.SupplierID,
                    CompanyName = s.FullName,
                    ContactName = "TODO", // s.ContactName ?? "",
                    Email = "TODO", //  s.Email ?? "",
                    Phone = "TODO", //  s.Phone ?? "",
                    AddressLine1 = "TODO", //  s.Address ?? "",
                    AddressLine2 = "",
                    TaxNumber = "TODO", //  s.TaxNumber ?? ""
                }).ToList();

            return Json(suppliers);
        }
        #endregion

        #region Get Addresses
        [HttpGet]
        public JsonResult GetAddresses()
        {
            var addresses = _addressRepository.AllActive()
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

            return Json(addresses);
        }
        #endregion

        #region Convert to Purchase Order
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConvertToPurchaseOrder(ConvertToPurchaseOrderViewModel model, BaseViewModel? baseView)
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

            int? empID = await GetCurrentEmployeeIdAsync();
            int? org = await GetCurrentOrganizationIdAsync();
            var result = await _requisitionToPOService.ConvertToPurchaseOrderAsync(model, empID, org, baseView);

            return Json(result);
        }
        #endregion

        #region Get Next PO Code
        [HttpGet]
        public async Task<IActionResult> GetNextPOCode()
        {
            var code = await _poService.GetNextPOCode();
            return Ok(code);
        }
        #endregion

        #region Add Supplier
        [HttpPost]
        public async Task<JsonResult> AddSupplier([FromBody] SupplierDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.CompanyName))
                return Json(new { error = "Invalid supplier data" });

            var supplier = new Suppliers
            {
                FullName = dto.CompanyName,
                //ContactName = dto.ContactName,
                //Email = dto.Email,
                //Phone = dto.Phone,
                //Address = dto.AddressLine1,
                //TaxNumber = dto.TaxNumber,
                //CreatedAt = DateTime.UtcNow,
                CreatedBy = await GetCurrentEmployeeIdAsync()
            };

            await _supplierRepository.AddAsync(supplier);

            dto.Id = supplier.SupplierID;
            return Json(dto);
        }
        #endregion

        #region Add Address
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
                CreatedAt = DateTime.UtcNow,
                CreatedBy = await GetCurrentEmployeeIdAsync()
            };

            await _addressRepository.AddAsync(address);

            dto.Id = address.PurOrderBaseSAddressID;
            return Json(dto);
        }
        #endregion

        #region Export
        [HttpPost]
        public async Task<IActionResult> GeneratePDF(string? fromDate = null, string? toDate = null)
        {
            int? orgID = await GetCurrentOrganizationIdAsync();

            var pdfBytes = await _requisitionToPOService.GeneratePDF(orgID ?? 0, fromDate, toDate);

            if (pdfBytes == null || pdfBytes.Length == 0)
                return NotFound("PDF generation failed");

            return File(pdfBytes, "application/pdf", "approved_requisitions.pdf");
        }

        [HttpGet]
        public async Task<IActionResult> DownloadExcel(string? fromDate = null, string? toDate = null)
        {
            int? orgID = await GetCurrentOrganizationIdAsync();

            var excelBytes = await _requisitionToPOService.GenerateExcel(orgID ?? 0, fromDate, toDate);

            return File(excelBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "approved_requisitions.xlsx");
        }
        #endregion
    }



}
