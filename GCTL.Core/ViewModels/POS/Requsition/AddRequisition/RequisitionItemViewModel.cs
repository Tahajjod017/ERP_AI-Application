namespace GCTL.Core.ViewModels.POS.Requsition.AddRequisition
{
    public class RequisitionItemViewModel
    {
        public int Id { get; set; }
        public int? RequisitionId { get; set; }
        public string ProductName { get; set; }
        public string ProductType { get; set; }
        public DateTime? RequisitionDate { get; set; }
        public string Unit { get; set; }
        public int StockInWarehouse { get; set; }
        public int UnusedQuantity { get; set; }
        public decimal? RequisitionQuantity { get; set; }
        public string Status { get; set; }
        public decimal? ApproveQuantity { get; set; }
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