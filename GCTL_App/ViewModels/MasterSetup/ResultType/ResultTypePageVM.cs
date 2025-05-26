using GCTL.Core.ViewModels.MasterSetup.ResultType;

namespace GCTL_App.ViewModels.MasterSetup.ResultType
{
    public class ResultTypePageVM
    {
        public ResultTypeVM Setup { get; set; } = new ResultTypeVM();
        public List<ResultTypeVM> List { get; set; } = new List<ResultTypeVM>();
    }
}
