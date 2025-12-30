
using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.POS.Sales.InvoiceDetailsF;
using GCTL.Core.ViewModels.POS.Sales.PriceQuotationDetails;
using GCTL.Core.ViewModels.POS.Sales.SalesOrders;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.Language;
using GCTL.Service.MasterSetup.Statuse;
using GCTL.Service.POS.Sales.InvoiceF;
using GCTL.Service.POS.Sales.SalesOrderF;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GCTL_App.Controllers.POS.Sales.InvoiceF
{
    public class InvoiceDetailsController : BaseController
    {
        #region CTOR

        private readonly IGenericRepository<Challans> _challanRepository;
        private readonly IGenericRepository<Invoices> _invoiceRepository;
        private readonly IGenericRepository<InvoiceItems> _invoiceItemRepository;
        //private readonly IGenericRepository<InvoicesVersions> _invoiceVersionRepository;
        private readonly IInvoice _invoiceService;
        private readonly IGenericRepository<Products> _productRepository;
        private readonly IGenericRepository<Customers> _customerRepository;
        private readonly IGenericRepository<CustomerAddresses> _customerAddressRepository;
        private readonly IGenericRepository<Addresses> _addressRepository;
        private readonly IGenericRepository<SalesOrders> _salesOrderRepository;
        private readonly IGenericRepository<PaymentTransactions> _paymentTransactionRepository;
        private readonly IGenericRepository<PaymentMethods> _paymentMethodRepository;
        private readonly IUserInfoService _userInfoService;
        private readonly IStatusService _statusService;


        public InvoiceDetailsController(
            ITranslateService translateService,
            IUserProfileService userProfileService,
            IGenericRepository<Invoices> invoiceRepository,
            IGenericRepository<InvoiceItems> invoiceItemRepository,
            IInvoice invoiceService,
            IGenericRepository<Products> productRepository,
            IGenericRepository<Customers> customerRepository,
            IGenericRepository<CustomerAddresses> customerAddressRepository,
            IGenericRepository<Addresses> addressRepository,
            IGenericRepository<SalesOrders> salesOrderRepository,
            IGenericRepository<PaymentTransactions> paymentTransactionRepository,
            IGenericRepository<PaymentMethods> paymentMethodRepository,
            IUserInfoService userInfoService,
            IGenericRepository<Challans> challanRepository,
            IStatusService statusService)
            //IGenericRepository<InvoicesVersions> invoiceVersionRepository)
            : base(translateService, userProfileService)
        {
            _invoiceRepository = invoiceRepository;
            _invoiceItemRepository = invoiceItemRepository;
            _invoiceService = invoiceService;
            _productRepository = productRepository;
            _customerRepository = customerRepository;
            _customerAddressRepository = customerAddressRepository;
            _addressRepository = addressRepository;
            _salesOrderRepository = salesOrderRepository;
            _paymentTransactionRepository = paymentTransactionRepository;
            _paymentMethodRepository = paymentMethodRepository;
            _userInfoService = userInfoService;
            _challanRepository = challanRepository;
            _statusService = statusService;
            //_invoiceVersionRepository = invoiceVersionRepository;
        }

        #endregion

        #region READ-ONLY MODE - View Invoice
        public IActionResult Index(int id)
        {
            SetSmartPageCode(902500);

            try
            {
                ViewBag.Products = new SelectList(_productRepository.AllActive().ToList(), "ProductID", "ProductName");
                ViewBag.PaymentMethods = new SelectList(_paymentMethodRepository.AllActive().ToList(), "PaymentMethodID", "MethodName");
                ViewBag.IsEditMode = false;

                var invoice = _invoiceRepository.AllActive()
                    .Include(e => e.InvoiceItems).ThenInclude(e => e.Product)
                    .Include(e => e.Customer)
                    .Include(e=>e.SalesOrderVersion)
                    .ThenInclude(e => e.SalesOrders)
                    .Include(e => e.IBaseBillingAddress)
                    .Include(e => e.IBaseShippingAddress)
                    .Include(e => e.CreatedByNavigation)
                    .Include(e => e.UpdatedByNavigation)
                    .Include(e => e.PaymentTransactions).ThenInclude(pt => pt.PaymentMethod)
                    .FirstOrDefault(e => e.InvoiceID == id);

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
                                   address.Email,
                                   address.Phone,
                                   AddressLine1 = address.FullAddress,
                                   AddressLine2 = address.Additionaladdress,
                                   TaxNumber = address.OtherPhone
                               }).FirstOrDefault();


                var versions = _invoiceRepository.AllActive().Where(e => e.InvoiceNumber == invoice.InvoiceNumber).Select(e => new PriceQuotationVersionViewModel
                {
                    id = e.InvoiceID,
                    number = e.InvoiceNumber,
                    version = e.Version,
                    draft = e.IsDraft,
                    draftSign = e.IsDraft != true ? "" : "(Draft)",
                    finalSign = e.IsFinal == true && e.IsDraft != true ? "(Final)" : ""
                }).ToList();

                var FinalAse = _invoiceRepository.AllActive().FirstOrDefault(e => e.InvoiceNumber == invoice.InvoiceNumber && e.IsFinal == true && e.IsDraft != true);

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


                var challan = _challanRepository.AllActive().Where(e=>e.InvoiceID == invoice.InvoiceID || e.SalesOrdersVersionID == invoice.SalesOrderVersionID).Select(s => new ShipmentInfo
                {
                    ShipmentId = s.ChallanID,
                    ShipmentNumber = s.ChallanNumber,
                    Status = s.Status != null ? s.Status.StatusName : "Pending",
                    StatusClass = GetShipmentStatusClass("Pending"),

                }).ToList() ?? new List<ShipmentInfo>();

                var challanData = _challanRepository.AllActive().Include(e=>e.ChallanItems).Where(e => e.InvoiceID == invoice.InvoiceID || e.SalesOrdersVersionID == invoice.SalesOrderVersionID).ToList();

                bool isFullyShipped = false;
                bool hasShipmentsStarted = false;

                if (challanData != null && challanData.Any(s => s.DeletedAt == null))
                {
                    hasShipmentsStarted = true;

                    var cancelledStatusId = _statusService.GetStatusID("Cancelled");

                    isFullyShipped = invoice.InvoiceItems.All(soi =>
                    {
                        decimal orderedQty = soi.Quantity ?? 0;

                        decimal shippedQty = challanData
                            .Where(s => s.StatusID != cancelledStatusId && s.DeletedAt == null)
                            .SelectMany(s => s.ChallanItems)
                            .Where(si => si.ProductID == soi.ProductID && si.DeletedAt == null)
                            .Sum(si => si.DeliveredQuantity ?? 0);

                        return orderedQty <= shippedQty;
                    });


                }




                var vm = new InvoiceDetailsViewModel
                {
                    Id = invoice.InvoiceID,
                    InvoiceDate = invoice.InvoiceDate,
                    InvoiceNumber = invoice?.InvoiceNumber ?? "N/A",
                    SelectedCustomerId = invoice?.CustomerID ?? 0,
                    SelectedSalesOrderId = invoice?.SalesOrderVersionID,
                    SalesOrderNumber = invoice?.SalesOrderVersion.SalesOrders?.SalesOrderNumber ?? "", // safe chaining
                    IsDraft = invoice?.IsDraft ?? false,

                    //Items = invoice.InvoiceItems?.Select(m => new InvoiceItemDetails
                    //{
                    //    SL = m.InvoiceItemID,
                    //    ProductId = m.ProductID ?? 0,
                    //    ProductName = m.Product?.ProductName ?? "Unknown Product",
                    //    Quantity = m.Quantity ?? 0m,
                    //    UnitPrice = m.UnitPrice ?? 0m
                    //}).ToList() ?? new List<InvoiceItemDetails>(),

                    //VatPercent = invoice.VatPercentage ?? 0m,
                    //SubTotal = invoice.SubTotal ?? 0m,
                    //VatAmount = invoice.VatAmount ?? 0m,
                    //GrandTotal = invoice.GrandTotal ?? 0m,

                    Items = invoice?.InvoiceItems?.Select(m => new InvoiceItemDetails
                    {
                        SL = m.InvoiceItemID,
                        ProductId = m.ProductID ?? 0,
                        ProductName = m.Product?.ProductName ?? "Unknown Product",
                        Quantity = m.Quantity ?? 0m,
                        UnitPrice = m.UnitPrice ?? 0m,
                        // NEW: Add per-item VAT and Amount
                        VatAmount = m.VatAmount ?? 0m,
                        Amount = m.Amount ?? 0m
                    }).ToList() ?? new List<InvoiceItemDetails>(),

                    // VAT Mode Flags
                    IsVatAfterSubtotal = invoice.IsVatAfterSubtotal,
                    IsItemPriceIncludingVat = invoice.IsItemPriceIncludingVat,
                    IsPriceWithoutVat = invoice.IsPriceWithoutVat,
                    ShowTaxColumn = invoice.ShowTaxColumn,

                    // VAT and AIT
                    VatPercent = invoice.VatPercentage ?? 0m,
                    SubTotal = invoice.SubTotal ?? 0m,
                    VatAmount = invoice.VatAmount ?? 0m,
                    GrossSubtotal = invoice.GrossSubtotal ?? 0m,

                    IsAit = invoice.IsAit,
                    AitPercent = invoice.AitPercent,
                    AitAmount = invoice.AitAmount,

                    GrandTotal = invoice.GrandTotal ?? 0m,

                    PaidAmount = invoice?.PaidAmount ?? 0m,
                    OtherReference = invoice?.OtherReference ?? "",
                    InvoiceNote = invoice?.InvoiceNote ?? "",
                    Finalized = FinalAse != null,

                    BillingAddress = invoice?.IBaseBillingAddress != null ? new AddressViewModel
                    {
                        FirstName = invoice.IBaseBillingAddress.FirstName,
                        LastName = invoice.IBaseBillingAddress.LastName,
                        FullAddress = invoice.IBaseBillingAddress.FullAddress,
                        City = invoice.IBaseBillingAddress.City,
                        State = invoice.IBaseBillingAddress.State,
                        PostalCode = invoice.IBaseBillingAddress.PostalCode,
                        Phone = invoice.IBaseBillingAddress.Phone,
                        Email = invoice.IBaseBillingAddress.Email
                    } : new AddressViewModel(),

                    ShippingAddress = invoice?.IBaseShippingAddress != null ? new AddressViewModel 
                    {
                        FirstName = invoice.IBaseBillingAddress.FirstName,
                        LastName = invoice.IBaseBillingAddress.LastName,
                        FullAddress = invoice.IBaseBillingAddress.FullAddress,
                        City = invoice.IBaseBillingAddress.City,
                        State = invoice.IBaseBillingAddress.State,
                        PostalCode = invoice.IBaseBillingAddress.PostalCode,
                        Phone = invoice.IBaseBillingAddress.Phone,
                        Email = invoice.IBaseBillingAddress.Email
                    } : new AddressViewModel(),

                    PaymentHistory = invoice?.PaymentTransactions?.Select(pt => new PaymentHistoryViewModel
                    {
                        PaymentTransactionID = pt.PaymentTransactionID,
                        TransactionRefNo = pt.TransactionRefNo,
                        TransactionDate = pt.TransactionDate,
                        PaymentMethodName = pt.PaymentMethod?.MethodName ?? "Unknown",
                        Amount = pt.Amount,
                        Status = pt.Status
                    }).ToList() ?? new List<PaymentHistoryViewModel>(),

                    CreatedByName = invoice?.CreatedByNavigation != null ? $"{invoice.CreatedByNavigation.FirstName} {invoice.CreatedByNavigation.LastName}" : "Unknown",

                    UpdatedByName = invoice?.UpdatedByNavigation != null ? $"{invoice.UpdatedByNavigation.FirstName} {invoice.UpdatedByNavigation.LastName}" : "Unknown",

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
                    UpdatedAt = vm.UpdatedAt,
                    SalesOrderId = vm.SelectedSalesOrderId,

                    Shipments = challan,
                    HasShipmentsStarted = hasShipmentsStarted,
                    IsFullyShipped = isFullyShipped,

                };

                if (invoice?.IsFinal == true)
                {
                    sidebarVm.CanCreateShipment = (challan.Count != 0 || challan != null) ? true : false;

                }

                ViewBag.SidebarData = sidebarVm;

                return View(vm);
            }
            catch (Exception)
            {

                throw;
            }
            
        }

        #endregion
        private string GetShipmentStatusClass(string? statusId)
        {
            return statusId switch
            {
                "Pending" => "warning",  // Pending
                "Packed" => "info",     // Packed
                "Shipped" => "primary",  // Shipped
                "In Transit" => "primary",  // In Transit
                "Delivered" => "success",  // Delivered
                "Cancelled" => "danger",   // Cancelled
                _ => "secondary"
            };
        }

        #region EDIT MODE - Edit Invoice
        public IActionResult Edit(int id)
        {
            ViewBag.Products = new SelectList(_productRepository.AllActive().ToList(), "ProductID", "ProductName");
            ViewBag.PaymentMethods = new SelectList(_paymentMethodRepository.AllActive().ToList(), "PaymentMethodID", "MethodName");
            ViewBag.IsEditMode = true;

            var invoice = _invoiceRepository.AllActive()
                .Include(e => e.InvoiceItems)
                .Include(e => e.CreatedByNavigation)
                .Include(e => e.UpdatedByNavigation)
                .Include(e => e.SalesOrderVersion)
                .ThenInclude(e => e.SalesOrders)
                .Include(e => e.IBaseBillingAddress)
                .Include(e => e.IBaseShippingAddress)
                .Include(e => e.PaymentTransactions)
                .FirstOrDefault(e => e.InvoiceID == id);

            if (invoice == null)
            {
                return NotFound();
            }


            var versions = _invoiceRepository.AllActive().Where(e => e.InvoiceID == invoice.InvoiceID).Select(e => new PriceQuotationVersionViewModel
            {
                id = e.InvoiceID,
                number = e.InvoiceNumber,
                version = e.Version,
                draft = e.IsDraft,
                draftSign = e.IsDraft != true ? "" : "(Draft)",
                finalSign = e.IsFinal == true && e.IsDraft != true ? "(Final)" : ""
            }).ToList();

            var ifFinal = _invoiceRepository.AllActive().FirstOrDefault(e => e.InvoiceNumber == invoice.InvoiceNumber && e.IsFinal == true && e.IsDraft != true);


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
                Id = invoice.InvoiceID,
                InvoiceDate = invoice.InvoiceDate,
                InvoiceNumber = invoice?.InvoiceNumber ?? "N/A",
                SelectedCustomerId = invoice?.CustomerID ?? 0,
                SelectedSalesOrderId = invoice?.SalesOrderVersionID,
                SalesOrderNumber = invoice?.SalesOrderVersion.SalesOrders?.SalesOrderNumber ?? "", // safe chaining
                IsDraft = invoice?.IsDraft ?? false,

                //Items = invoice.InvoiceItems?.Select(m => new InvoiceItemDetails
                //{
                //    SL = m.InvoiceItemID,
                //    ProductId = m.ProductID ?? 0,
                //    ProductName = m.Product?.ProductName ?? "Unknown Product",
                //    Quantity = m.Quantity ?? 0m,
                //    UnitPrice = m.UnitPrice ?? 0m
                //}).ToList() ?? new List<InvoiceItemDetails>(),

                //VatPercent = invoice.VatPercentage ?? 0m,
                //SubTotal = invoice.SubTotal ?? 0m,
                //VatAmount = invoice.VatAmount ?? 0m,
                //GrandTotal = invoice.GrandTotal ?? 0m,


                Items = invoice?.InvoiceItems?.Select(m => new InvoiceItemDetails
                {
                    SL = m.InvoiceItemID,
                    ProductId = m.ProductID ?? 0,
                    ProductName = m.Product?.ProductName ?? "Unknown Product",
                    Quantity = m.Quantity ?? 0m,
                    UnitPrice = m.UnitPrice ?? 0m,
                    // NEW: Add per-item VAT and Amount
                    VatAmount = m.VatAmount ?? 0m,
                    Amount = m.Amount ?? 0m
                }).ToList() ?? new List<InvoiceItemDetails>(),

                // VAT Mode Flags
                IsVatAfterSubtotal = invoice?.IsVatAfterSubtotal ?? false,
                IsItemPriceIncludingVat = invoice?.IsItemPriceIncludingVat ?? false,
                IsPriceWithoutVat = invoice?.IsPriceWithoutVat ?? false,
                ShowTaxColumn = invoice?.ShowTaxColumn ?? false,

                // VAT and AIT
                VatPercent = invoice?.VatPercentage ?? 0m,
                SubTotal = invoice?.SubTotal ?? 0m,
                VatAmount = invoice?.VatAmount ?? 0m,
                GrossSubtotal = invoice?.GrossSubtotal ?? 0m,

                IsAit = invoice?.IsAit ?? false,
                AitPercent = invoice?.AitPercent ?? 0m,
                AitAmount = invoice?.AitAmount ?? 0m,

                GrandTotal = invoice?.GrandTotal ?? 0m,


                PaidAmount = invoice?.PaidAmount ?? 0m,
                OtherReference = invoice?.OtherReference ?? "",
                InvoiceNote = invoice?.InvoiceNote ?? "",
                

                BillingAddress = invoice?.IBaseBillingAddress != null ? new AddressViewModel
                {
                    FirstName = invoice.IBaseBillingAddress.FirstName,
                    LastName = invoice.IBaseBillingAddress.LastName,
                    FullAddress = invoice.IBaseBillingAddress.FullAddress,
                    City = invoice.IBaseBillingAddress.City,
                    State = invoice.IBaseBillingAddress.State,
                    PostalCode = invoice.IBaseBillingAddress.PostalCode,
                    Phone = invoice.IBaseBillingAddress.Phone,
                    Email = invoice.IBaseBillingAddress.Email
                } : new AddressViewModel(),

                ShippingAddress = invoice?.IBaseShippingAddress != null ? new AddressViewModel
                {
                    FirstName = invoice.IBaseBillingAddress.FirstName,
                    LastName = invoice.IBaseBillingAddress.LastName,
                    FullAddress = invoice.IBaseBillingAddress.FullAddress,
                    City = invoice.IBaseBillingAddress.City,
                    State = invoice.IBaseBillingAddress.State,
                    PostalCode = invoice.IBaseBillingAddress.PostalCode,
                    Phone = invoice.IBaseBillingAddress.Phone,
                    Email = invoice.IBaseBillingAddress.Email
                } : new AddressViewModel(),

                PaymentHistory = invoice?.PaymentTransactions?.Select(pt => new PaymentHistoryViewModel
                {
                    PaymentTransactionID = pt.PaymentTransactionID,
                    TransactionRefNo = pt.TransactionRefNo,
                    TransactionDate = pt.TransactionDate,
                    PaymentMethodName = pt.PaymentMethod?.MethodName ?? "Unknown",
                    Amount = pt.Amount,
                    Status = pt.Status
                }).ToList() ?? new List<PaymentHistoryViewModel>(),

                CreatedByName = invoice?.CreatedByNavigation != null ? $"{invoice.CreatedByNavigation.FirstName} {invoice.CreatedByNavigation.LastName}" : "Unknown",

                UpdatedByName = invoice?.UpdatedByNavigation != null ? $"{invoice.UpdatedByNavigation.FirstName} {invoice.UpdatedByNavigation.LastName}" : "Unknown",

               
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
                UpdatedAt = vm.UpdatedAt,
                SalesOrderId = vm.SelectedSalesOrderId
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
        public async Task<IActionResult> UpdateStatus(int id, int status, BaseViewModel baseView)
        {
            try
            {
                var invoice = await _invoiceRepository.All().Include(e => e.InvoiceItems).FirstOrDefaultAsync(e => e.InvoiceID == id);
                if (invoice == null)
                {
                    return Json(new { success = false, message = "Invoice not found" });
                }

                invoice.IsFinal = true;
                invoice.IsDraft = false;

                await _invoiceRepository.UpdateAsync(invoice);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult MarkAsPaid(int id)
        {
            try
            {
                var invoice = _invoiceRepository.All().FirstOrDefault(e => e.InvoiceID == id);
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
            return GetCurrentEmployeeIdAsync().Result ?? 1;
        }
    }
}
