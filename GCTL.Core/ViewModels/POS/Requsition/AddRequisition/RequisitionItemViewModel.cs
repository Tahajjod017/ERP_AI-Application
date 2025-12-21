namespace GCTL.Core.ViewModels.POS.Requsition.AddRequisition
{
    public class RequisitionItemViewModel
    {
        public int Id { get; set; }
        public int? RequisitionId { get; set; }
       
        public DateTime? RequisitionDate { get; set; }
        
        public string Status { get; set; }
       
        public string RequitionCode { get; set; }
        public string Note { get; set; }
        public int RequisitionItems { get; set; }
        public string? Priority { get; set; }
        public int? ApprovalStep { get; set; }
        public string RequisitionBy { get; set; }
    }

    public class PaginatedResultCommon<T>
    {
        public List<T> Items { get; set; }
        public int TotalRecords { get; set; }

        public PaginatedResultCommon(List<T> items, int totalRecords)
        {
            Items = items;
            TotalRecords = totalRecords;
        }
    }
}