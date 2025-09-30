using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.Finance.TransactionAccountVM;

namespace GCTL_App.ViewModels.Finance.ChartOfAccount
{
    public class TransactionAccountPageVM : BaseViewModel
    {
        public CreateTransactionAccountVM Create { get; set; } = new CreateTransactionAccountVM();
        public UpdateTransactionAccountVM Update { get; set; } = new UpdateTransactionAccountVM();
        public GetByIdTransactionAccountVM GetById { get; set; } = new GetByIdTransactionAccountVM();
        public GetAllTransactionAccountVM GetAll { get; set; } = new GetAllTransactionAccountVM();
        public DeleteTransactionAccountVM Delete { get; set; } = new DeleteTransactionAccountVM();
    }
}
