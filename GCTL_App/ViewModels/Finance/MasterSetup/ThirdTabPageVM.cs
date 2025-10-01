using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.Finance.ThirdTabVM;

namespace GCTL_App.ViewModels.Finance.MasterSetup
{
    public class ThirdTabPageVM : BaseViewModel
    {
        public CreateThirdTabVM Create { get; set; } = new CreateThirdTabVM();
        public UpdateThirdTabVM Update { get; set; } = new UpdateThirdTabVM();
        public GetByIdThirdTabVM GetById { get; set; } = new GetByIdThirdTabVM();
        public GetAllThirdTabVM GetAll { get; set; } = new GetAllThirdTabVM();
        public DeleteThirdTabVM Delete { get; set; } = new DeleteThirdTabVM();
    }
}
