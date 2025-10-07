using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.Finance.AddSubAccountVM;

namespace GCTL_App.ViewModels.Finance.ChartOfAccount
{
    public class AddSubAccountPageVM : BaseViewModel
    {
        public CreateAddSubAccountVM Create { get; set; } = new CreateAddSubAccountVM();
        public UpdateAddSubAccountVM Update { get; set; } = new UpdateAddSubAccountVM();
        public GetByIdAddSubAccountVM GetById { get; set; } = new GetByIdAddSubAccountVM();
        public GetAllAddSubAccountVM GetAll { get; set; } = new GetAllAddSubAccountVM();
        public DeleteAddSubAccountVM Delete { get; set; } = new DeleteAddSubAccountVM();
    }
}
