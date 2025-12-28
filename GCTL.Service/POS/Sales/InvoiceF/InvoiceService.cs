using GCTL.Core.Helpers;
using GCTL.Core.Helpers.Jsonserialize;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.POS.Sales;
using GCTL.Core.ViewModels.POS.Sales.InvoiceF;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace GCTL.Service.POS.Sales.InvoiceF
{

    public class InvoiceService : IInvoice
    {
        private readonly IGenericRepository<Invoices> _invoiceRepository;
        private readonly IGenericRepository<InvoiceItems> _invoiceItemRepository;
       // private readonly IGenericRepository<InvoicesVersions> _invoiceVersionRepository;
        private readonly IGenericRepository<InvoiceBaseCAddresses> _addressRepository;
        private readonly IUserInfoService _userInfoService;

        public InvoiceService(
            IGenericRepository<Invoices> invoiceRepository,
            IGenericRepository<InvoiceItems> invoiceItemRepository,
           // IGenericRepository<InvoicesVersions> invoiceVersionRepository,
            IGenericRepository<InvoiceBaseCAddresses> addressRepository,
            IUserInfoService userInfoService)
        {
            _invoiceRepository = invoiceRepository;
            _invoiceItemRepository = invoiceItemRepository;
          //  _invoiceVersionRepository = invoiceVersionRepository;
            _addressRepository = addressRepository;
            _userInfoService = userInfoService;
        }

        public async Task<string> GetNextInvoiceCode()
        {
            var lastInvoice = await _invoiceRepository.AllActive()
                .OrderByDescending(i => i.InvoiceNumber)
                .Select(i => i.InvoiceNumber)
                .FirstOrDefaultAsync();

            int nextNumber = 1;

            if (!string.IsNullOrEmpty(lastInvoice) && lastInvoice.StartsWith("INV"))
            {
                string numericPart = lastInvoice.Substring(3);
                if (int.TryParse(numericPart, out int parsed))
                {
                    nextNumber = parsed + 1;
                }
            }

            string nextInvoice = $"INV{nextNumber.ToString("D5")}";
            return nextInvoice;
        }

        public async Task<CommonReturnViewModel> SaveAsync(InvoiceViewModel vm, bool isUpdate = false)
        {
            await _invoiceItemRepository.BeginTransactionAsync();

            try
            {
                int vers = 1;
                // Generate invoice number if not provided
                if (string.IsNullOrEmpty(vm.InvoiceNumber) || vm.InvoiceNumber.StartsWith("INV-"))
                {
                    vm.InvoiceNumber = await GetNextInvoiceCode();
                }

                
                
                var invoice = await _invoiceRepository.AllActive().FirstOrDefaultAsync(e => e.InvoiceNumber == vm.InvoiceNumber);
                
                

                //if (isUpdate)
                //{
                //    vers = invoice != null ? invoice.InvoicesVersions.Count + 1 : 1;

                //}
               

                // Save Billing Address
                int? billingAddressId = null;
                if (vm.BillingAddress != null)
                {
                    var billingAddress = new InvoiceBaseCAddresses
                    {
                        FirstName = vm.BillingAddress.FirstName,
                        LastName = vm.BillingAddress.LastName,
                        FullAddress = vm.BillingAddress.FullAddress,
                        City = vm.BillingAddress.City,
                        State = vm.BillingAddress.State,
                        PostalCode = vm.BillingAddress.PostalCode,
                        Phone = vm.BillingAddress.Phone,
                        Email = vm.BillingAddress.Email,
                        CreatedAt = DateTime.Now,
                        CreatedBy = vm.CreatedBy
                    };
                    await _addressRepository.AddAsync(billingAddress);
                    billingAddressId = billingAddress.InvoiceBaseCAddressID;
                }

                // Save Shipping Address
                int? shippingAddressId = null;
                if (vm.ShippingAddress != null)
                {
                    var shippingAddress = new InvoiceBaseCAddresses
                    {
                        FirstName = vm.ShippingAddress.FirstName,
                        LastName = vm.ShippingAddress.LastName,
                        FullAddress = vm.ShippingAddress.FullAddress,
                        City = vm.ShippingAddress.City,
                        State = vm.ShippingAddress.State,
                        PostalCode = vm.ShippingAddress.PostalCode,
                        Phone = vm.ShippingAddress.Phone,
                        Email = vm.ShippingAddress.Email,
                        CreatedAt = DateTime.Now,
                        CreatedBy = vm.CreatedBy
                    };
                    await _addressRepository.AddAsync(shippingAddress);
                    shippingAddressId = shippingAddress.InvoiceBaseCAddressID;
                }

                // Calculate totals
                decimal subTotal = vm.Items.Sum(i => i.Amount);
                decimal vatAmount = subTotal * vm.VatPercent / 100;
                decimal grandTotal = subTotal + vatAmount;



                if (invoice == null)
                {
                    invoice = new Invoices
                    {
                        SalesOrderVersionID = vm.SelectedSalesOrderId,
                        InvoiceNumber = await GetNextInvoiceCode(),
                        Version = vers,
                        IsDraft = vm.IsDraft,
                        IsFinal = !vm.IsDraft,
                        //InvoiceNumber = vm.InvoiceNumber,
                        CustomerID = vm.SelectedCustomerId,
                        IBaseBillingAddressID = billingAddressId,
                        IBaseShippingAddressID = shippingAddressId,

                        InvoiceDate = vm.InvoiceDate,
                        VatPercentage = vm.VatPercent,
                        VatAmount = vatAmount,
                        SubTotal = subTotal,
                        GrandTotal = grandTotal,
                        PaidAmount = 0,
                        OtherReference = vm.OtherReference,
                        InvoiceNote = vm.InvoiceNote,
                        CreatedAt = DateTime.Now,
                        CreatedBy = vm.CreatedBy
                    };
                    await _invoiceRepository.AddAsync(invoice, vm);
                    await _userInfoService.ActionLogAsync("Invoice", ActionName.DataAdd, null, invoice, invoice.InvoiceID, vm);
                }

                // Create invoice header
                

                // Save invoice items
                foreach (var item in vm.Items)
                {
                    var modelItem = new InvoiceItems
                    {
                        InvoiceID = invoice.InvoiceID,
                        ProductID = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        CreatedAt = DateTime.Now,
                        CreatedBy = vm.CreatedBy
                    };

                    await _invoiceItemRepository.AddAsync(modelItem);
                    await _userInfoService.ActionLogAsync("Invoice", ActionName.DataAdd, null, modelItem, modelItem.InvoiceItemID, vm);
                }

                // Commit transaction
                await _invoiceItemRepository.CommitTransactionAsync();

                return new CommonReturnViewModel
                {
                    Success = true,
                    Message = "Invoice saved successfully",
                    Data = invoice.InvoiceID
                };
            }
            catch (Exception ex)
            {
                await _invoiceItemRepository.RollbackTransactionAsync();
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }


        public async Task<CommonReturnViewModel> UpdateAsync(InvoiceViewModel vm)
        {
            await _invoiceItemRepository.OpenTransactionAsync();

            try
            {
                // Fetch existing invoice
                var invoice = await _invoiceRepository.AllActive().FirstOrDefaultAsync(i => i.InvoiceID == vm.Id);
                if (invoice == null)
                {
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = "Invoice not found"
                    };
                }

                // If invoice is finalized, create new instead of updating
                if (invoice.IsDraft == false)
                {
                    await _invoiceRepository.CompleteTransactionAsync();
                    var data = await SaveAsync(vm, true);
                    return data;
                }

                var beforeEntity = JsonConvert.DeserializeObject<CustomerListVM>(
                    JsonConvert.SerializeObject(invoice, JsonSettings.IgnoreReferenceLoop));

                // Update Billing Address
                if (vm.BillingAddress != null && invoice.IBaseBillingAddressID.HasValue)
                {
                    var billingAddress = await _addressRepository.GetByIdAsync(invoice.IBaseBillingAddressID.Value);
                    if (billingAddress != null)
                    {
                        billingAddress.FirstName = vm.BillingAddress.FirstName;
                        billingAddress.LastName = vm.BillingAddress.LastName;
                        billingAddress.FullAddress = vm.BillingAddress.FullAddress;
                        billingAddress.City = vm.BillingAddress.City;
                        billingAddress.State = vm.BillingAddress.State;
                        billingAddress.PostalCode = vm.BillingAddress.PostalCode;
                        billingAddress.Phone = vm.BillingAddress.Phone;
                        billingAddress.Email = vm.BillingAddress.Email;
                        billingAddress.UpdatedAt = DateTime.Now;
                        billingAddress.UpdatedBy = vm.UpdatedBy;
                        await _addressRepository.UpdateAsync(billingAddress);
                    }
                }

                // Update Shipping Address
                if (vm.ShippingAddress != null && invoice.IBaseShippingAddressID.HasValue)
                {
                    var shippingAddress = await _addressRepository.GetByIdAsync(invoice.IBaseShippingAddressID.Value);
                    if (shippingAddress != null)
                    {
                        shippingAddress.FirstName = vm.ShippingAddress.FirstName;
                        shippingAddress.LastName = vm.ShippingAddress.LastName;
                        shippingAddress.FullAddress = vm.ShippingAddress.FullAddress;
                        shippingAddress.City = vm.ShippingAddress.City;
                        shippingAddress.State = vm.ShippingAddress.State;
                        shippingAddress.PostalCode = vm.ShippingAddress.PostalCode;
                        shippingAddress.Phone = vm.ShippingAddress.Phone;
                        shippingAddress.Email = vm.ShippingAddress.Email;
                        shippingAddress.UpdatedAt = DateTime.Now;
                        shippingAddress.UpdatedBy = vm.UpdatedBy;
                        await _addressRepository.UpdateAsync(shippingAddress);
                    }
                }

                // Recalculate totals
                decimal subTotal = vm.Items.Sum(i => i.Amount);
                decimal vatAmount = subTotal * vm.VatPercent / 100;
                decimal grandTotal = subTotal + vatAmount;

                // Update invoice header
                invoice.CustomerID = vm.SelectedCustomerId;
                //invoice.SalesOrdersID = vm.SelectedSalesOrderId;
                invoice.InvoiceDate = vm.InvoiceDate;
                invoice.InvoiceNumber = vm.InvoiceNumber;
                invoice.VatPercentage = vm.VatPercent;
                invoice.SubTotal = subTotal;
                invoice.VatAmount = vatAmount;
                invoice.GrandTotal = grandTotal;
                invoice.OtherReference = vm.OtherReference;
                invoice.InvoiceNote = vm.InvoiceNote;
                invoice.IsDraft = vm.IsDraft;
                invoice.UpdatedAt = DateTime.Now;
                invoice.UpdatedBy = vm.UpdatedBy;

                await _invoiceRepository.UpdateAsync(invoice);

                var afterEntity = JsonConvert.DeserializeObject<CustomerListVM>(JsonConvert.SerializeObject(invoice, JsonSettings.IgnoreReferenceLoop));

                await _userInfoService.ActionLogAsync("Invoice", ActionName.DataUpdated, beforeEntity, afterEntity, invoice.InvoiceID, vm);

                // Replace existing items
                var existingItems = await _invoiceItemRepository.AllActive()
                    .Where(e => e.InvoiceID == invoice.InvoiceID).ToListAsync();

                await _invoiceItemRepository.DeleteRangeAsync(existingItems);

                foreach (var item in vm.Items)
                {
                    var modelItem = new InvoiceItems
                    {
                        InvoiceID = invoice.InvoiceID,
                        ProductID = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        CreatedAt = DateTime.Now,
                        CreatedBy = vm.UpdatedBy
                    };

                    await _invoiceItemRepository.AddAsync(modelItem);
                    await _userInfoService.ActionLogAsync("Invoice Item", ActionName.DataAdd,
                        null, modelItem, modelItem.InvoiceItemID, vm);
                }

                // Commit transaction
                await _invoiceItemRepository.CommitTransactionAsync();

                return new CommonReturnViewModel
                {
                    Success = true,
                    Message = "Invoice updated successfully",
                    Data = invoice.InvoiceID
                };
            }
            catch (Exception ex)
            {
                await _invoiceItemRepository.RollbackTransactionAsync();
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }


        //public async Task<CommonReturnViewModel> UpdateAsync(InvoiceViewModel vm)
        //{
        //    await _invoiceItemRepository.BeginTransactionAsync();

        //    try
        //    {
        //        // Fetch existing invoice
        //        var invoice = await _invoiceRepository.GetByIdAsync(vm.Id);
        //        if (invoice == null)
        //        {
        //            return new CommonReturnViewModel
        //            {
        //                Success = false,
        //                Message = "Invoice not found"
        //            };
        //        }

        //       // if (invoice.IsDraft == false)
        //        if (false)
        //        {
        //            var data = await SaveAsync(vm);
        //            return data;
        //        }
        //        else
        //        {
        //            var beforeEntity = JsonConvert.DeserializeObject<CustomerListVM>(JsonConvert.SerializeObject(invoice, JsonSettings.IgnoreReferenceLoop));



        //            if (vm.BillingAddress != null && invoice.IBaseBillingAddressID.HasValue)
        //            {
        //                var billingAddress = await _addressRepository.GetByIdAsync(invoice.IBaseBillingAddressID.Value);
        //                if (billingAddress != null)
        //                {
        //                    billingAddress.FirstName = vm.BillingAddress.FirstName;
        //                    billingAddress.LastName = vm.BillingAddress.LastName;
        //                    billingAddress.FullAddress = vm.BillingAddress.FullAddress;
        //                    billingAddress.City = vm.BillingAddress.City;
        //                    billingAddress.State = vm.BillingAddress.State;
        //                    billingAddress.PostalCode = vm.BillingAddress.PostalCode;
        //                    billingAddress.Phone = vm.BillingAddress.Phone;
        //                    billingAddress.Email = vm.BillingAddress.Email;
        //                    billingAddress.UpdatedAt = DateTime.Now;
        //                    billingAddress.UpdatedBy = vm.UpdatedBy;
        //                    await _addressRepository.UpdateAsync(billingAddress);
        //                }
        //            }

        //            // Update Shipping Address
        //            if (vm.ShippingAddress != null && invoice.IBaseShippingAddressID.HasValue)
        //            {
        //                var shippingAddress = await _addressRepository.GetByIdAsync(invoice.IBaseShippingAddressID.Value);
        //                if (shippingAddress != null)
        //                {
        //                    shippingAddress.FirstName = vm.ShippingAddress.FirstName;
        //                    shippingAddress.LastName = vm.ShippingAddress.LastName;
        //                    shippingAddress.FullAddress = vm.ShippingAddress.FullAddress;
        //                    shippingAddress.City = vm.ShippingAddress.City;
        //                    shippingAddress.State = vm.ShippingAddress.State;
        //                    shippingAddress.PostalCode = vm.ShippingAddress.PostalCode;
        //                    shippingAddress.Phone = vm.ShippingAddress.Phone;
        //                    shippingAddress.Email = vm.ShippingAddress.Email;
        //                    shippingAddress.UpdatedAt = DateTime.Now;
        //                    shippingAddress.UpdatedBy = vm.UpdatedBy;
        //                    await _addressRepository.UpdateAsync(shippingAddress);
        //                }
        //            }

        //            // Recalculate totals
        //            decimal subTotal = vm.Items.Sum(i => i.Amount);
        //            decimal vatAmount = (subTotal * vm.VatPercent) / 100;
        //            decimal grandTotal = subTotal + vatAmount;

        //            // Update invoice header
        //            invoice.CustomerID = vm.SelectedCustomerId;
        //            invoice.SalesOrderID = vm.SelectedSalesOrderId;
        //            invoice.InvoiceDate = vm.InvoiceDate;
        //            invoice.InvoiceNumber = vm.InvoiceNumber;
        //            invoice.VatPercentage = vm.VatPercent;
        //            invoice.SubTotal = subTotal;
        //            invoice.VatAmount = vatAmount;
        //            invoice.GrandTotal = grandTotal;
        //            invoice.OtherReference = vm.OtherReference;
        //            invoice.InvoiceNote = vm.InvoiceNote;
        //            invoice.IsDraft = vm.IsDraft;
        //            invoice.UpdatedAt = DateTime.Now;
        //            invoice.UpdatedBy = vm.UpdatedBy;

        //            await _invoiceRepository.UpdateAsync(invoice);

        //            var afterEntity = JsonConvert.DeserializeObject<CustomerListVM>(JsonConvert.SerializeObject(invoice, JsonSettings.IgnoreReferenceLoop));
        //            await _userInfoService.ActionLogAsync("Customer", ActionName.DataUpdated, beforeEntity, afterEntity, invoice.CustomerID, vm);




        //            var existingItems = await _invoiceItemRepository.AllActive().Where(e => e.InvoiceID == invoice.InvoiceID).ToListAsync();
        //            await _invoiceItemRepository.DeleteRangeAsync(existingItems);

        //            foreach (var existing in existingItems)
        //            {
        //                await _invoiceItemRepository.DeleteAsync(existing);
        //            }

        //            foreach (var item in vm.Items)
        //            {
        //                var modelItem = new InvoiceItems
        //                {
        //                    InvoiceID = invoice.InvoiceID,
        //                    ProductID = item.ProductId,
        //                    Quantity = item.Quantity,
        //                    UnitPrice = item.UnitPrice,
        //                    CreatedAt = DateTime.Now,
        //                    CreatedBy = vm.UpdatedBy
        //                };

        //                await _invoiceItemRepository.AddAsync(modelItem);
        //                await _userInfoService.ActionLogAsync("Invoice", ActionName.DataAdd, null, modelItem, modelItem.InvoiceItemID, vm);
        //            }

        //            // Commit transaction
        //            await _invoiceItemRepository.CommitTransactionAsync();

        //            return new CommonReturnViewModel
        //            {
        //                Success = true,
        //                Message = "Invoice updated successfully",
        //                Data = invoice.InvoiceID
        //            };
        //        }


        //    }
        //    catch (Exception ex)
        //    {
        //        await _invoiceItemRepository.RollbackTransactionAsync();
        //        return new CommonReturnViewModel
        //        {
        //            Success = false,
        //            Message = ex.Message
        //        };
        //    }
        //}



    }

}
