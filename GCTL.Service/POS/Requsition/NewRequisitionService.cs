using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.POS.Requsition.AddRequisition;

namespace GCTL.Service.POS.Requsition
{
    public class NewRequisitionService : INewRequisitionService
    {
        public Task<CommonReturnViewModel> DeleteRequisitionAsync(int id, BaseViewModel? baseView, int? empID)
        {
            throw new NotImplementedException();
        }

        public Task<byte[]> GeneratePDF(int orgid, int id, string? FromDate, string? ToDate)
        {
            throw new NotImplementedException();
        }

        public Task<byte[]> GenerateXL(int orgid, int id, string? FromDate, string? ToDate)
        {
            throw new NotImplementedException();
        }

        public Task<EditRequisitionViewModel> GetRequisitionByIdAsync(int id, int? empID)
        {
            throw new NotImplementedException();
        }

        public Task<PaginatedResultCommon<RequisitionItemViewModel>> GetRequisitionListAsync(int page, int pageSize, string search, string sortColumn, string sortDirection, int? projectId, int? productTypeId, int? empID, string? FromDat, string? ToDate)
        {
            throw new NotImplementedException();
        }

        public Task<CommonReturnViewModel> SaveRequsitionAsync(CreateRequisitionViewModel model)
        {
            throw new NotImplementedException();
        }

        public Task<CommonReturnViewModel> UpdateRequisitionAsync(EditRequisitionViewModel model, int? empID)
        {
            throw new NotImplementedException();
        }
    }
}
