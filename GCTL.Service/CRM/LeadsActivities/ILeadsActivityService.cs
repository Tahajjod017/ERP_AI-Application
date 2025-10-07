using Azure;
using GCTL.Core.DataTables;
using GCTL.Core.ViewModels.CRM;
using OfficeOpenXml.Drawing.Chart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.CRM.LeadsActivities
{
    public interface ILeadsActivityService
    {
        Task<ReturnDataView<LeadDetailsDTO>> GetUpcomingActivityList(int page, int itemPerPage, string search, string sort, string direction, string dateRange, int? userID, int? CustomerTypeID, string? LeadStatusID, int? ActivityTypeID);
        Task<byte[]> GeneratePDF();
    }
}
