using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.Finance.BaseAccountVM;

namespace GCTL_App.ViewModels.Finance.MasterSetup
{
    public class BaseAccountPageVM : BaseViewModel
    {
        public CreateBaseAccountVM Create { get; set; } = new CreateBaseAccountVM();
        public UpdateBaseAccountVM Update { get; set; } = new UpdateBaseAccountVM();
        public GetByIdBaseAccountVM GetById { get; set; } = new GetByIdBaseAccountVM();
        public GetAllBaseAccountVM GetAll { get; set; } = new GetAllBaseAccountVM();
        public DeleteBaseAccountVM Delete { get; set; } = new DeleteBaseAccountVM();
    }
}
