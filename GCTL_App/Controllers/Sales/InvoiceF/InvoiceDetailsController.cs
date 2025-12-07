
using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.Sales.InvoiceDetailsF;
using GCTL.Core.ViewModels.Sales.PriceQuotationDetails;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.Language;
using GCTL.Service.Sales.InvoiceF;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GCTL_App.Controllers.Sales.InvoiceF
{
    public class InvoiceDetailsController : BaseController
    {
        #region CTOR

        private readonly IGenericRepository<Invoices> _invoiceRepository;
        private readonly IGenericRepository<InvoiceVersionItems> _invoiceItemRepository;
        private readonly IGenericRepository<InvoicesVersions> _invoiceVersionRepository;
        private readonly IInvoice _invoiceService;
        private readonly IGenericRepository<Products> _productRepository;
        private readonly IGenericRepository<Customers> _customerRepository;
        private readonly IGenericRepository<CustomerAddresses> _customerAddressRepository;
        private readonly IGenericRepository<Addresses> _addressRepository;
        private readonly IGenericRepository<SalesOrders> _salesOrderRepository;
        //private readonly IGenericRepository<PaymentTransactions> _paymentTransactionRepository;
        //private readonly IGenericRepository<PaymentMethods> _paymentMethodRepository;
        private readonly IUserInfoService _userInfoService;

        public InvoiceDetailsController(ITranslateService translateService, IUserProfileService userProfileService, IGenericRepository<Invoices> invoiceRepository, IGenericRepository<InvoiceVersionItems> invoiceItemRepository, IGenericRepository<InvoicesVersions> invoiceVersionRepository, IInvoice invoiceService, IGenericRepository<Products> productRepository, IGenericRepository<Customers> customerRepository, IGenericRepository<CustomerAddresses> customerAddressRepository, IGenericRepository<Addresses> addressRepository, IGenericRepository<SalesOrders> salesOrderRepository, IUserInfoService userInfoService) : base(translateService, userProfileService)
        {
            _invoiceRepository = invoiceRepository;
            _invoiceItemRepository = invoiceItemRepository;
            _invoiceVersionRepository = invoiceVersionRepository;
            _invoiceService = invoiceService;
            _productRepository = productRepository;
            _customerRepository = customerRepository;
            _customerAddressRepository = customerAddressRepository;
            _addressRepository = addressRepository;
            _salesOrderRepository = salesOrderRepository;
            _userInfoService = userInfoService;
        }



        #endregion

        #region READ-ONLY MODE - View Invoice
        public IActionResult Index(int id)
        {
            try
            {
                ViewBag.Products = new SelectList(_productRepository.AllActive().ToList(), "ProductID", "ProductName");
               // ViewBag.PaymentMethods = new SelectList(_paymentMethodRepository.AllActive().ToList(), "PaymentMethodID", "MethodName");
                ViewBag.IsEditMode = false;

                var invoice = _invoiceVersionRepository.AllActive()
                    .Include(e => e.InvoiceVersionItems).ThenInclude(e => e.Product)
                    .Include(e => e.Customer)
                    .Include(e => e.Invoice).ThenInclude(e => e.SalesOrders)
                    .Include(e => e.IBaseBillingAddress)
                    .Include(e => e.IBaseShippingAddress)
                    .Include(e => e.CreatedByNavigation)
                    .Include(e => e.UpdatedByNavigation)
                    //.Include(e => e.PaymentTransactions).ThenInclude(pt => pt.PaymentMethod)
                    .FirstOrDefault(e => e.InvoicesVersionID == id);

                if (invoice == null)
                {
                    return NotFound();
                }

                var company = (from customer in _customerRepository.AllActive()
                               join custAddr in _customerAddressRepository.AllActive()
                                   on customer.CustomerID equals custAddr.CustomerID
                               join address in _addressRepository.AllActive()
                                   on custAddr.AddressID equals address.AddressID
                               where customer.CustomerID == invoice.CustomerID
                               select new
                               {
                                   Id = customer.CustomerID,
                                   CompanyName = customer.FullName,
                                   ContactName = address.FirstName + " " + address.LastName,
                                   Email = address.Email,
                                   Phone = address.Phone,
                                   AddressLine1 = address.FullAddress,
                                   AddressLine2 = address.Additionaladdress,
                                   TaxNumber = address.OtherPhone
                               }).FirstOrDefault();


                var versions = _invoiceVersionRepository.AllActive().Where(e => e.InvoiceNumber == invoice.InvoiceNumber).Select(e => new PriceQuotationVersionViewModel
                {
                    id = e.InvoicesVersionID,
                    number = e.InvoiceNumber,
                    version = e.Version,
                    draft = e.IsDraft,
                    draftSign = e.IsDraft != true ? "" : "(Draft)",
                    finalSign = e.IsFinal == true && e.IsDraft != true ? "(Final)" : ""
                }).ToList();

                var FinalAse = _invoiceVersionRepository.AllActive().FirstOrDefault(e => e.InvoiceNumber == invoice.InvoiceNumber && e.IsFinal == true && e.IsDraft != true);

                //var vm = new InvoiceDetailsViewModel
                //{
                //    Id = invoice.InvoicesVersionID,
                //    InvoiceDate = invoice.InvoiceDate,
                //    InvoiceNumber = invoice.Invoice.InvoiceNumber,
                //    SelectedCustomerId = invoice.CustomerID,
                //    SelectedSalesOrderId = invoice.Invoice.SalesOrdersID,
                //    SalesOrderNumber = invoice.Invoice.SalesOrders?.SalesOrderNumber,
                //    IsDraft = invoice.IsDraft ?? false,
                //    Items = invoice.InvoiceVersionItems.Select(m => new InvoiceItemDetails
                //    {
                //        SL = m.InvoiceVersionItemID,
                //        ProductId = m.ProductID ?? 0,
                //        ProductName = m.Product?.ProductName ?? "",
                //        Quantity = m.Quantity ?? 0m,
                //        UnitPrice = m.UnitPrice ?? 0m
                //    }).ToList(),
                //    VatPercent = invoice.VatPercentage ?? 0m,
                //    SubTotal = invoice.SubTotal ?? 0m,
                //    VatAmount = invoice.VatAmount ?? 0m,
                //    GrandTotal = invoice.GrandTotal ?? 0m,
                //    PaidAmount = invoice.PaidAmount ?? 0m,
                //    OtherReference = invoice.OtherReference,
                //    InvoiceNote = invoice.InvoiceNote,
                //    Finalized = FinalAse != null ? true : false,

                //    // Billing Address
                //    BillingAddress = invoice.IBaseBillingAddress != null ? new AddressViewModel
                //    {
                //        FirstName = invoice.IBaseBillingAddress.FirstName,
                //        LastName = invoice.IBaseBillingAddress.LastName,
                //        FullAddress = invoice.IBaseBillingAddress.FullAddress,
                //        City = invoice.IBaseBillingAddress.City,
                //        State = invoice.IBaseBillingAddress.State,
                //        PostalCode = invoice.IBaseBillingAddress.PostalCode,
                //        Phone = invoice.IBaseBillingAddress.Phone,
                //        Email = invoice.IBaseBillingAddress.Email
                //    } : null,

                //    // Shipping Address
                //    ShippingAddress = invoice.IBaseShippingAddress != null ? new AddressViewModel
                //    {
                //        FirstName = invoice.IBaseShippingAddress.FirstName,
                //        LastName = invoice.IBaseShippingAddress.LastName,
                //        FullAddress = invoice.IBaseShippingAddress.FullAddress,
                //        City = invoice.IBaseShippingAddress.City,
                //        State = invoice.IBaseShippingAddress.State,
                //        PostalCode = invoice.IBaseShippingAddress.PostalCode,
                //        Phone = invoice.IBaseShippingAddress.Phone,
                //        Email = invoice.IBaseShippingAddress.Email
                //    } : null,

                //    // Payment History
                //    PaymentHistory = invoice.PaymentTransactions.Select(pt => new PaymentHistoryViewModel
                //    {
                //        PaymentTransactionID = pt.PaymentTransactionID,
                //        TransactionRefNo = pt.TransactionRefNo,
                //        TransactionDate = pt.TransactionDate,
                //        PaymentMethodName = pt.PaymentMethod?.MethodName ?? "",
                //        Amount = pt.Amount,
                //        Status = pt.Status
                //    }).ToList(),

                //    // Sidebar data
                //    CreatedByName = invoice.CreatedByNavigation != null ? invoice.CreatedByNavigation.FirstName + " " + invoice.CreatedByNavigation.LastName : "Unknown",
                //    CreatedAt = invoice.CreatedAt,
                //    UpdatedByName = invoice.UpdatedByNavigation != null ? invoice.UpdatedByNavigation.FirstName + " " + invoice.UpdatedByNavigation.LastName : "Unknown",
                //    UpdatedAt = invoice.UpdatedAt,

                //    CustomerData = new CustomerDetailsViewModel()
                //    {
                //        CompanyName = company != null ? company.CompanyName : "",
                //        AddressLine1 = company != null ? company.AddressLine1 : "",
                //        ContactName = company != null ? company.ContactName : "",
                //        Email = company != null ? company.Email : "",
                //        Phone = company != null ? company.Phone : ""
                //    }
                //};


                var vm = new InvoiceDetailsViewModel
                {
                    Id = invoice.InvoicesVersionID,
                    InvoiceDate = invoice.InvoiceDate,
                    InvoiceNumber = invoice.Invoice?.InvoiceNumber ?? "N/A",
                    SelectedCustomerId = invoice.CustomerID,
                    SelectedSalesOrderId = invoice.Invoice?.SalesOrdersID,
                    SalesOrderNumber = invoice.Invoice?.SalesOrders?.SalesOrderNumber, // safe chaining
                    IsDraft = invoice.IsDraft ?? false,

                    Items = invoice.InvoiceVersionItems?.Select(m => new InvoiceItemDetails
                    {
                        SL = m.InvoiceVersionItemID,
                        ProductId = m.ProductID ?? 0,
                        ProductName = m.Product?.ProductName ?? "Unknown Product",
                        Quantity = m.Quantity ?? 0m,
                        UnitPrice = m.UnitPrice ?? 0m
                    }).ToList() ?? new List<InvoiceItemDetails>(),

                    VatPercent = invoice.VatPercentage ?? 0m,
                    SubTotal = invoice.SubTotal ?? 0m,
                    VatAmount = invoice.VatAmount ?? 0m,
                    GrandTotal = invoice.GrandTotal ?? 0m,
                    PaidAmount = invoice.PaidAmount ?? 0m,
                    OtherReference = invoice.OtherReference,
                    InvoiceNote = invoice.InvoiceNote,
                    Finalized = FinalAse != null,

                    BillingAddress = invoice.IBaseBillingAddress != null ? new AddressViewModel
                    {
                        FirstName = invoice.IBaseBillingAddress.FirstName,
                        LastName = invoice.IBaseBillingAddress.LastName,
                        FullAddress = invoice.IBaseBillingAddress.FullAddress,
                        City = invoice.IBaseBillingAddress.City,
                        State = invoice.IBaseBillingAddress.State,
                        PostalCode = invoice.IBaseBillingAddress.PostalCode,
                        Phone = invoice.IBaseBillingAddress.Phone,
                        Email = invoice.IBaseBillingAddress.Email
                    } : null,

                    ShippingAddress = invoice.IBaseShippingAddress != null ? new AddressViewModel 
                    {
                        FirstName = invoice.IBaseBillingAddress.FirstName,
                        LastName = invoice.IBaseBillingAddress.LastName,
                        FullAddress = invoice.IBaseBillingAddress.FullAddress,
                        City = invoice.IBaseBillingAddress.City,
                        State = invoice.IBaseBillingAddress.State,
                        PostalCode = invoice.IBaseBillingAddress.PostalCode,
                        Phone = invoice.IBaseBillingAddress.Phone,
                        Email = invoice.IBaseBillingAddress.Email
                    } : null,

                    PaymentHistory = 
                    //invoice.PaymentTransactions?.Select(pt => new PaymentHistoryViewModel
                    //{
                    //    PaymentTransactionID = pt.PaymentTransactionID,
                    //    TransactionRefNo = pt.TransactionRefNo,
                    //    TransactionDate = pt.TransactionDate,
                    //    PaymentMethodName = pt.PaymentMethod?.MethodName ?? "Unknown",
                    //    Amount = pt.Amount,
                    //    Status = pt.Status
                    //}).ToList() ?? 
                    new List<PaymentHistoryViewModel>(),

                    CreatedByName = invoice.CreatedByNavigation != null
        ? $"{invoice.CreatedByNavigation.FirstName} {invoice.CreatedByNavigation.LastName}"
        : "Unknown",

                    UpdatedByName = invoice.UpdatedByNavigation != null
        ? $"{invoice.UpdatedByNavigation.FirstName} {invoice.UpdatedByNavigation.LastName}"
        : "Unknown",

                    CustomerData = new CustomerDetailsViewModel()
                    {
                        CompanyName = company?.CompanyName ?? "",
                        AddressLine1 = company?.AddressLine1 ?? "",
                        ContactName = company?.ContactName ?? "",
                        Email = company?.Email ?? "",
                        Phone = company?.Phone ?? ""
                    }
                };

                // Create sidebar view model
                var sidebarVm = new InvoiceSidebarDetailsViewModel
                {
                    InvoiceId = vm.Id,
                    InvoiceIdList = versions,
                    InvoiceNumber = vm.InvoiceNumber,
                    SalesOrderNumber = vm.SalesOrderNumber,
                    IsDraft = vm.IsDraft,
                    TotalAmount = vm.GrandTotal,
                    PaidAmount = vm.PaidAmount,
                    DueAmount = vm.GrandTotal - vm.PaidAmount,
                    CreatedByName = vm.CreatedByName,
                    CreatedAt = vm.CreatedAt,
                    UpdatedByName = vm.UpdatedByName,
                    UpdatedAt = vm.UpdatedAt
                };

                ViewBag.SidebarData = sidebarVm;

                return View(vm);
            }
            catch (Exception)
            {

                throw;
            }
            
        }
        #endregion

        #region EDIT MODE - Edit Invoice
        public IActionResult Edit(int id)
        {
            ViewBag.Products = new SelectList(_productRepository.AllActive().ToList(), "ProductID", "ProductName");
            //ViewBag.PaymentMethods = new SelectList(_paymentMethodRepository.AllActive().ToList(), "PaymentMethodID", "MethodName");
            ViewBag.IsEditMode = true;

            var invoice = _invoiceVersionRepository.AllActive()
                .Include(e => e.InvoiceVersionItems)
                .Include(e => e.CreatedByNavigation)
                .Include(e => e.UpdatedByNavigation)
                .Include(e => e.Invoice)
                .ThenInclude(e => e.SalesOrders)
                .Include(e => e.IBaseBillingAddress)
                .Include(e => e.IBaseShippingAddress)
               // .Include(e => e.PaymentTransactions)
                .FirstOrDefault(e => e.InvoicesVersionID == id);

            if (invoice == null)
            {
                return NotFound();
            }


            var versions = _invoiceVersionRepository.AllActive().Where(e => e.InvoiceID == invoice.InvoiceID).Select(e => new PriceQuotationVersionViewModel
            {
                id = e.InvoicesVersionID,
                number = e.Invoice.InvoiceNumber,
                version = e.Version,
                draft = e.IsDraft,
                draftSign = e.IsDraft != true ? "" : "(Draft)",
                finalSign = e.IsFinal == true && e.IsDraft != true ? "(Final)" : ""
            }).ToList();

            var ifFinal = _invoiceVersionRepository.AllActive().FirstOrDefault(e => e.InvoiceNumber == invoice.InvoiceNumber && e.IsFinal == true && e.IsDraft != true);


            //var vm = new InvoiceDetailsViewModel
            //{
            //    Id = invoice.InvoicesVersionID,
            //    InvoiceDate = invoice.InvoiceDate,
            //    InvoiceNumber = invoice.Invoice.InvoiceNumber,
            //    SelectedCustomerId = invoice.CustomerID,
            //    SelectedSalesOrderId = invoice.Invoice.SalesOrdersID,
            //    SalesOrderNumber = invoice.Invoice.SalesOrders.SalesOrderNumber,
            //    IsDraft = invoice.IsDraft ?? false,
            //    Items = invoice.InvoiceVersionItems.Select(m => new InvoiceItemDetails
            //    {
            //        SL = m.InvoiceVersionItemID,
            //        ProductId = m.ProductID ?? 0,
            //        Quantity = m.Quantity ?? 0m,
            //        UnitPrice = m.UnitPrice ?? 0m
            //    }).ToList(),
            //    VatPercent = invoice.VatPercentage ?? 0m,
            //    SubTotal = invoice.SubTotal ?? 0m,
            //    VatAmount = invoice.VatAmount ?? 0m,
            //    GrandTotal = invoice.GrandTotal ?? 0m,
            //    PaidAmount = invoice.PaidAmount ?? 0m,
            //    OtherReference = invoice.OtherReference,
            //    InvoiceNote = invoice.InvoiceNote,
            //    Finalized = ifFinal != null ? true : false,


            //    BillingAddress = invoice.IBaseBillingAddress != null ? new AddressViewModel
            //    {
            //        FirstName = invoice.IBaseBillingAddress.FirstName,
            //        LastName = invoice.IBaseBillingAddress.LastName,
            //        FullAddress = invoice.IBaseBillingAddress.FullAddress,
            //        City = invoice.IBaseBillingAddress.City,
            //        State = invoice.IBaseBillingAddress.State,
            //        PostalCode = invoice.IBaseBillingAddress.PostalCode,
            //        Phone = invoice.IBaseBillingAddress.Phone,
            //        Email = invoice.IBaseBillingAddress.Email
            //    } : null,

            //    // Shipping Address
            //    ShippingAddress = invoice.IBaseShippingAddress != null ? new AddressViewModel
            //    {
            //        FirstName = invoice.IBaseShippingAddress.FirstName,
            //        LastName = invoice.IBaseShippingAddress.LastName,
            //        FullAddress = invoice.IBaseShippingAddress.FullAddress,
            //        City = invoice.IBaseShippingAddress.City,
            //        State = invoice.IBaseShippingAddress.State,
            //        PostalCode = invoice.IBaseShippingAddress.PostalCode,
            //        Phone = invoice.IBaseShippingAddress.Phone,
            //        Email = invoice.IBaseShippingAddress.Email
            //    } : null,





            //    // Sidebar data
            //    CreatedByName = invoice.CreatedByNavigation != null
            //        ? invoice.CreatedByNavigation.FirstName + " " + invoice.CreatedByNavigation.LastName
            //        : "Unknown",
            //    CreatedAt = invoice.CreatedAt,
            //    UpdatedByName = invoice.UpdatedByNavigation?.FirstName,
            //    UpdatedAt = invoice.UpdatedAt,
            //};


            var vm = new InvoiceDetailsViewModel
            {
                Id = invoice.InvoicesVersionID,
                InvoiceDate = invoice.InvoiceDate,
                InvoiceNumber = invoice.Invoice?.InvoiceNumber ?? "N/A",
                SelectedCustomerId = invoice.CustomerID,
                SelectedSalesOrderId = invoice.Invoice?.SalesOrdersID,
                SalesOrderNumber = invoice.Invoice?.SalesOrders?.SalesOrderNumber, // safe chaining
                IsDraft = invoice.IsDraft ?? false,

                Items = invoice.InvoiceVersionItems?.Select(m => new InvoiceItemDetails
                {
                    SL = m.InvoiceVersionItemID,
                    ProductId = m.ProductID ?? 0,
                    ProductName = m.Product?.ProductName ?? "Unknown Product",
                    Quantity = m.Quantity ?? 0m,
                    UnitPrice = m.UnitPrice ?? 0m
                }).ToList() ?? new List<InvoiceItemDetails>(),

                VatPercent = invoice.VatPercentage ?? 0m,
                SubTotal = invoice.SubTotal ?? 0m,
                VatAmount = invoice.VatAmount ?? 0m,
                GrandTotal = invoice.GrandTotal ?? 0m,
                PaidAmount = invoice.PaidAmount ?? 0m,
                OtherReference = invoice.OtherReference,
                InvoiceNote = invoice.InvoiceNote,
                

                BillingAddress = invoice.IBaseBillingAddress != null ? new AddressViewModel
                {
                    FirstName = invoice.IBaseBillingAddress.FirstName,
                    LastName = invoice.IBaseBillingAddress.LastName,
                    FullAddress = invoice.IBaseBillingAddress.FullAddress,
                    City = invoice.IBaseBillingAddress.City,
                    State = invoice.IBaseBillingAddress.State,
                    PostalCode = invoice.IBaseBillingAddress.PostalCode,
                    Phone = invoice.IBaseBillingAddress.Phone,
                    Email = invoice.IBaseBillingAddress.Email
                } : null,

                ShippingAddress = invoice.IBaseShippingAddress != null ? new AddressViewModel
                {
                    FirstName = invoice.IBaseBillingAddress.FirstName,
                    LastName = invoice.IBaseBillingAddress.LastName,
                    FullAddress = invoice.IBaseBillingAddress.FullAddress,
                    City = invoice.IBaseBillingAddress.City,
                    State = invoice.IBaseBillingAddress.State,
                    PostalCode = invoice.IBaseBillingAddress.PostalCode,
                    Phone = invoice.IBaseBillingAddress.Phone,
                    Email = invoice.IBaseBillingAddress.Email
                } : null,

                PaymentHistory = 
                //invoice.PaymentTransactions?.Select(pt => new PaymentHistoryViewModel
                //{
                //    PaymentTransactionID = pt.PaymentTransactionID,
                //    TransactionRefNo = pt.TransactionRefNo,
                //    TransactionDate = pt.TransactionDate,
                //    PaymentMethodName = pt.PaymentMethod?.MethodName ?? "Unknown",
                //    Amount = pt.Amount,
                //    Status = pt.Status
                //}).ToList() ?? 
                new List<PaymentHistoryViewModel>(),

                CreatedByName = invoice.CreatedByNavigation != null
       ? $"{invoice.CreatedByNavigation.FirstName} {invoice.CreatedByNavigation.LastName}"
       : "Unknown",

                UpdatedByName = invoice.UpdatedByNavigation != null
       ? $"{invoice.UpdatedByNavigation.FirstName} {invoice.UpdatedByNavigation.LastName}"
       : "Unknown",

               
            };


            // Create sidebar view model
            var sidebarVm = new InvoiceSidebarDetailsViewModel
            {
                InvoiceId = vm.Id,
                InvoiceIdList = versions,
                InvoiceNumber = vm.InvoiceNumber,
                SalesOrderNumber = vm.SalesOrderNumber,
                IsDraft = vm.IsDraft,
                TotalAmount = vm.GrandTotal,
                PaidAmount = vm.PaidAmount,
                DueAmount = vm.GrandTotal - vm.PaidAmount,
                CreatedByName = vm.CreatedByName,
                CreatedAt = vm.CreatedAt,
                UpdatedByName = vm.UpdatedByName,
                UpdatedAt = vm.UpdatedAt
            };

            ViewBag.SidebarData = sidebarVm;

            return View("Index", vm);
        }
        #endregion

        #region Sidebar Actions

        [HttpPost]
        public async Task<IActionResult> Duplicate(int id, BaseViewModel vm)
        {
            await _invoiceRepository.BeginTransactionAsync();
                   return Json(new { success = false, message = "Invoice not found" });

            //try
            //{
            //    var original = _invoiceRepository.AllActive()
            //        .Include(e => e.InvoiceItems)
            //        .FirstOrDefault(e => e.InvoiceID == id);

            //    if (original == null)
            //    {
            //        return Json(new { success = false, message = "Invoice not found" });
            //    }

            //    // Create duplicate
            //    var duplicate = new Invoices
            //    {
            //        CustomerID = original.CustomerID,
            //        SalesOrderID = original.SalesOrderID,
            //        InvoiceDate = DateTime.Now,
            //        InvoiceNumber = await _invoiceService.GetNextInvoiceCode(),
            //        VatPercentage = original.VatPercentage,
            //        SubTotal = original.SubTotal,
            //        VatAmount = original.VatAmount,
            //        GrandTotal = original.GrandTotal,
            //        InvoiceNote = original.InvoiceNote + " (Copy)",
            //        IsDraft = true,
            //        CreatedAt = DateTime.Now,
            //        CreatedBy = vm.CreatedBy
            //    };

            //    await _invoiceRepository.AddAsync(duplicate, vm);
            //    await _userInfoService.ActionLogAsync("InvoiceDup", ActionName.DataAdd, null, duplicate, duplicate.InvoiceID, vm);

            //    // Copy items
            //    foreach (var item in original.InvoiceItems)
            //    {
            //        var duplicateItem = new InvoiceItems
            //        {
            //            InvoiceID = duplicate.InvoiceID,
            //            ProductID = item.ProductID,
            //            Quantity = item.Quantity,
            //            UnitPrice = item.UnitPrice,
            //            CreatedAt = DateTime.Now,
            //            CreatedBy = vm.CreatedBy
            //        };
            //        await _invoiceItemRepository.AddAsync(duplicateItem, vm);
            //        await _userInfoService.ActionLogAsync("InvoiceDup", ActionName.DataAdd, null, duplicateItem, duplicateItem.InvoiceItemID, vm);
            //    }

            //    await _invoiceItemRepository.CommitTransactionAsync();

            //    return Json(new { success = true, newInvoiceId = duplicate.InvoiceID });
            //}
            //catch (Exception ex)
            //{
            //    await _invoiceItemRepository.RollbackTransactionAsync();
            //    return Json(new { success = false, message = ex.Message });
            //}
        }

        [HttpPost]
        public IActionResult MarkAsPaid(int id)
        {
            try
            {
                var invoice = _invoiceVersionRepository.All().FirstOrDefault(e => e.InvoicesVersionID == id);
                if (invoice == null)
                {
                    return Json(new { success = false, message = "Invoice not found" });
                }

                invoice.IsDraft = false;
                // Add payment transaction logic here

                return Json(new { success = true });
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
                var invoice = _invoiceRepository.All().FirstOrDefault(e => e.InvoiceID == id);
                if (invoice == null)
                {
                    return Json(new { success = false, message = "Invoice not found" });
                }

                // Soft delete
                invoice.DeletedAt = DateTime.Now;
                invoice.DeletedBy = GetCurrentUserId();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        #endregion

        // Helper methods
        private int GetCurrentUserId()
        {
            return 1; // Replace with actual implementation
        }
    }
}
