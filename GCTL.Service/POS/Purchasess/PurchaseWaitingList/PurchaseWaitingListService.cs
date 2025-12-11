using GCTL.Core.Repository;
using GCTL.Core.ViewModels.POS.Purchasess.Purchase.PurchaseWaitionList;
using GCTL.Core.ViewModels.Services;
using GCTL.Data.Models;
using GCTL.Service.FileHandler;
using GCTL.Service.ProductService;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;

namespace GCTL.Service.POS.Purchasess.PurchaseWaitingList
{
    public class PurchaseWaitingListService : IPurchaseWaitingList
    {
        private readonly IGenericRepository<PurchasOrders> _purchaseOrderService;
        private readonly IGenericRepository<PurchasOrderItems> _purchaseOrderItemService;
        private readonly IPdfFileHandler _pdfFileHandlerService;

        private static List<PurchaseRequestItem> _purchaseRequests = new List<PurchaseRequestItem>
        {
            new PurchaseRequestItem
            {
                ReqId = "#req9849gg",
                ProjectName = "Highway Construction Phase 1",
                ProjectId = 1,
                RequestedBy = "John Smith",
                RequestDate = DateTime.Now.AddDays(-5),
                ProductType = "Scaffolding Materials",
                ProductTypeId = 1,
                ProductName = "1700 Scaffolding",
                ProductId = 1,
                Quantity = 500,
                EstimatedCost = 25000.00m,
                Status = "Pending",
                Priority = "High"
            },
            new PurchaseRequestItem
            {
                ReqId = "#req9849",
                ProjectName = "Bridge Renovation",
                ProjectId = 2,
                RequestedBy = "Jane Doe",
                RequestDate = DateTime.Now.AddDays(-3),
                ProductType = "Scaffolding Materials",
                ProductTypeId = 1,
                ProductName = "1100 Scaffolding",
                ProductId = 2,
                Quantity = 300,
                EstimatedCost = 18000.00m,
                Status = "Pending",
                Priority = "Medium"
            },
            new PurchaseRequestItem
            {
                ReqId = "#req9850",
                ProjectName = "Office Building Construction",
                ProjectId = 3,
                RequestedBy = "Mike Johnson",
                RequestDate = DateTime.Now.AddDays(-7),
                ProductType = "Electrical Equipment",
                ProductTypeId = 2,
                ProductName = "Industrial Cables",
                ProductId = 3,
                Quantity = 1000,
                EstimatedCost = 12000.00m,
                Status = "Pending",
                Priority = "Low"
            },
            new PurchaseRequestItem
            {
                ReqId = "#req9851",
                ProjectName = "Shopping Mall Construction",
                ProjectId = 4,
                RequestedBy = "Sarah Wilson",
                RequestDate = DateTime.Now.AddDays(-2),
                ProductType = "Construction Materials",
                ProductTypeId = 3,
                ProductName = "Steel Beams",
                ProductId = 4,
                Quantity = 200,
                EstimatedCost = 50000.00m,
                Status = "Pending",
                Priority = "High"
            },
            new PurchaseRequestItem
            {
                ReqId = "#req9852",
                ProjectName = "Residential Complex",
                ProjectId = 5,
                RequestedBy = "David Brown",
                RequestDate = DateTime.Now.AddDays(-1),
                ProductType = "Plumbing Materials",
                ProductTypeId = 4,
                ProductName = "Copper Pipes",
                ProductId = 5,
                Quantity = 800,
                EstimatedCost = 15000.00m,
                Status = "Pending",
                Priority = "Medium"
            },
            new PurchaseRequestItem
            {
                ReqId = "#req9853",
                ProjectName = "School Building",
                ProjectId = 6,
                RequestedBy = "Lisa Garcia",
                RequestDate = DateTime.Now.AddDays(-4),
                ProductType = "Safety Equipment",
                ProductTypeId = 5,
                ProductName = "Safety Helmets",
                ProductId = 6,
                Quantity = 150,
                EstimatedCost = 3000.00m,
                Status = "Pending",
                Priority = "High"
            },
            new PurchaseRequestItem
            {
                ReqId = "#req9854",
                ProjectName = "Hospital Construction",
                ProjectId = 7,
                RequestedBy = "Robert Taylor",
                RequestDate = DateTime.Now.AddDays(-6),
                ProductType = "HVAC Equipment",
                ProductTypeId = 6,
                ProductName = "Air Conditioning Units",
                ProductId = 7,
                Quantity = 25,
                EstimatedCost = 75000.00m,
                Status = "Pending",
                Priority = "High"
            },
            new PurchaseRequestItem
            {
                ReqId = "#req9855",
                ProjectName = "Parking Garage",
                ProjectId = 8,
                RequestedBy = "Emily Davis",
                RequestDate = DateTime.Now.AddDays(-8),
                ProductType = "Concrete Materials",
                ProductTypeId = 7,
                ProductName = "Ready Mix Concrete",
                ProductId = 8,
                Quantity = 500,
                EstimatedCost = 30000.00m,
                Status = "Pending",
                Priority = "Medium"
            },
            new PurchaseRequestItem
            {
                ReqId = "#req9856",
                ProjectName = "Sports Complex",
                ProjectId = 9,
                RequestedBy = "James Miller",
                RequestDate = DateTime.Now.AddDays(-9),
                ProductType = "Flooring Materials",
                ProductTypeId = 8,
                ProductName = "Sports Flooring",
                ProductId = 9,
                Quantity = 1000,
                EstimatedCost = 40000.00m,
                Status = "Pending",
                Priority = "Low"
            },
            new PurchaseRequestItem
            {
                ReqId = "#req9857",
                ProjectName = "Warehouse Construction",
                ProjectId = 10,
                RequestedBy = "Amanda Wilson",
                RequestDate = DateTime.Now.AddDays(-10),
                ProductType = "Roofing Materials",
                ProductTypeId = 9,
                ProductName = "Metal Roofing Sheets",
                ProductId = 10,
                Quantity = 300,
                EstimatedCost = 20000.00m,
                Status = "Pending",
                Priority = "Medium"
            },

            new PurchaseRequestItem
            {
                ReqId = "#req9849gg",
                ProjectName = "Highway Construction Phase 1",
                ProjectId = 1,
                RequestedBy = "John Smith",
                RequestDate = DateTime.Now.AddDays(-5),
                ProductType = "Scaffolding Materials",
                ProductTypeId = 1,
                ProductName = "1700 Scaffolding",
                ProductId = 1,
                Quantity = 500,
                EstimatedCost = 25000.00m,
                Status = "Pending",
                Priority = "High"
            },
            new PurchaseRequestItem
            {
                ReqId = "#req9849",
                ProjectName = "Bridge Renovation",
                ProjectId = 2,
                RequestedBy = "Jane Doe",
                RequestDate = DateTime.Now.AddDays(-3),
                ProductType = "Scaffolding Materials",
                ProductTypeId = 1,
                ProductName = "1100 Scaffolding",
                ProductId = 2,
                Quantity = 300,
                EstimatedCost = 18000.00m,
                Status = "Pending",
                Priority = "Medium"
            },
            new PurchaseRequestItem
            {
                ReqId = "#req9850",
                ProjectName = "Office Building Construction",
                ProjectId = 3,
                RequestedBy = "Mike Johnson",
                RequestDate = DateTime.Now.AddDays(-7),
                ProductType = "Electrical Equipment",
                ProductTypeId = 2,
                ProductName = "Industrial Cables",
                ProductId = 3,
                Quantity = 1000,
                EstimatedCost = 12000.00m,
                Status = "Pending",
                Priority = "Low"
            },
            new PurchaseRequestItem
            {
                ReqId = "#req9851",
                ProjectName = "Shopping Mall Construction",
                ProjectId = 4,
                RequestedBy = "Sarah Wilson",
                RequestDate = DateTime.Now.AddDays(-2),
                ProductType = "Construction Materials",
                ProductTypeId = 3,
                ProductName = "Steel Beams",
                ProductId = 4,
                Quantity = 200,
                EstimatedCost = 50000.00m,
                Status = "Pending",
                Priority = "High"
            },
            new PurchaseRequestItem
            {
                ReqId = "#req9852",
                ProjectName = "Residential Complex",
                ProjectId = 5,
                RequestedBy = "David Brown",
                RequestDate = DateTime.Now.AddDays(-1),
                ProductType = "Plumbing Materials",
                ProductTypeId = 4,
                ProductName = "Copper Pipes",
                ProductId = 5,
                Quantity = 800,
                EstimatedCost = 15000.00m,
                Status = "Pending",
                Priority = "Medium"
            },
            new PurchaseRequestItem
            {
                ReqId = "#req9853",
                ProjectName = "School Building",
                ProjectId = 6,
                RequestedBy = "Lisa Garcia",
                RequestDate = DateTime.Now.AddDays(-4),
                ProductType = "Safety Equipment",
                ProductTypeId = 5,
                ProductName = "Safety Helmets",
                ProductId = 6,
                Quantity = 150,
                EstimatedCost = 3000.00m,
                Status = "Pending",
                Priority = "High"
            },
            new PurchaseRequestItem
            {
                ReqId = "#req9854",
                ProjectName = "Hospital Construction",
                ProjectId = 7,
                RequestedBy = "Robert Taylor",
                RequestDate = DateTime.Now.AddDays(-6),
                ProductType = "HVAC Equipment",
                ProductTypeId = 6,
                ProductName = "Air Conditioning Units",
                ProductId = 7,
                Quantity = 25,
                EstimatedCost = 75000.00m,
                Status = "Pending",
                Priority = "High"
            },
            new PurchaseRequestItem
            {
                ReqId = "#req9855",
                ProjectName = "Parking Garage",
                ProjectId = 8,
                RequestedBy = "Emily Davis",
                RequestDate = DateTime.Now.AddDays(-8),
                ProductType = "Concrete Materials",
                ProductTypeId = 7,
                ProductName = "Ready Mix Concrete",
                ProductId = 8,
                Quantity = 500,
                EstimatedCost = 30000.00m,
                Status = "Pending",
                Priority = "Medium"
            },
            new PurchaseRequestItem
            {
                ReqId = "#req9856",
                ProjectName = "Sports Complex",
                ProjectId = 9,
                RequestedBy = "James Miller",
                RequestDate = DateTime.Now.AddDays(-9),
                ProductType = "Flooring Materials",
                ProductTypeId = 8,
                ProductName = "Sports Flooring",
                ProductId = 9,
                Quantity = 1000,
                EstimatedCost = 40000.00m,
                Status = "Pending",
                Priority = "Low"
            },
            new PurchaseRequestItem
            {
                ReqId = "#req9857",
                ProjectName = "Warehouse Construction",
                ProjectId = 10,
                RequestedBy = "Amanda Wilson",
                RequestDate = DateTime.Now.AddDays(-10),
                ProductType = "Roofing Materials",
                ProductTypeId = 9,
                ProductName = "Metal Roofing Sheets",
                ProductId = 10,
                Quantity = 300,
                EstimatedCost = 20000.00m,
                Status = "Pending",
                Priority = "Medium"
            },

        };

        public PurchaseWaitingListService(IGenericRepository<PurchasOrders> purchaseOrderService, IGenericRepository<PurchasOrderItems> purchaseOrderItemService, IPdfFileHandler pdfFileHandlerService)
        {
            _purchaseOrderService = purchaseOrderService;
            _purchaseOrderItemService = purchaseOrderItemService;
            _pdfFileHandlerService = pdfFileHandlerService;
        }

        public PurchaseWaitingListViewModel GetPurchaseWaitingList(FilterOptions filters, PaginationInfo pagination)
        {
            try
            {
                var querys = _purchaseRequests.AsQueryable();

                var query1 = _purchaseOrderService.AllActive().ToList();

                var orders = _purchaseOrderService.AllActive()
                    .Include(e => e.PurchasOrderItems).ThenInclude(e => e.Product).ThenInclude(e => e.ProductType)
                    //.Include(e => e.ProductMovements).ThenInclude(e => e.ToLocation).ThenInclude(e => e.Projects).ThenInclude(e => e.ProjectManager)
                   // .Include(e => e.ProductMovements).ThenInclude(e => e.RequisitionItem).ThenInclude(e => e.Requisition)
                    .Include(e => e.Status).OrderByDescending(e => e.PurchasOrderID)
                    .ToList(); // fetch all into memory

                var query = orders.Select(e =>
                {
                   // var firstMovement = e.ProductMovements.FirstOrDefault();
                    var firstOrderItem = e.PurchasOrderItems.FirstOrDefault();

                   // var project = firstMovement?.ToLocation?.Projects?.FirstOrDefault();
                   // var manager = project?.ProjectManager;

                    return new PurchaseRequestItem
                    {
                        PurchaseId = e.PurchasOrderID,
                        PoId = e.POID,
                    //    ReqId = firstMovement?.RequisitionItem?.RequisitionID?.ToString() ?? "Manual",
                     //   ProjectName = project?.ProjectName ?? "Manual",
                       
                     //   ProjectId = project?.ProjectID ?? 0,
                      //  RequestedBy = ((manager?.FirstName ?? "") + " " + (manager?.LastName ?? "")).Trim(),
                       // RequestDate = firstMovement?.RequisitionItem?.Requisition?.RequisitionDate ?? DateTime.MinValue,
                        RequestDate = e.PurchaseDate ?? DateTime.MinValue,
                        ProductType = firstOrderItem?.Product?.ProductType?.ProductTypeName ?? "",
                        ProductTypeId = firstOrderItem?.Product?.ProductTypeID ?? 0,
                        ProductName = firstOrderItem?.Product?.ProductName ?? "",
                        ProductId = firstOrderItem?.ProductID ?? 0,
                        Quantity = e.PurchasOrderItems.Sum(i => i.Quantity ?? 0),
                        EstimatedCost = 20000.00m,
                        Status = e.Status?.StatusName ?? "",
                        Priority = e.Status?.StatusName?.ToLower() == "pending" ? "0" : "1",
                    };
                }).AsQueryable();




                // Apply search filter
                if (!string.IsNullOrEmpty(filters.SearchTerm))
                {
                    var searchTerm = filters.SearchTerm.ToLower();
                    query = query.Where(x =>
                        x.ReqId.ToLower().Contains(searchTerm) ||
                        x.ProjectName.ToLower().Contains(searchTerm) ||
                        x.RequestedBy.ToLower().Contains(searchTerm) ||
                        x.ProductName.ToLower().Contains(searchTerm) ||
                        x.ProductType.ToLower().Contains(searchTerm));
                }

                // Apply filters
                if (filters.ProductTypeId.HasValue)
                {
                    query = query.Where(x => x.ProductTypeId == filters.ProductTypeId.Value);
                }

                if (filters.ProjectId.HasValue)
                {
                    query = query.Where(x => x.ProjectId == filters.ProjectId.Value);
                }

                if (filters.ProductId.HasValue)
                {
                    query = query.Where(x => x.ProductId == filters.ProductId.Value);
                }

                // Apply sorting
                switch (filters.SortBy?.ToLower())
                {
                    case "reqid":
                        query = filters.SortDirection == "desc" ?
                            query.OrderByDescending(x => x.ReqId) :
                            query.OrderBy(x => x.ReqId);
                        break;
                    case "projectname":
                        query = filters.SortDirection == "desc" ?
                            query.OrderByDescending(x => x.ProjectName) :
                            query.OrderBy(x => x.ProjectName);
                        break;
                    case "requestedby":
                        query = filters.SortDirection == "desc" ?
                            query.OrderByDescending(x => x.RequestedBy) :
                            query.OrderBy(x => x.RequestedBy);
                        break;
                    case "requestdate":
                        query = filters.SortDirection == "desc" ?
                            query.OrderByDescending(x => x.RequestDate) :
                            query.OrderBy(x => x.RequestDate);
                        break;
                    case "producttype":
                        query = filters.SortDirection == "desc" ?
                            query.OrderByDescending(x => x.ProductType) :
                            query.OrderBy(x => x.ProductType);
                        break;
                    case "productname":
                        query = filters.SortDirection == "desc" ?
                            query.OrderByDescending(x => x.ProductName) :
                            query.OrderBy(x => x.ProductName);
                        break;
                    case "quantity":
                        query = filters.SortDirection == "desc" ?
                            query.OrderByDescending(x => x.Quantity) :
                            query.OrderBy(x => x.Quantity);
                        break;
                    default:
                        query = query.OrderBy(x => x.ReqId);
                        break;
                }

                var totalItems = query.Count();
                pagination.TotalItems = totalItems;

                // Apply pagination
                var items = query
                    .Skip((pagination.CurrentPage - 1) * pagination.PageSize)
                    .Take(pagination.PageSize)
                    .ToList();

                return new PurchaseWaitingListViewModel
                {
                    PurchaseRequests = items,
                    Pagination = pagination,
                    Filters = filters
                };
            }
            catch (Exception)
            {

                throw;
            }
            
        }

        public List<PurchaseRequestItem> GetAllPurchaseRequests()
        {
            return _purchaseRequests;
        }

        //public PurchaseRequestItem GetPurchaseRequestById(string purchaseOrderId)
        //{
        //    return _purchaseRequests.FirstOrDefault(x => x.ReqId == purchaseOrderId);
        //}

        public PurchaseRequestViewModel GetPurchaseRequestById(string reqId)
        {
            var request = _purchaseOrderService.AllActive()
                .Include(r => r.PurchasOrderItems)
                .Include(r => r.PurchasOrderItems).ThenInclude(i => i.Product).ThenInclude(i => i.ProductType)
                
                .FirstOrDefault(r => r.PurchasOrderID.ToString() == reqId);

            if (request == null)
                return null;

            return new PurchaseRequestViewModel
            {
                PurchaseId = request.PurchasOrderID,
                PoId = request.POID,
               // ReqId = request.ReqId,
                ProjectId = request.PurchasOrderID.ToString(),
                //ProjectName = request.Project?.ProjectName,
                //RequestedBy = request.RequestedBy,
                //RequestDate = request.RequestDate,
                Status = request.Status.StatusName,
               // Priority = request.Priority,
                Items = request.PurchasOrderItems.Select(i => new PurchaseRequestItemViewModel
                {
                    ProductId = i.ProductID,
                    ProductName = i.Product.ProductName,
                    ProductTypeId = 0,
                    ProductType = "ddd",
                    Quantity = 0,
                    EstimatedCost = 0
                }).ToList()
            };
        }

        #region GetWaitingItemData
        public async Task<WaittingItemVM> GetItemData(int id)
        {
            //var test = await _purchaseOrderService.AllActive()
            //           .Include(e => e.ToLocationNavigation).ThenInclude(e => e.Projects).ThenInclude(e => e.Address)
            //           .Include(e => e.ToLocationNavigation).ThenInclude(e => e.Projects).ThenInclude(e => e.ProjectManager)
            //           .Select(e => new
            //           {
            //               addreess = e.ToLocationNavigation.Projects.FirstOrDefault().Address.FullAddress,
            //               name = e.ToLocationNavigation.Projects.FirstOrDefault().ProjectManager.FirstName + " " + e.ToLocationNavigation.Projects.FirstOrDefault().ProjectManager.FirstName,
            //               phone = e.ToLocationNavigation.Projects.FirstOrDefault().ProjectManager.MobileNumber ,
            //               projectname = e.ToLocationNavigation.LocationName,


            //           }).FirstOrDefaultAsync();



            //var result = await _purchaseOrderService.AllActive()
            //    .Include(t => t.PurchasOrderItems).ThenInclude(t => t.Product)
            //    .Include(t => t.OBBillingAddress)
            //    .Include(t => t.OBShipingAddress)
            //      .Include(e => e.ToLocationNavigation).ThenInclude(e => e.Projects).ThenInclude(e => e.Address)
            //           .Include(e => e.ToLocationNavigation).ThenInclude(e => e.Projects).ThenInclude(e => e.ProjectManager)
            //    .Where(t => t.PurchasOrderID == id)
            //    .Select(t => new WaittingItemVM
            //    {
            //        POID = t.POID,
            //        PODate = t.PurchaseDate.ToString() ?? "",
            //        OBBillingName = $"{t.OBBillingAddress.FirstName ?? ""} {t.OBBillingAddress.LastName ?? ""}",
            //        OBBillingFullAddress = t.OBBillingAddress.FullAddress ?? "",
            //        OBBillingPhone = t.OBBillingAddress.Phone ?? "",
            //       // OBShippingName = $"{t.OBShipingAddress.FirstName ?? ""} {t.OBShipingAddress.LastName ?? ""}",
            //        OBShippingName = test.name,
            //        //OBShippingFullAddress = t.OBShipingAddress.FullAddress ?? "",
            //        OBShippingFullAddress = test.addreess,
            //        //OBShippingPhone = t.OBShipingAddress.Phone ?? "",
            //        OBShippingPhone = test.phone,
            //        PurchasOrderItemList = t.PurchasOrderItems.Select(i => new PurchasOrderItem
            //        {
            //            ProductName = i.Product.ProductName,
            //            ProductQuentity = i.Quantity,
            //            ProductUnitPrice = i.UnitPrice,
            //        }).ToList()
            //    })
            //    .FirstOrDefaultAsync();

            var result = await _purchaseOrderService.AllActive()
    .Include(t => t.PurchasOrderItems).ThenInclude(t => t.Product)
    .Include(t => t.OBBillingAddress)
    .Include(t => t.OBShipingAddress)
   // .Include(e => e.ToLocationNavigation)
     //   .ThenInclude(e => e.Projects)
          //  .ThenInclude(e => e.Address)
   // .Include(e => e.ToLocationNavigation)
       // .ThenInclude(e => e.Projects)
          //  .ThenInclude(e => e.ProjectManager)
    .Where(t => t.PurchasOrderID == id)
    .Select(t => new WaittingItemVM
    {
        POID = t.POID,
        PODate = t.PurchaseDate.ToString() ?? "",

        // Billing Info
        OBBillingName = $"{t.OBBillingAddress.FirstName ?? ""} {t.OBBillingAddress.LastName ?? ""}",
        OBBillingFullAddress = t.OBBillingAddress.FullAddress ?? "",
        OBBillingPhone = t.OBBillingAddress.Phone ?? "",

        // Shipping Info — with full null safety
        //OBShippingName = t.ToLocationNavigation.Projects
        //                    .Select(p =>
        //                        ((p.ProjectManager != null
        //                            ? ((p.ProjectManager.FirstName ?? "") + " " + (p.ProjectManager.LastName ?? ""))
        //                            : "")
        //                        ))
        //                    .FirstOrDefault() ?? "",

        //OBShippingFullAddress = t.ToLocationNavigation.Projects
        //                    .Select(p => p.Address != null
        //                        ? (p.Address.FullAddress ?? "")
        //                        : "")
        //                    .FirstOrDefault() ?? "",

        //OBShippingPhone = t.ToLocationNavigation.Projects
        //                    .Select(p => p.ProjectManager != null
        //                        ? (p.ProjectManager.MobileNumber ?? "")
        //                        : "")
        //                    .FirstOrDefault() ?? "",

        //ProjectName = t.ToLocationNavigation.LocationName ?? "",

        // Purchase order items
        PurchasOrderItemList = t.PurchasOrderItems
            .Select(i => new PurchasOrderItem
            {
                ProductName = i.Product.ProductName ?? "",
                ProductQuentity = i.Quantity,
                ProductUnitPrice = i.UnitPrice
            })
            .ToList()
    })
    .FirstOrDefaultAsync();


            

            return result;
        }
        #endregion


        public async Task<byte[]> GeneratePDF(int orgid, int id)
        {
            try
            {
                var activityDataSource = await GetItemData(id);
                if (activityDataSource == null)
                    return Array.Empty<byte>();

                var grandTotal = activityDataSource.PurchasOrderItemList
                    .Sum(x => (x.ProductQuentity ?? 0) * (x.ProductUnitPrice ?? 0));

                var model = new PDFServiceModel
                {
                    Title = ["title: Purchase Order"],
                    Headers = new List<HeaderInfo>
            {
                new HeaderInfo{Text = "SL", Width = 35 },
                new HeaderInfo{Text = "Product Name", Width = 250},
                new HeaderInfo{Text = "Quantity"},
                new HeaderInfo{Text = "Unit Price"},
                new HeaderInfo{Text = "Total Price"},
            },
                    Rows = activityDataSource.PurchasOrderItemList.Select((x, index) => new List<CellInfo>
            {
                new CellInfo{Text=(index + 1).ToString(), Align="Center"},
                new CellInfo{Text=x.ProductName ?? ""},
                new CellInfo{Text=x.ProductQuentity?.ToString() ?? ""},
                new CellInfo{Text=x.ProductUnitPrice?.ToString() ?? ""},
                new CellInfo{Text=(x.ProductQuentity * x.ProductUnitPrice)?.ToString() ?? ""}
            }).ToList(),
                    TopBox = new TopBox
                    {
                        KeyAndValues = new List<KeyAndValue>
                        {
                            new KeyAndValue { Key = "POID", Value = activityDataSource.POID ?? "", Show = "both" },
                            new KeyAndValue { Key = "PODate", Value = activityDataSource.PODate ?? "" , Show = "both"},
                        }
                    },
                    LeftBox = new LeftBox
                    {
                        Title = "Billing Address", 
                        KeyAndValues = new List<KeyAndValue>
                        {
                            new KeyAndValue { Key = "Name", Value = activityDataSource.OBBillingName ?? "" },
                            new KeyAndValue { Key = "Full Address", Value = activityDataSource.OBBillingFullAddress ?? "" },
                            new KeyAndValue { Key = "Phone", Value = activityDataSource.OBBillingPhone ?? "" }
                        }
                    },
                    RightBox = new RightBox
                    {
                        Title = "Shipping Address",
                        KeyAndValues = new List<KeyAndValue>
                        {
                            new KeyAndValue { Key = "Name", Value = activityDataSource.OBShippingName ?? "" },
                            new KeyAndValue { Key = "Full Address", Value = activityDataSource.OBShippingFullAddress ?? "" },
                            new KeyAndValue { Key = "Phone", Value = activityDataSource.OBShippingPhone ?? "" }
                        }
                    },
                    FooterBox = new FooterBox
                    {
                        KeyAndValues = new List<KeyAndValue>
                {
                    new KeyAndValue { Key = "Grand Total", Value = grandTotal.ToString("N2") }
                }
                    }
                };

                var document = new PDFService(model, _pdfFileHandlerService, orgid);
                var resut = document.GeneratePdf();
                return resut;
            }
            catch
            {
                return Array.Empty<byte>();
            }
        }


        public class WaittingItemVM
        {
            public string? POID { get; set; }
            public string? PODate { get; set; }
            public string? OBBillingName { get; set; }
            public string? OBBillingFullAddress { get; set; }
            public string? OBBillingPhone { get; set; }
            public string? OBShippingName { get; set; }
            public string? OBShippingFullAddress { get; set; }
            public string? OBShippingPhone { get; set; }
            public List<PurchasOrderItem> PurchasOrderItemList { get; set; } = new();
        }

        public class PurchasOrderItem
        {
            public string? ProductName { get; set; }
            public decimal? ProductQuentity { get; set; }
            public decimal? ProductUnitPrice { get; set; }
            public decimal? TotalPrice => (ProductQuentity ?? 0) * (ProductUnitPrice ?? 0);
        }
    }
}
