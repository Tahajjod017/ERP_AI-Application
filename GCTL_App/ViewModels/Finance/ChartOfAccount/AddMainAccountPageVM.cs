using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.Finance.AddMainAccountVM;

namespace GCTL_App.ViewModels.Finance.ChartOfAccount
{
    public class AddMainAccountPageVM : BaseViewModel
    {
        public CreateAddMainAccountVM Create { get; set; } = new CreateAddMainAccountVM();
        public UpdateAddMainAccountVM Update { get; set; } = new UpdateAddMainAccountVM();
        public GetByIdAddMainAccountVM GetById { get; set; } = new GetByIdAddMainAccountVM();
        public GetAllAddMainAccountVM GetAll { get; set; } = new GetAllAddMainAccountVM();
        public DeleteAddMainAccountVM Delete { get; set; } = new DeleteAddMainAccountVM();
    }
}
