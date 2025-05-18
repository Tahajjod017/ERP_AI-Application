using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.Language
{
    public interface ITranslateService
    {
        Task<string> GetTranslationAsyncInd(string key, string code, string languageCode);
        string GetTranslationInd(string key, string code, string languageCode);
    }

}
