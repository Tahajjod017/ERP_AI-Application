using System.Threading.Tasks;

using GCTL.Core.Enums;
using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.POS.Sales.PriceQuotationDetails;
using GCTL.Core.ViewModels.POS.Sales.SalesOrders;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.Language;
using GCTL.Service.POS.Sales.PriceQuotation;
using GCTL.Service.POS.Sales.SalesOrderF;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace GCTL_App.Controllers.POS.Sales
{
    public class PriceQuotationDetailsController : BaseController
    {
        #region CTOR

        private readonly IGenericRepository<PriceQuotations> _priceQuotationRepository;
        private readonly IGenericRepository<PriceQuotationVersionItems> _priceQuotationItemRepository;
        private readonly IGenericRepository<PriceQuotationVersions> _priceQuotationVersionRepository;
        private readonly IGenericRepository<SalesOrders> _salesOrderRepository;
        private readonly IGenericRepository<SalesOrdersVersions> _salesVersionRepository;
        private readonly IGenericRepository<Products> _productRepository;
        private readonly IGenericRepository<Locations> _locationRepository;
        private readonly IPriceQuotation _priceQuotationService;
        private readonly IGenericRepository<UnitTypes> _unitTypeRepository;
        private readonly IGenericRepository<Customers> _customerRepository;
        private readonly IGenericRepository<CustomerAddresses> _customerAddressRepository;
        private readonly IGenericRepository<Addresses> _addressRepository;
        private readonly IGenericRepository<GCTL.Data.Models.Inventory> _inventoryRepository;
        private readonly IUserInfoService _userInfoService;

        private readonly IGenericRepository<PriceQuotationVersions> _priceQuotationVersionsRepository;
        private readonly ISalesOrder _salesOrderService;



        // Add this for Work Orders when you create the table
        // private readonly IGenericRepository<WorkOrders> _workOrderRepository;

        public PriceQuotationDetailsController(
            ITranslateService translateService,
            IUserProfileService userProfileService,
            IGenericRepository<PriceQuotations> priceQuotationRepository,
            IGenericRepository<PriceQuotationVersionItems> priceQuotationItemRepository,
            IPriceQuotation priceQuotationService,
            IGenericRepository<UnitTypes> unitTypeRepository,
            IGenericRepository<Customers> customerRepository,
            IGenericRepository<CustomerAddresses> customerAddressRepository,
            IGenericRepository<Addresses> addressRepository,
            IUserInfoService userInfoService,
            IGenericRepository<PriceQuotationVersions> priceQuotationVersionRepository,
            IGenericRepository<SalesOrders> salesOrderRepository,
            IGenericRepository<SalesOrdersVersions> salesVersionRepository,
            IGenericRepository<Products> productRepository,
            IGenericRepository<PriceQuotationVersions> priceQuotationVersionsRepository,
            ISalesOrder salesOrderService,
            IGenericRepository<Locations> locationRepository,
            IGenericRepository<GCTL.Data.Models.Inventory> inventoryRepository)
            : base(translateService, userProfileService)
        {
            _priceQuotationRepository = priceQuotationRepository;
            _priceQuotationItemRepository = priceQuotationItemRepository;
            _priceQuotationService = priceQuotationService;
            _unitTypeRepository = unitTypeRepository;
            _customerRepository = customerRepository;
            _customerAddressRepository = customerAddressRepository;
            _addressRepository = addressRepository;
            _userInfoService = userInfoService;
            _priceQuotationVersionRepository = priceQuotationVersionRepository;
            _salesOrderRepository = salesOrderRepository;
            _salesVersionRepository = salesVersionRepository;
            _productRepository = productRepository;
            _priceQuotationVersionsRepository = priceQuotationVersionsRepository;
            _salesOrderService = salesOrderService;
            _locationRepository = locationRepository;
            _inventoryRepository = inventoryRepository;
        }

        #endregion

        #region READ-ONLY MODE - View Quotation
        public IActionResult Index(int id)
        {
            ViewBag.location = new SelectList(_locationRepository.AllActive().Select(e => new { Id = e.LocationID, Name = e.LocationName + " (" + e.LocationCode + ")" }).ToList(), "Id", "Name");

            ViewBag.Unit = new SelectList(_unitTypeRepository.AllActive().ToList(), "UnitTypeID", "UnitTypeName");
            ViewBag.IsEditMode = false; // Read-only mode

            SetSmartPageCode(90259000);


            var quotation = _priceQuotationVersionRepository.AllActive()
                .Include(e=>e.PriceQuotation)
                .Include(e=>e.PriceQuotationVersionItems).ThenInclude(e=>e.Product)
                .Include(e => e.CreatedByNavigation)
                .Include(e => e.UpdatedByNavigation)
                .Include(e => e.Location)
                .FirstOrDefault(e => e.PriceQuotationVersionID == id);

            


            if (quotation == null)
            {
                return NotFound();
            }

            var salesOrder = _salesVersionRepository.AllActive().Include(e => e.SalesOrders)
                .Where(e => e.SalesOrders.PriceQuotationID == quotation.PriceQuotationID && (e.IsFinal == true || e.IsDraft == true)).FirstOrDefault();

            var versions = _priceQuotationVersionRepository.AllActive().Where(e => e.PriceQuotationID == quotation.PriceQuotationID).Select(e=>new PriceQuotationVersionViewModel
            {
                id = e.PriceQuotationVersionID,
                number = e.PriceQuotation.QuotationNumber,
                version = e.Version,
                draft = e.IsDraft,
                draftSign = e.IsDraft != true ? "" : "(Draft)",
                finalSign = e.IsFinalVersion == true && e.IsDraft != true ? "(Final)" : "" ,
                current = e.PriceQuotationVersionID == id ? "current" : "",
                isFinal = e.IsFinalVersion == true && e.IsDraft != true ? true : false,
            }).ToList();


            var company = (from customer in _customerRepository.AllActive()
                           join custAddr in _customerAddressRepository.AllActive()
                               on customer.CustomerID equals custAddr.CustomerID
                           join address in _addressRepository.AllActive()
                               on custAddr.AddressID equals address.AddressID
                           where customer.CustomerID == quotation.CustomerID
                           select new
                           {
                               Id = customer.CustomerID,
                               CompanyName = customer.FullName,
                               ContactName = address.FirstName + " " + address.LastName,
                               address.Email,
                               address.Phone,
                               AddressLine1 = address.FullAddress,
                               AddressLine2 = address.Additionaladdress,
                               TaxNumber = address.OtherPhone

                           }).FirstOrDefault();

            var vm = new PriceQuotationDetailsViewModel
            {
                Id = quotation.PriceQuotationVersionID,
                InvoiceDate = quotation.QuotationDate,
                DueDate = DateTime.Today.AddDays(30),
                InvoiceNumber = quotation.PriceQuotation?.QuotationNumber,
                OtherNumber = quotation.OtherNumber,
                SelectedCustomerId = quotation.CustomerID,
                Items = quotation.PriceQuotationVersionItems.Select(m => new QuotationItemDetails
                {
                    SL = m.PriceQuotationVersionItemID,
                    Description = m.Description,
                    UnitName = m.Product != null ? m.Product.ProductName : "",
                    Area = m.Area ?? 0m,
                    Rate = m.Rate ?? 0m,
                    PercentInBill = 100,
                    Product = m.ProductID,
                    Stock = _inventoryRepository.AllActive().Where(e => e.ProductID == m.ProductID && e.LocationID == quotation.LocationID).Sum(e => e.Quantity - e.ReservedQuantity)

                }).ToList(),
                RetentionPercent = quotation.VatPercentage ?? 0m,
                Note = quotation.Note,

                // Sidebar data
                Status = QuotationStatus.Draft, // quotation.IsDraft == true ? QuotationStatus.Draft : QuotationStatus.Sent, // TODO: Get from database when you add Status column
                CreatedByName = quotation.CreatedByNavigation != null ? quotation.CreatedByNavigation.FirstName + " " + quotation.CreatedByNavigation.LastName  : "Unknown",
                CreatedAt = quotation.CreatedAt,
                //UpdatedByName = quotation.UpdatedByNavigation?.FirstName,
                UpdatedByName = quotation.UpdatedByNavigation != null ? quotation.UpdatedByNavigation.FirstName + " " + quotation.UpdatedByNavigation.LastName : "Unknown",

                UpdatedAt = quotation.UpdatedAt,

                CustomerData = new CustomerDetailsViewModel()
                {
                    CompanyName = company != null ? company.CompanyName : "",
                    AddressLine1 = company != null ? company.CompanyName : "",
                    ContactName = company != null ? company.ContactName : "",
                    Email = company != null ? company.Email : "",
                     
                },

                LocationName = quotation.Location.LocationName + " (" + quotation.Location.LocationCode + ")" ,

                // TODO: Load from WorkOrders table when you create it
                // ConvertedToWorkOrderId = quotation.WorkOrderID,
                // WorkOrderNumber = quotation.WorkOrder?.WorkOrderNumber
            };

            // Calculate totals for display
            vm.SubTotal = vm.Items.Sum(i => i.AmountPerPercent);
            vm.RetentionAmount = vm.SubTotal * vm.RetentionPercent / 100;
            vm.GrandTotal = vm.SubTotal - vm.RetentionAmount;

            // Create sidebar view model
            var sidebarVm = new QuotationSidebarDetailsViewModel
            {
                QuotationId = vm.Id,
                QuotationIdList = versions,
                QuotationNumber = vm.InvoiceNumber,
                Status = vm.Status,
                CreatedByName = vm.CreatedByName,
                CreatedAt = vm.CreatedAt,
                UpdatedByName = vm.UpdatedByName,
                UpdatedAt = vm.UpdatedAt,
                WorkOrderId = vm.ConvertedToWorkOrderId,
                WorkOrderNumber = vm.WorkOrderNumber,

                
                HasSalesOrder = salesOrder != null ? true : false,
                SalesOrderNumber = salesOrder != null ? salesOrder.SalesOrders.SalesOrderNumber : "",
                SalesOrderId = salesOrder != null ? salesOrder.SalesOrdersVersionID : 0,

                CanMakeFinal = !versions.Where(e => e.id == vm.Id).Select(e => e.isFinal).FirstOrDefault() // ✅ true if not current
            };

            if (quotation.IsFinalVersion == true)
            {
                sidebarVm.CanCovertSalesOrder = salesOrder != null ? false : true ;
            }

            ViewBag.SidebarData = sidebarVm;

            return View(vm);
        }
        #endregion

        #region EDIT MODE - Edit Quotation
        public IActionResult Edit(int id)
        {
            ViewBag.product = new SelectList(_productRepository.AllActive().ToList(), "ProductID", "ProductName");
            ViewBag.Unit = new SelectList(_unitTypeRepository.AllActive().ToList(), "UnitTypeID", "UnitTypeName");
            ViewBag.IsEditMode = true; // Edit mode
            ViewBag.location = new SelectList(_locationRepository.AllActive().Select(e => new { Id = e.LocationID, Name = e.LocationName + " (" + e.LocationCode + ")" }).ToList(), "Id", "Name");




            var quotation = _priceQuotationVersionRepository.AllActive()
              .Include(e => e.PriceQuotation)
              .Include(e => e.PriceQuotationVersionItems)
              .Include(e => e.CreatedByNavigation)
              .Include(e => e.UpdatedByNavigation)
              .FirstOrDefault(e => e.PriceQuotationVersionID == id);

            if (quotation == null)
            {
                return NotFound();
            }

            var versions = _priceQuotationVersionRepository.AllActive().Where(e => e.PriceQuotationID == quotation.PriceQuotationID).Select(e => new PriceQuotationVersionViewModel
            {
                id = e.PriceQuotationVersionID,
                number = e.PriceQuotation.QuotationNumber,
                version = e.Version,
                draft = e.IsDraft,
                draftSign = e.IsDraft != true ? "" : "(Draft)",
                finalSign = e.IsFinalVersion == true && e.IsDraft != true ? "(Final)" : "",
                current = e.PriceQuotationVersionID == id ? "current" : ""
            }).ToList();

            var vm = new PriceQuotationDetailsViewModel
            {
                Id = quotation.PriceQuotationVersionID,
                InvoiceDate = quotation.QuotationDate,
                DueDate = DateTime.Today.AddDays(30),
                InvoiceNumber = quotation.PriceQuotation.QuotationNumber,
                OtherNumber = quotation.OtherNumber,
                SelectedCustomerId = quotation.CustomerID,
                Items = quotation.PriceQuotationVersionItems.Select(m => new QuotationItemDetails
                {
                    SL = m.PriceQuotationVersionItemID,
                    Description = m.Description,
                    Product = m.ProductID ?? 0,
                    UnitName = m.UnitType != null ? m.UnitType.UnitTypeName : "",
                    Area = m.Area ?? 0m,
                    Rate = m.Rate ?? 0m,
                    PercentInBill = 100,
                    Stock = _inventoryRepository.AllActive().Where(e => e.ProductID == m.ProductID && e.LocationID == quotation.LocationID).Sum(e => e.Quantity - e.ReservedQuantity)


                }).ToList(),
                 RetentionPercent = quotation.VatPercentage ?? 0m,
                 Note = quotation.Note,
                 LocationId = quotation.LocationID,

                // Sidebar data
                Status = QuotationStatus.Draft, // TODO: Get from database when you add Status column
                
                CreatedByName = quotation.CreatedByNavigation != null ? quotation.CreatedByNavigation.FirstName + " " + quotation.CreatedByNavigation.LastName : "Unknown",

                CreatedAt = quotation.CreatedAt,
                UpdatedByName = quotation.UpdatedByNavigation != null ? quotation.UpdatedByNavigation.FirstName + " " + quotation.UpdatedByNavigation.LastName : "Unknown",
                UpdatedAt = quotation.UpdatedAt,
            };

            // Calculate totals for display
            vm.SubTotal = vm.Items.Sum(i => i.AmountPerPercent);
            vm.RetentionAmount = vm.SubTotal * vm.RetentionPercent / 100;
            vm.GrandTotal = vm.SubTotal - vm.RetentionAmount;

            // Create sidebar view model
            var sidebarVm = new QuotationSidebarDetailsViewModel
            {
                QuotationId = vm.Id,
                QuotationIdList = versions,
                QuotationNumber = vm.InvoiceNumber,
                Status = vm.Status,
                CreatedByName = vm.CreatedByName,
                CreatedAt = vm.CreatedAt,
                UpdatedByName = vm.UpdatedByName,
                UpdatedAt = vm.UpdatedAt,
                WorkOrderId = vm.ConvertedToWorkOrderId,
                WorkOrderNumber = vm.WorkOrderNumber
            };

            ViewBag.SidebarData = sidebarVm;

            // Return the same view but in edit mode
            return View("Index", vm);
        }

        #endregion

        #region Set default

        public async Task<IActionResult> SetDefault(int id)
        {
            ViewBag.Unit = new SelectList(_unitTypeRepository.AllActive().ToList(), "UnitTypeID", "UnitTypeName");
            ViewBag.IsEditMode = false; // Read-only mode

            var quotation = _priceQuotationRepository.AllActive()
                .Include(e => e.PriceQuotationVersions)
                //.ThenInclude(e => e.UnitType)
                //.Include(e => e.Customer)
                .Include(e => e.CreatedByNavigation)
                .Include(e => e.UpdatedByNavigation)
                .FirstOrDefault(e => e.PriceQuotationID == id);



            if (quotation == null)
            {
                return NotFound();
            }

            var quora = await _priceQuotationRepository.AllActive().Where(e => e.QuotationNumber == quotation.QuotationNumber).ToListAsync();
           // quora.ForEach(e => e.IsFinalVersion = false);
            await _priceQuotationRepository.UpdateRangeAsync(quora);

           // quotation.IsFinalVersion = true;
            await _priceQuotationRepository.UpdateAsync(quotation);

            var versions = _priceQuotationRepository.AllActive().Where(e => e.QuotationNumber == quotation.QuotationNumber).Select(e => new PriceQuotationVersionViewModel
            {
                id = e.PriceQuotationID,
                number = e.QuotationNumber,
                version = e.PriceQuotationVersions.Count,
            }).ToList();


            var company = (from customer in _customerRepository.AllActive()
                           join custAddr in _customerAddressRepository.AllActive()
                               on customer.CustomerID equals custAddr.CustomerID
                           join address in _addressRepository.AllActive()
                               on custAddr.AddressID equals address.AddressID
                         //  where customer.CustomerID == quotation.CustomerID
                           select new
                           {
                               Id = customer.CustomerID,
                               CompanyName = customer.FullName,
                               ContactName = address.FirstName + " " + address.LastName,
                               address.Email,
                               address.Phone,
                               AddressLine1 = address.FullAddress,
                               AddressLine2 = address.Additionaladdress,
                               TaxNumber = address.OtherPhone

                           }).FirstOrDefault();

            var vm = new PriceQuotationDetailsViewModel
            {
                Id = quotation.PriceQuotationID,
               // InvoiceDate = quotation.QuotationDate,
                DueDate = DateTime.Today.AddDays(30),
                InvoiceNumber = quotation.QuotationNumber,
               // OtherNumber = quotation.OtherNumber,
                //SelectedCustomerId = quotation.CustomerID,
                Items = quotation.PriceQuotationVersions.Select(m => new QuotationItemDetails
                {
                    //SL = m.PriceQuotationItemID,
                    //Description = m.Description,
                    //UnitName = m.UnitType != null ? m.UnitType.UnitTypeName : "",
                    //Area = m.Area ?? 0m,
                    //Rate = m.Rate ?? 0m,
                    //PercentInBill = 100
                }).ToList(),
               // RetentionPercent = quotation.VatPercentage ?? 0m,
               // Note = quotation.Note,

                // Sidebar data
                Status = QuotationStatus.Draft, // TODO: Get from database when you add Status column
                CreatedByName = quotation.CreatedByNavigation?.FirstName ?? "Unknown",
                CreatedAt = quotation.CreatedAt,
                UpdatedByName = quotation.UpdatedByNavigation?.FirstName,
                UpdatedAt = quotation.UpdatedAt,

                CustomerData = new CustomerDetailsViewModel()
                {
                    CompanyName = company != null ? company.CompanyName : "",
                    AddressLine1 = company != null ? company.CompanyName : "",
                    ContactName = company != null ? company.ContactName : "",
                    Email = company != null ? company.Email : "",

                }

                // TODO: Load from WorkOrders table when you create it
                // ConvertedToWorkOrderId = quotation.WorkOrderID,
                // WorkOrderNumber = quotation.WorkOrder?.WorkOrderNumber
            };

            // Calculate totals for display
            vm.SubTotal = vm.Items.Sum(i => i.AmountPerPercent);
            vm.RetentionAmount = vm.SubTotal * vm.RetentionPercent / 100;
            vm.GrandTotal = vm.SubTotal - vm.RetentionAmount;

            // Create sidebar view model
            var sidebarVm = new QuotationSidebarDetailsViewModel
            {
                QuotationId = vm.Id,
                QuotationIdList = versions,
                QuotationNumber = vm.InvoiceNumber,
                Status = vm.Status,
                CreatedByName = vm.CreatedByName,
                CreatedAt = vm.CreatedAt,
                UpdatedByName = vm.UpdatedByName,
                UpdatedAt = vm.UpdatedAt,
                WorkOrderId = vm.ConvertedToWorkOrderId,
                WorkOrderNumber = vm.WorkOrderNumber
            };

            ViewBag.SidebarData = sidebarVm;

            return View("Index", vm);
        }




        #endregion

        #region Sidebar

        [HttpPost]
        public async Task<IActionResult> Duplicate(int id, BaseViewModel vm)
        {
            await _priceQuotationRepository.OpenTransactionAsync();

            try
            {
                var version = _priceQuotationVersionRepository.AllActive()
                    .Include(q => q.PriceQuotation)
                    .Include(q => q.PriceQuotationVersionItems)
                    .Where(q => q.PriceQuotationVersionID == id)
                    .FirstOrDefault();

                if (version == null)
                {
                    return Json(new { success = false, message = "Quotation not found" });
                }

                // 🔹 Create duplicate quotation header
                var duplicate = new PriceQuotations
                {
                    QuotationNumber = await _priceQuotationService.GetNextPQcode(),
                    CreatedBy = vm.CreatedBy,
                    CreatedAt = DateTime.Now,
                   
                };

                await _priceQuotationRepository.AddAsync(duplicate, vm);
                await _userInfoService.ActionLogAsync("PriceQuotationDup", ActionName.DataAdd, null, duplicate, duplicate.PriceQuotationID, vm);

              
                var duplicateVersion = new PriceQuotationVersions
                {
                    PriceQuotationID = duplicate.PriceQuotationID,
                    CustomerID = version.CustomerID,
                    QuotationDate = DateTime.Now,
                    OtherNumber = version.OtherNumber,
                    VatPercentage = version.VatPercentage,
                    Note = version.Note + " (Copy)",
                    Version = 1, // reset to first version for duplicate
                    IsFinalVersion = true,
                    IsDraft = version.IsDraft,
                    CreatedBy = vm.CreatedBy,
                    CreatedAt = DateTime.Now,
                    LocationID = version.LocationID
                };

                await _priceQuotationVersionRepository.AddAsync(duplicateVersion, vm);
                await _userInfoService.ActionLogAsync("PriceQuotationDup", ActionName.DataAdd, null, duplicateVersion, duplicateVersion.PriceQuotationVersionID, vm);

                // 🔹 Duplicate items for this version
                foreach (var item in version.PriceQuotationVersionItems)
                {
                    var duplicateItem = new PriceQuotationVersionItems
                    {
                        PriceQuotationVersionID = duplicateVersion.PriceQuotationVersionID,
                        Description = item.Description,
                        UnitTypeID = item.UnitTypeID,
                        ProductID = item.ProductID,
                        Area = item.Area,
                        Rate = item.Rate,
                        CreatedBy = vm.CreatedBy,
                        CreatedAt = DateTime.Now
                    };

                    await _priceQuotationItemRepository.AddAsync(duplicateItem, vm);
                    await _userInfoService.ActionLogAsync("PriceQuotationDup", ActionName.DataAdd, null, duplicateItem, duplicateItem.PriceQuotationVersionItemID, vm);
                }
                

                await _priceQuotationItemRepository.CompleteTransactionAsync();

                return Json(new { success = true, newQuotationId = duplicateVersion.PriceQuotationVersionID });
            }
            catch (Exception ex)
            {
                await _priceQuotationItemRepository.AbortTransactionAsync();
                return Json(new { success = false, message = ex.Message });
            }
        }


        


        #endregion

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, int status , BaseViewModel baseView)
        {
            try
            {
                var quotation = await _priceQuotationVersionRepository.All().FirstOrDefaultAsync(e => e.PriceQuotationVersionID == id);
                if (quotation == null)
                {
                    return Json(new { success = false, message = "Quotation not found" });
                }


                var quora = await _priceQuotationVersionRepository.AllActive().Where(e => e.PriceQuotationID == quotation.PriceQuotationID).ToListAsync();
                quora.ForEach(e => e.IsFinalVersion = false);
                quora.ForEach(e => e.IsDraft = false);
                await _priceQuotationVersionRepository.UpdateRangeAsync(quora);

                quotation.IsFinalVersion = true;
                await _priceQuotationVersionRepository.UpdateAsync(quotation);

               


                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult ConvertToWorkOrder(int id)
        {
            try
            {
                var quotation = _priceQuotationRepository.AllActive()
                    //.Include(e => e.PriceQuotationItems)
                    .FirstOrDefault(e => e.PriceQuotationID == id);

                if (quotation == null)
                {
                    return Json(new { success = false, message = "Quotation not found" });
                }

                // TODO: Create WorkOrder table and implement conversion logic
                // Example:
                /*
                var workOrder = new WorkOrders
                {
                    WorkOrderNumber = GenerateWorkOrderNumber(),
                    SourceQuotationID = quotation.PriceQuotationID,
                    CustomerID = quotation.CustomerID,
                    WorkOrderDate = DateTime.Now,
                    Status = WorkOrderStatus.Active,
                    CreatedBy = GetCurrentUserId(),
                    CreatedAt = DateTime.Now
                };
                _workOrderRepository.Add(workOrder);
                _workOrderRepository.SaveChanges();

                // Copy items to work order items
                // Update quotation status to Converted
                quotation.Status = QuotationStatus.Converted;
                quotation.ConvertedToWorkOrderID = workOrder.WorkOrderID;
                _priceQuotationRepository.Update(quotation);
                _priceQuotationRepository.SaveChanges();

                return Json(new { success = true, workOrderId = workOrder.WorkOrderID });
                */

                return Json(new { success = false, message = "Work Order conversion not yet implemented. Please create WorkOrders table first." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        [HttpPost]
        public async Task<IActionResult> ConvertToSalesOrder(int id , BaseViewModel? baseView)
        {
            try
            {
                var quotation = await _priceQuotationVersionsRepository.AllActive()
                   .Include(e => e.PriceQuotationVersionItems)
                   .Include(e => e.PriceQuotation)
                   .FirstOrDefaultAsync(e => e.PriceQuotationVersionID == id);

                if (quotation == null)
                {
                    return Ok( new CommonReturnViewModel() { Success = false, Message = "not found" });
                }

                if (quotation.IsFinalVersion != true)
                {
                    return Ok(new CommonReturnViewModel() { Success = false, Message = "the version is not final" });
                }

                var model = new SalesOrderViewModel()
                {
                    Id = id,
                    OrderDate = DateTime.Now,
                    IsDraft = true,
                    SelectedCustomerId = quotation.CustomerID,
                    SelectedQuotationId = quotation.PriceQuotationVersionID,
                    VatPercent = quotation.VatPercentage ?? 0m,
                    Note = quotation.Note + " (Auto Converted)",
                    LocationId = quotation.LocationID,

                    Items = quotation.PriceQuotationVersionItems.Select(x => new SalesOrderItem()
                    {

                        Description = x.Description,
                        Rate = x.Rate,
                        Quantity = x.Area,
                        //Product = x.UnitTypeID ?? ""
                        LIP = baseView?.LIP ?? "",
                        LMAC = baseView?.LMAC ?? "",

                        Product = (x.ProductID == null || x.ProductID == 0) ? null : x.ProductID


                    }).ToList()

                };

                var result = await _salesOrderService.SaveAsync(model);


                return Ok( new CommonReturnViewModel()
                {
                    Success = result.Success,
                    Data = result.Data,
                    Message = result.Success ? "Quotation converted to Work Order successfully!" : result.Message
                });

            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult SendEmail(int id, string recipientEmail, string subject, string message)
        {
            try
            {
                // TODO: Implement email sending logic
                // Use your email service to send the quotation

                return Json(new { success = false, message = "Email functionality not yet implemented" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public IActionResult PrintPDF(int id)
        {
            // TODO: Implement PDF generation
            return NotFound();
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            try
            {
                var quotation = _priceQuotationRepository.All().FirstOrDefault(e => e.PriceQuotationID == id);
                if (quotation == null)
                {
                    return Json(new { success = false, message = "Quotation not found" });
                }

                // Soft delete
                quotation.DeletedAt = DateTime.Now;
                quotation.DeletedBy = GetCurrentUserId();
                //_priceQuotationRepository.Update(quotation);
                //_priceQuotationRepository.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Helper methods
        private string GenerateNewQuotationNumber()
        {
            // Implement your quotation number generation logic
            return "QUO-" + DateTime.Now.ToString("yyyyMMddHHmmss");
        }

        private int GetCurrentUserId()
        {
            return GetCurrentEmployeeIdAsync().Result ?? 1;
        }
    }
}