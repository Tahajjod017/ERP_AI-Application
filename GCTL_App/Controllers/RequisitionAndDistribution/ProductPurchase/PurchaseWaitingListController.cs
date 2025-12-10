using GCTL.Core.Repository;
using GCTL.Core.ViewModels;

using GCTL.Core.ViewModels.RequisitionDistribution.Purchase.PurchaseWaitionList;
using GCTL.Data.Models;
using GCTL.Service.Language;

using GCTL.Service.RequisitionDistribution.PurchaseWaitingList;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace GCTL_App.Controllers.RequisitionAndDistribution.ProductPurchase
{
    [Authorize]
    public class PurchaseWaitingListController : BaseController
    {
        private readonly IPurchaseWaitingList _purchaseWaitingList;

        private readonly IGenericRepository<ProductTypes> _productTypeRepository;
       // private readonly IGenericRepository<Projects> _projectRepository;
        private readonly IGenericRepository<Products> _productRepository;

        public PurchaseWaitingListController(ITranslateService translateService, IUserProfileService userProfileService, IPurchaseWaitingList purchaseWaitingList, IGenericRepository<ProductTypes> productTypeRepository, IGenericRepository<Products> productRepository) : base(translateService, userProfileService)
        {
            _purchaseWaitingList = purchaseWaitingList;
            _productTypeRepository = productTypeRepository;
           
            _productRepository = productRepository;
        }

        public IActionResult Index()
        {
            ViewBag.ProductTypeDD = new SelectList(_productTypeRepository.AllActive().Select(e => new { Id = e.ProductTypeID, Name = e.ProductTypeName }).ToList(), "Id", "Name");
            //ViewBag.ProjectDD = new SelectList(_projectRepository.AllActive().Select(e => new { Id = e.ProjectID, Name = e.ProjectName }).ToList(), "Id", "Name");
            ViewBag.ProductDD = new SelectList(_productRepository.AllActive().Select(e => new { Id = e.ProductID, Name = e.ProductName }).ToList(), "Id", "Name");



            return View();
        }



        // AJAX endpoint to get purchase waiting list data
        [Microsoft.AspNetCore.Mvc.HttpPost]
        public IActionResult GetPurchaseWaitingList([FromBody] PurchaseRequestModel request)
        {
            try
            {
                var filters = new FilterOptions
                {
                    SearchTerm = request.SearchTerm ?? "",
                    ProductTypeId = request.ProductTypeId,
                    ProjectId = request.ProjectId,
                    ProductId = request.ProductId,
                    SortBy = request.SortBy ?? "reqId",
                    SortDirection = request.SortDirection ?? "asc"
                };

                var pagination = new PaginationInfo
                {
                    CurrentPage = request.Page > 0 ? request.Page : 1,
                    PageSize = request.PageSize > 0 ? request.PageSize : 10
                };

                var result = _purchaseWaitingList.GetPurchaseWaitingList(filters, pagination);

                return Json(new
                {
                    success = true,
                    data = result.PurchaseRequests.Select(p => new
                    {
                        purchaseId = p.PurchaseId,
                        poId = p.PoId,
                        reqId = p.ReqId, 
                        projectName = p.ProjectName,
                        projectId = p.ProjectId,
                        requestedBy = p.RequestedBy,
                        requestDate = p.RequestDate != DateTime.MinValue ? p.RequestDate.Value.ToString("yyyy-MM-dd") : "",
                        requestDateFormatted = p.RequestDate != DateTime.MinValue ? p.RequestDate.Value.ToString("MMM dd, yyyy") : "",
                        productType = p.ProductType,
                        productTypeId = p.ProductTypeId,
                        productName = p.ProductName,
                        productId = p.ProductId,
                        quantity = p.Quantity,
                        estimatedCost = p.EstimatedCost,
                        status = p.Status,
                        priority = p.Priority
                    }),
                    pagination = new
                    {
                        currentPage = result.Pagination.CurrentPage,
                        pageSize = result.Pagination.PageSize,
                        totalItems = result.Pagination.TotalItems,
                        totalPages = result.Pagination.TotalPages,
                        hasPrevious = result.Pagination.HasPrevious,
                        hasNext = result.Pagination.HasNext,
                        startItem = result.Pagination.StartItem,
                        endItem = result.Pagination.EndItem
                    },
                    filters = filters
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Error loading purchase waiting list: " + ex.Message
                });
            }
        }

        // AJAX endpoint to get dropdown data
        [Microsoft.AspNetCore.Mvc.HttpGet]
        public IActionResult GetDropdownData()
        {
            try
            {
                var productTypes = _productTypeRepository.AllActive()
                    .Select(e => new { id = e.ProductTypeID, name = e.ProductTypeName })
                    .ToList();

               // var projects = _projectRepository.AllActive().Select(e => new { id = e.ProjectID, name = e.ProjectName }).ToList();

                var products = _productRepository.AllActive()
                    .Select(e => new { id = e.ProductID, name = e.ProductName })
                    .ToList();

                return Json(new
                {
                    success = true,
                    productTypes = productTypes,
                   // projects = projects,
                    products = products
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Error loading dropdown data: " + ex.Message
                });
            }
        }

        // AJAX endpoint to get single purchase request
        [Microsoft.AspNetCore.Mvc.HttpGet]

        //public IActionResult GetPurchaseRequest(string reqId)
        //{
        //    try
        //    {
        //        var purchaseRequest = _purchaseWaitingList.GetPurchaseRequestById(reqId);

        //        if (purchaseRequest == null)
        //        {
        //            return Json(new
        //            {
        //                success = false,
        //                message = "Purchase request not found"
        //            });
        //        }

        //        return Json(new
        //        {
        //            success = true,
        //            data = purchaseRequest
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new
        //        {
        //            success = false,
        //            message = "Error loading purchase request: " + ex.Message
        //        });
        //    }
        //}


        public IActionResult GetPurchaseRequest(string reqId)
        {
            try
            {
                // Static data that matches your HTML design exactly
                var purchaseData = new
                {
                    success = true,
                    data = new
                    {
                        poNumber = "PO-2025-0001",
                        status = "DRAFT",
                        poDate = "Sept 23, 2025",
                        dueDate = "Oct 15, 2025",
                        workOrder = "WO-2025-045",
                        reference = "REF-IT-EQUIPMENT",
                        vendorDetails = @"<strong>ABC Supplies Ltd.</strong><br>
                                        123 Business Street<br>
                                        Dhaka-1212, Bangladesh<br>
                                        <strong>Contact:</strong> John Doe<br>
                                        <strong>Phone:</strong> +88 01234-567890",
                        shipToDetails = @"<strong>GCTL InfoSys - Warehouse</strong><br>
                                        House-42 (5th Floor), Sector-4<br>
                                        Uttara, Dhaka-1230<br>
                                        <strong>Contact:</strong> Warehouse Manager",
                        items = new List<object>
                        {
                            new {
                                id = "ITM-001",
                                description = "Dell OptiPlex 7090",
                                unit = "PC",
                                quantity = 5,
                                price = 85000,
                                total = 425000
                            },
                            new {
                                id = "ITM-002",
                                description = "HP LaserJet M404dn",
                                unit = "PC",
                                quantity = 2,
                                price = 35000,
                                total = 70000
                            }
                        },
                        summary = new
                        {
                            subtotal = "৳495,000",
                            tax = "৳74,250",
                            grandTotal = "৳569,250"
                        },
                        notes = "Please deliver items in original packaging. Setup required for desktops.",
                        terms = "Payment Net 30 | 1yr Warranty | Free Dhaka delivery",
                        barcodeText = "PO-2025-0001",
                        footer = "Generated on Sept 23, 2025 | Page 1 of 1",
                        requestedBy = "Project Manager",
                        projectName = "IT Equipment Upgrade",
                        productName = "Dell OptiPlex 7090",
                        productType = "Computer Hardware",
                        quantity = 5,
                        estimatedCost = 85000
                    }
                };

                return Json(purchaseData);
            }
            catch (System.Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        #region GeneratePDF
        [HttpPost]
        public async Task<IActionResult> GeneratePDF(int id)
        {
            if (id <= 0)
                return BadRequest("Invalid ID");

            var pdfBytes = await _purchaseWaitingList.GeneratePDF(await GetCurrentOrganizationIdAsync() ?? 0, id);
            if (pdfBytes == null || pdfBytes.Length == 0)
                return NotFound("PDF generation failed");

            return File(pdfBytes, "application/pdf", "report.pdf");
        }
        #endregion
    }
}
