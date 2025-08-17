using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.Helpers;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.Employee.EmployeeResign;
using GCTL.Core.ViewModels.Employee.EmployeeTermination;

namespace GCTL.Service.Employees.EmployeeTermination
{
    public interface IEmployeeTermination
    {
        object GetTerminations(int page, int pageSize, string sortColumn, string sortDirection, string fromDate, string toDate, string imgSrcThumb);
        Task<CommonReturnViewModel> InsertTermination(TerminationPostViewModel model);
        Task<CommonReturnViewModel> UpdateTermination(int TerminationId, TerminationPostViewModel model);
        Task<CommonReturnViewModel> DeleteTermination(DeleteRequestVM delete);
        CommonReturnViewModel GetTerminationById(int resignationId);



        /// <summary>
        /// Retrieves a list of pending resignations based on provided filters.
        /// </summary>
        /// <param name="dateRange">Date range filter for notice dates (e.g., "d/m/y to d/m/y").</param>
        /// <param name="department">Department ID filter.</param>
        /// <param name="designation">Designation ID filter.</param>
        /// <returns>A list of pending resignation view models.</returns>
        Task<List<TerminationGetViewModel>> GetPendingTerminations(string dateRange, string department, string designation, string terminationType, string imgSrcThumb, int? currentUser);

        /// <summary>
        /// Retrieves a list of processed resignations based on provided filters.
        /// </summary>
        /// <param name="dateRange">Date range filter for processed dates (e.g., "d/m/y to d/m/y").</param>
        /// <param name="department">Department ID filter.</param>
        /// <param name="designation">Designation ID filter.</param>
        /// <returns>A list of processed resignation view models.</returns>
        Task<List<TerminationGetViewModel>> GetProcessedTerminations(string dateRange, string department, string designation, string imgSrcThumb, string imgSrcThumb1, int? currentUser);

        /// <summary>
        /// Retrieves details of a specific resignation by its ID.
        /// </summary>
        /// <param name="id">The ID of the resignation.</param>
        /// <returns>The resignation view model or null if not found.</returns>
        Task<TerminationGetViewModel> GetAppTerminationById(int id);

        /// <summary>
        /// Processes a resignation request with the specified action and details.
        /// </summary>
        /// <param name="id">The ID of the resignation.</param>
        /// <param name="action">The action to perform (e.g., "approve", "decline", "hold").</param>
        /// <param name="hrComments">Optional HR comments.</param>
        /// <param name="handoverStatus">Handover status (e.g., "pending", "in-progress", "completed").</param>
        /// <param name="assetReturned">Whether company assets have been returned.</param>
        /// <param name="clearanceCompleted">Whether department clearance is completed.</param>
        /// <param name="documentsPrepared">Whether exit documents are prepared.</param>
        /// <returns>A result object indicating success or failure with a message.</returns>
        Task<CommonReturnViewModel> ProcessTermination(int id, string action, string hrComments, string handoverStatus, bool assetReturned, bool clearanceCompleted, bool documentsPrepared, CommonBaseViewModel? baseModel);

    }
}
