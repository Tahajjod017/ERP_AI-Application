using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.POS.Requsition.AddRequisition;

namespace GCTL.Service.POS.Requsition
{
    public interface INewRequisitionService
    {
        Task<PaginatedResultCommon<RequisitionItemViewModel>> GetRequisitionListAsync(int page, int pageSize, string search, string sortColumn, string sortDirection, int? projectId, int? productTypeId, int? empID, string? FromDat, string? ToDate);
        Task<CommonReturnViewModel> SaveRequsitionAsync(CreateRequisitionViewModel model);


        Task<CommonReturnViewModel> DeleteRequisitionAsync(int id, BaseViewModel? baseView, int? empID);
        Task<EditRequisitionViewModel> GetRequisitionByIdAsync(int id, int? empID);
        Task<CommonReturnViewModel> UpdateRequisitionAsync(EditRequisitionViewModel model, int? empID);
        Task<byte[]> GeneratePDF(int orgid, int id, string? FromDate, string? ToDate);
        Task<byte[]> GenerateXL(int orgid, int id, string? FromDate, string? ToDate);
    }
}
