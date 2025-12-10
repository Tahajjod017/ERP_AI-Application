using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.RequisitionDistribution.Purchase.CreatePO;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.Finance.AddSubAccount;
using GCTL.Service.Finance.TransactionAccount;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks;
using static Dapper.SqlMapper;

namespace GCTL.Service.RequisitionDistribution.CreatePO
{
    public class CreatePurchaseOrderService : ICreatePurchaseOrder
    {
        #region Repositories and Services
        private readonly IGenericRepository<Suppliers> _supperRepository;
        private readonly IGenericRepository<SupplierAddresses> _supperAddressRepository;
        private readonly IGenericRepository<SAddresses> _supperAddressDetailsRepository;
        private readonly IGenericRepository<Locations> _locationRepository;
        private readonly IGenericRepository<PurOrderBaseSAddresses> _purOrderBaseSAddressesRepository;
        private readonly IGenericRepository<PurchasOrderItems> _purchasOrderItemsRepository;
        private readonly IGenericRepository<PurchasOrders> _purchasOrdersRepository;
        private readonly IGenericRepository<Statuses> _statusRepository;
        private readonly IUserInfoService _userInfoService;
       // private readonly IRequisitionApprovalService _requisitionApproveService;

        

        #region Data
        // Static vendor data
        private readonly List<VendorViewModel> vendors = new List<VendorViewModel>
        {
            new VendorViewModel
            {
                Id = 1,
                Name = "Jone",
                Address = "235 Mirpur, Road # 205, Dhaka 1216, Bangladesh",
                TaxNumber = "10236254",
                Phone = "(+880) 01737172631",
                Email = "mailfd@gmail.com"
            },
            new VendorViewModel
            {
                Id = 2,
                Name = "Riya",
                Address = "123 Gulshan, Dhaka, Bangladesh",
                TaxNumber = "98765432",
                Phone = "(+880) 01712345678",
                Email = "riya@example.com"
            }
        };

        // Static shipping address data
        private readonly List<ShippingAddressViewModel> shippingAddresses = new List<ShippingAddressViewModel>
        {
            new ShippingAddressViewModel
            {
                Id = 1,
                Address = "Mirpur-10, Dhaka, Bangladesh",
                Contact = "Robiul Islam",
                Phone = "(+880) 01737172631",
                Email = "mailfd@gmail.com"
            },
            new ShippingAddressViewModel
            {
                Id = 2,
                Address = "Mirpur-6, Dhaka, Bangladesh",
                Contact = "Aminul Islam",
                Phone = "(+880) 01712345678",
                Email = "aminul@example.com"
            }
        };

        #endregion
        public CreatePurchaseOrderService(IGenericRepository<Suppliers> supperRepository, IGenericRepository<SupplierAddresses> supperAddressRepository, IGenericRepository<SAddresses> supperAddressDetailsRepository, IGenericRepository<Locations> locationRepository, IGenericRepository<PurOrderBaseSAddresses> purOrderBaseSAddressesRepository, IGenericRepository<PurchasOrderItems> purchasOrderItemsRepository, IGenericRepository<PurchasOrders> purchasOrdersRepository, IGenericRepository<Statuses> statusRepository, IUserInfoService userInfoService)
        {
            _supperRepository = supperRepository;
            _supperAddressRepository = supperAddressRepository;
            _supperAddressDetailsRepository = supperAddressDetailsRepository;
            _locationRepository = locationRepository;
            _purOrderBaseSAddressesRepository = purOrderBaseSAddressesRepository;
            _purchasOrderItemsRepository = purchasOrderItemsRepository;
            _purchasOrdersRepository = purchasOrdersRepository;
            _statusRepository = statusRepository;
            _userInfoService = userInfoService;
        }

        #endregion


        #region GetVendorsAsync
        public async Task<List<VendorViewModel>> GetVendorsAsync()
        {
            try
            {
                var data = await _supperAddressRepository.AllActive()
               .Include(e => e.AddressType)  .Where(e => e.AddressType.AddressTypeName == "Billing Address")
               .Include(e => e.Supplier)
               .Include(e => e.SAddress)
               .Select(e => new VendorViewModel
               {
                   Id = e.SupplierID ?? 0,
                   Name = e.Supplier.FullName ?? "",
                   Address = e.SAddress.FullAddress ?? "",
                   TaxNumber = "",
                   Phone = e.SAddress.Phone,
                   Email = e.SAddress.Email
               }).ToListAsync();

                return data;
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion


        #region GetShippingAddressesAsync
        public async Task<List<ShippingAddressViewModel>> GetShippingAddressesAsync()
        {
            //var wirehouseData = await _locationRepository.AllActive()
            //    .Include(e => e.Wirehouses).ThenInclude(e => e.Manage)
            //    .SelectMany(e => e.Wirehouses.Select(w => new ShippingAddressViewModel
            //    {
            //        Id = e.LocationID,
            //        Name = w.WirehouseName,
            //        Address = w.FullAddress,
            //        Contact = w.Manage.FirstName + " " + w.Manage.LastName,
            //        Phone = w.Phone,
            //        Email = w.Email
            //    }))
            //    .ToListAsync();

            //var projectData = await _locationRepository.AllActive()
            //    .Include(e => e.Projects).ThenInclude(p => p.ProjectManager)
            //    .Include(e => e.Projects).ThenInclude(p => p.Address)
            //    .SelectMany(e => e.Projects.Select(p => new ShippingAddressViewModel
            //    {
            //        Id = e.LocationID,
            //        Name = p.ProjectName,
            //        Address = p.Address.FullAddress,
            //        Contact = p.ProjectManager.FirstName + " " + p.ProjectManager.LastName,
            //        Phone = p.Address.Phone,
            //        Email = p.Address.Email
            //    }))
            //    .ToListAsync();

            

            var combined = new List<ShippingAddressViewModel>()
            {
                new ShippingAddressViewModel
                {
                     Id = 1,
                     Name = "ProjectName",
                     Address = "p.Address.FullAddress",
                     Contact = "LastName",
                     Phone = "p.Address.Phone",
                     Email = "p.Address.Email"
                }
            };
            return combined;
        }
        #endregion


        #region AddVendorAsync
        public async Task<bool> AddVendorAsync(VendorViewModel vendor)
        {
            await _supperAddressDetailsRepository.BeginTransactionAsync();

            try
            {
                

                var sup = new Suppliers()
                {
                    FullName = vendor.CompanyName,
                    IsPerson = false,
                   
                };
                await _supperRepository.AddAsync(sup, vendor);
                await _userInfoService.ActionLogAsync("Suppliers", ActionName.DataAdd, null, sup, sup.SupplierID, vendor);


                var address = new SAddresses()
                {
                    Phone = vendor.Phone
                };

                await _supperAddressDetailsRepository.AddAsync(address, vendor);
                await _userInfoService.ActionLogAsync("SAddresses", ActionName.DataAdd, null, address, address.SAddressID, vendor);


                var supAddress = new SupplierAddresses()
                {
                    SupplierID = sup.SupplierID,
                    AddressTypeID = 1,
                    SAddressID = address.SAddressID
                };
                await _supperAddressRepository.AddAsync(supAddress, vendor);
                await _userInfoService.ActionLogAsync("SupplierAddresses", ActionName.DataAdd, null, supAddress, supAddress.SupplierID, vendor);

                await _supperAddressDetailsRepository.CommitTransactionAsync();
                return true;
            }
            catch (Exception)
            {
                await _supperAddressDetailsRepository.RollbackTransactionAsync();
                return false;
            }

            
        }
        #endregion


        #region AddShippingAddressAsync
        public Task<bool> AddShippingAddressAsync(ShippingAddressViewModel address)
        {
            address.Id = shippingAddresses.Count + 1;
            shippingAddresses.Add(address);
            return Task.FromResult(true);
        }
        #endregion


        #region SavePurchaseOrderAsync
        public Task<(bool Success, string Message, int PurchaseId)> SavePurchaseOrderAsync(PurchaseOrderViewModel model)
        {
            // Simulate saving to a database
            return Task.FromResult((true, "Purchase order saved successfully", 123));
        }
        #endregion


        #region SavePurchaseOrderAsync
        public async Task<(bool Success, string Message, int PurchaseId)> SavePurchaseOrderAsync( PurchaseOrderSubmissionViewModel model)
        {

            await _purchasOrdersRepository.BeginTransactionAsync();

            try
            {
                
                if (model.BillingInfo == null || string.IsNullOrEmpty(model.BillingInfo.Name))
                {
                    await _purchasOrdersRepository.RollbackTransactionAsync();
                    return (false, "Billing information is required", 0);
                }

                if (model.Items == null || model.Items.Count == 0)
                {
                    await _purchasOrdersRepository.RollbackTransactionAsync();
                    return (false, "At least one item is required", 0);
                }
                    

                if (string.IsNullOrEmpty(model.PoNumber))
                {
                    await _purchasOrdersRepository.RollbackTransactionAsync();
                    return (false, "PO Number is required", 0);
                }


                var status = await _statusRepository.AllActive()
                   .FirstOrDefaultAsync(s => s.StatusName.ToLower() == "onprocess");

                if (status == null)
                {
                    status = new Statuses
                    {
                        StatusName = "OnProcess",
                        CreatedAt = DateTime.Now,
                        CreatedBy = model.CreatedBy

                    };
                    await _statusRepository.AddAsync(status);
                    await _userInfoService.ActionLogAsync("Statuses", ActionName.DataAdd, null, status, status.StatusID, model);
                }

                var purchaseOrder = await _purchasOrdersRepository.AllActive()
                 .FirstOrDefaultAsync(e => e.PurchasOrderID == model.PurchaseOderId);


                if (purchaseOrder != null && purchaseOrder.StatusID == status.StatusID)
                {
                    await _purchasOrdersRepository.RollbackTransactionAsync();
                    return (false, "PO is On Process", 0);
                }

                    decimal calculatedTotal = 0;
                foreach (var item in model.Items)
                {
                    if (item.Quantity <= 0 || item.UnitPrice <= 0)
                    {
                        await _purchasOrdersRepository.RollbackTransactionAsync();
                        return (false, $"Invalid quantity or price for item: {item.ItemName}", 0);
                    }

                    calculatedTotal += item.Quantity * item.UnitPrice;
                }

                decimal taxAmount = calculatedTotal * (model.TaxRate / 100);
                decimal grandTotal = calculatedTotal + taxAmount;

               var supper = await _supperAddressDetailsRepository.AllActive()
                    .Include(e => e.SupplierAddresses).ThenInclude(e => e.Supplier)
                    .Include(e => e.SupplierAddresses).ThenInclude(e => e.AddressType)
                    .Where(e => e.SupplierAddresses.Any(sa => sa.SupplierID == model.SupperId && sa.AddressType.AddressTypeName == "Billing Address"))
                    .FirstOrDefaultAsync();

                var billingAddress = new PurOrderBaseSAddresses
                {
                    FirstName = supper?.FirstName ?? "",
                    LastName = supper?.LastName ?? "",
                    FullAddress = supper?.FullAddress ?? "",
                    Phone = supper?.Phone ?? model.BillingInfo.Phone,
                    Email = supper?.Email ?? model.BillingInfo.Email,
                    CreatedAt = DateTime.Now,
                    State = supper?.State ?? "",
                    City = supper?.City ?? "",
                    PostalCode = supper?.PostalCode ?? "",
                    CountryID = supper?.CountryID ?? null,
                    OtherPhone = supper?.OtherPhone ?? "",
                    Street = supper?.Street ?? "",
                    Additionaladdress = supper?.Additionaladdress ?? "",
                    Longitude = supper?.Longitude ?? null,
                    Latitude = supper?.Latitude ?? null
                };

                await _purOrderBaseSAddressesRepository.AddAsync(billingAddress);
                await _userInfoService.ActionLogAsync("PurOrderBaseSAddresses", ActionName.DataAdd, null, billingAddress, billingAddress.PurOrderBaseSAddressID, model);

                //var location = await _locationRepository.AllActive()
                //   .Where(l => l.LocationID == model.ToLocationId)
                //   .Select(l => new
                //   {
                //       Wirehouse = l.Wirehouses
                //           .Select(w => new AddressDTO
                //           {
                //               FirstName = w.Manage.FirstName,
                //               LastName = w.Manage.LastName,
                //               FullAddress = w.FullAddress,
                //               Phone = w.Phone,
                //               Email = w.Email,
                //               // Wirehouse doesn't have these, so leave blank
                //               State = null,
                //               City = null,
                //               PostalCode = null,
                //               CountryID = null,
                //               OtherPhone = null,
                //               Street = null,
                //               Additionaladdress = null,
                //               Longitude = null,
                //               Latitude = null
                //           })
                //           .FirstOrDefault(),

                //       Project = l.Projects
                //           .Select(p => new AddressDTO
                //           {
                //               FirstName = p.ProjectManager.FirstName,
                //               LastName = p.ProjectManager.LastName,
                //               FullAddress = p.Address.FullAddress,
                //               Phone = p.Address.Phone,
                //               Email = p.Address.Email,
                //               State = p.Address.State,
                //               City = p.Address.City,
                //               PostalCode = p.Address.PostalCode,
                //               CountryID = p.Address.CountryID,
                //               OtherPhone = p.Address.OtherPhone,
                //               Street = p.Address.Street,
                //               Additionaladdress = p.Address.Additionaladdress,
                //               Longitude = p.Address.Longitude,
                //               Latitude = p.Address.Latitude
                //           })
                //           .FirstOrDefault()
                //   }).FirstOrDefaultAsync();

                var source = new AddressDTO();

                var shippingAddress = new PurOrderBaseSAddresses
                {
                    FirstName = source?.FirstName ?? "",
                    LastName = source?.LastName ?? "",
                    FullAddress = source?.FullAddress ?? "",
                    Phone = source?.Phone ?? model.ShippingInfo.Phone,
                    Email = source?.Email ?? model.ShippingInfo.Email,
                    CreatedAt = DateTime.Now,
                    State = source?.State ?? "",
                    City = source?.City ?? "",
                    PostalCode = source?.PostalCode ?? "",
                    CountryID = source?.CountryID,
                    OtherPhone = source?.OtherPhone ?? "",
                    Street = source?.Street ?? "",
                    Additionaladdress = source?.Additionaladdress ?? "",
                    Longitude = source?.Longitude,
                    Latitude = source?.Latitude
                };

                await _purOrderBaseSAddressesRepository.AddAsync(shippingAddress);
                await _userInfoService.ActionLogAsync("PurOrderBaseSAddresses", ActionName.DataAdd, null, shippingAddress, shippingAddress.PurOrderBaseSAddressID, model);

                if (purchaseOrder != null)
                {
                    purchaseOrder.POID = model.PoNumber;
                    purchaseOrder.PurchaseDate = DateTime.TryParse(model.PoDate, out var poDate) ? poDate : DateTime.Now;
                    purchaseOrder.DueDate = DateTime.TryParse(model.DueDate, out var dueDate) ? dueDate : null;
                    purchaseOrder.WorkOrderDate = DateTime.TryParse(model.WorkOrderDate, out var workOrderDate) ? workOrderDate : null;
                    purchaseOrder.OtherReference = model.OtherReference;
                    purchaseOrder.WorkorderNo = model.WorkOrderNo;
                    purchaseOrder.TaxPercent = model.TaxRate;
                    purchaseOrder.Note = model.Note;
                    purchaseOrder.TermsAndConditions = model.Terms;
                    purchaseOrder.AttachmentLink = model.Attachments != null ? string.Join(",", model.Attachments) : null;
                    purchaseOrder.OBBillingAddressID = billingAddress.PurOrderBaseSAddressID;
                    purchaseOrder.OBShipingAddressID = shippingAddress.PurOrderBaseSAddressID;
                    purchaseOrder.SupplierID = model.SupperId;
                    purchaseOrder.StatusID = status?.StatusID ?? null;
                    //purchaseOrder.ToLocation = model.ToLocationId;

                    purchaseOrder.UpdatedAt = DateTime.Now;
                    purchaseOrder.UpdatedBy = model.UpdatedBy;

                    await _purchasOrdersRepository.UpdateAsync(purchaseOrder);
                }
               
                foreach (var item in model.Items)
                {

                    var orderItemExisting = await _purchasOrderItemsRepository.AllActive()
                        .FirstOrDefaultAsync(i => i.PurchasOrderItemID == item.PurchasOrderItemID && i.PurchasOrderID == model.PurchaseOderId);

                    if (orderItemExisting != null)
                    {
                        orderItemExisting.ProductID = item.SerialNumber;
                        orderItemExisting.Quantity = item.Quantity;
                        orderItemExisting.UnitPrice = item.UnitPrice;
                        orderItemExisting.UpdatedAt = DateTime.Now;
                        orderItemExisting.UpdatedBy = model.UpdatedBy;
                        await _purchasOrderItemsRepository.UpdateAsync(orderItemExisting);
                    }
                }

                await _purchasOrdersRepository.CommitTransactionAsync();

                return (true, $"Purchase order saved successfully with ID: {purchaseOrder?.PurchasOrderID}", purchaseOrder?.PurchasOrderID ?? 0 );
            }
            catch (Exception ex)
            {
                await _purchasOrdersRepository.RollbackTransactionAsync();
                Console.WriteLine($"Error saving purchase order: {ex.Message}");
                return (false, "An error occurred while saving the purchase order", 0);
            }
        }
        #endregion


        #region GetNextPO
        public async Task<string> GetNextPO()
        {
            throw new NotImplementedException();
        }
        #endregion


        #region SaveManualPurchaseOrderAsync
        public async Task<(object success, object message, object purchaseId)> SaveManualPurchaseOrderAsync(PurchaseOrderSubmissionViewModel model)
        {
            await _purchasOrdersRepository.BeginTransactionAsync();

            try
            {
                if (model.BillingInfo == null || string.IsNullOrEmpty(model.BillingInfo.Name))
                {
                    await _purchasOrdersRepository.RollbackTransactionAsync();
                    return (false, "Billing information is required", 0);
                }

                if (model.Items == null || model.Items.Count == 0)
                {
                    await _purchasOrdersRepository.RollbackTransactionAsync();
                    return (false, "At least one item is required", 0);
                }

                if (string.IsNullOrEmpty(model.PoNumber))
                {
                    await _purchasOrdersRepository.RollbackTransactionAsync();
                    return (false, "PO Number is required", 0);
                }

                var status = await _statusRepository.AllActive().FirstOrDefaultAsync(s => s.StatusName.ToLower() == "onprocess");

                if (status == null)
                {
                    status = new Statuses
                    {
                        StatusName = "OnProcess",
                        CreatedAt = DateTime.Now,
                        CreatedBy = model.CreatedBy

                    };
                    await _statusRepository.AddAsync(status);
                    await _userInfoService.ActionLogAsync("Statuses", ActionName.DataAdd, null, status, status.StatusID, model);
                }

                var purchaseOrder1 = await _purchasOrdersRepository.AllActive().FirstOrDefaultAsync(e => e.PurchasOrderID == model.PurchaseOderId);

                if (purchaseOrder1 != null && purchaseOrder1.StatusID == status.StatusID)
                {
                    await _purchasOrdersRepository.RollbackTransactionAsync();
                    return (false, "PO is On Process", 0);
                }

                decimal calculatedTotal = 0;
                foreach (var item in model.Items)
                {
                    if (item.Quantity <= 0 || item.UnitPrice <= 0)
                    {
                        await _purchasOrdersRepository.RollbackTransactionAsync();
                        return (false, $"Invalid quantity or price for item: {item.ItemName}", 0);
                    }

                    calculatedTotal += item.Quantity * item.UnitPrice;
                }

                decimal taxAmount = calculatedTotal * (model.TaxRate / 100);
                decimal grandTotal = calculatedTotal + taxAmount;
                decimal dueAmount = grandTotal - model.PaidAmount;

                var supper = await _supperAddressDetailsRepository.AllActive()
                     .Include(e => e.SupplierAddresses).ThenInclude(e => e.Supplier)
                     .Include(e => e.SupplierAddresses).ThenInclude(e => e.AddressType)
                     .Where(e => e.SupplierAddresses.Any(sa => sa.SupplierID == model.SupperId && sa.AddressType.AddressTypeName == "Billing Address"))
                     .FirstOrDefaultAsync();

                var billingAddress = new PurOrderBaseSAddresses
                {
                    FirstName = supper?.FirstName ?? "",
                    LastName = supper?.LastName ?? "",
                    FullAddress = supper?.FullAddress ?? "",
                    Phone = supper?.Phone ?? model.BillingInfo.Phone,
                    Email = supper?.Email ?? model.BillingInfo.Email,
                    CreatedAt = DateTime.Now,
                    State = supper?.State ?? "",
                    City = supper?.City ?? "",
                    PostalCode = supper?.PostalCode ?? "",
                    CountryID = supper?.CountryID ?? null,
                    OtherPhone = supper?.OtherPhone ?? "",
                    Street = supper?.Street ?? "",
                    Additionaladdress = supper?.Additionaladdress ?? "",
                    Longitude = supper?.Longitude ?? null,
                    Latitude = supper?.Latitude ?? null
                };

                await _purOrderBaseSAddressesRepository.AddAsync(billingAddress);
                await _userInfoService.ActionLogAsync("PurOrderBaseSAddresses", ActionName.DataAdd, null, billingAddress, billingAddress.PurOrderBaseSAddressID, model);

                

                var source = new AddressDTO();

                var shippingAddress = new PurOrderBaseSAddresses
                {
                    FirstName = source?.FirstName ?? "",
                    LastName = source?.LastName ?? "",
                    FullAddress = source?.FullAddress ?? "",
                    Phone = source?.Phone ?? model?.ShippingInfo?.Phone ?? "",
                    Email = source?.Email ?? model?.ShippingInfo?.Email ?? "",
                    CreatedAt = DateTime.Now,
                    State = source?.State ?? "",
                    City = source?.City ?? "",
                    PostalCode = source?.PostalCode ?? "",
                    CountryID = source?.CountryID,
                    OtherPhone = source?.OtherPhone ?? "",
                    Street = source?.Street ?? "",
                    Additionaladdress = source?.Additionaladdress ?? "",
                    Longitude = source?.Longitude ?? 0m,
                    Latitude = source?.Latitude ?? 0m
                };

                await _purOrderBaseSAddressesRepository.AddAsync(shippingAddress);


                


                var purchaseOrder = new PurchasOrders();
               // purchaseOrder.POID = _requisitionApproveService.GenerateNextPoNumberAsync().Result;
                purchaseOrder.PurchaseDate = DateTime.TryParse(model.PoDate, out var poDate) ? poDate : DateTime.Now;
                purchaseOrder.DueDate = DateTime.TryParse(model.DueDate, out var dueDate) ? dueDate : null;
                purchaseOrder.WorkOrderDate = DateTime.TryParse(model.WorkOrderDate, out var workOrderDate) ? workOrderDate : null;
                purchaseOrder.OtherReference = model.OtherReference;
                purchaseOrder.WorkorderNo = model.WorkOrderNo;
                purchaseOrder.TaxPercent = model.TaxRate;
                purchaseOrder.Note = model.Note;
                purchaseOrder.TermsAndConditions = model.Terms;
                purchaseOrder.AttachmentLink = model.Attachments != null ? string.Join(",", model.Attachments) : null;
                purchaseOrder.OBBillingAddressID = billingAddress.PurOrderBaseSAddressID;
                purchaseOrder.OBShipingAddressID = shippingAddress.PurOrderBaseSAddressID;
                purchaseOrder.SupplierID = model.SupperId;
                purchaseOrder.StatusID = status?.StatusID ?? null;
               // purchaseOrder.ToLocation = model.ToLocationId;
                

                purchaseOrder.UpdatedAt = DateTime.Now;
                purchaseOrder.UpdatedBy = model.UpdatedBy;

                await _purchasOrdersRepository.AddAsync(purchaseOrder);

                foreach (var item in model.Items)
                {
                    var orderItemExisting = new PurchasOrderItems();
                    orderItemExisting.ProductID = item.SerialNumber;
                    orderItemExisting.PurchasOrderID = purchaseOrder.PurchasOrderID;
                    orderItemExisting.Quantity = item.Quantity;
                    orderItemExisting.UnitPrice = item.UnitPrice;
                    orderItemExisting.UpdatedAt = DateTime.Now;
                    orderItemExisting.UpdatedBy = model.UpdatedBy;
                    await _purchasOrderItemsRepository.AddAsync(orderItemExisting);
                }


                


                await _purchasOrdersRepository.CommitTransactionAsync();

                return (true, $"Purchase order saved successfully with ID: {purchaseOrder?.POID}", purchaseOrder?.PurchasOrderID ?? 0);
            }
            catch (Exception ex)
            {
                await _purchasOrdersRepository.RollbackTransactionAsync();
                Console.WriteLine($"Error saving purchase order: {ex.Message}");
                return (false, "An error occurred while saving the purchase order", 0);
            }
        }
        #endregion


        
    }
}