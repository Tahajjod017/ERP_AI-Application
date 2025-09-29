using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.Finance.SecondTabVM;

namespace GCTL_App.ViewModels.Finance.MasterSetup
{
    public class SecondTabPageVM : BaseViewModel
    {
        public CreateSecondTabVM Create { get; set; } = new CreateSecondTabVM();
        public UpdateSecondTabVM Update { get; set; } = new UpdateSecondTabVM();
        public GetByIdSecondTabVM GetById { get; set; } = new GetByIdSecondTabVM();
        public GetAllSecondTabVM GetAll { get; set; } = new GetAllSecondTabVM();
        public DeleteSecondTabVM Delete { get; set; } = new DeleteSecondTabVM();
    }
}
