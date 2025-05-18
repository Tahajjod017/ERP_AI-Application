using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.Language
{
    public class CommonLanguageVM
    {
        public int Id { get; set; }
        public int LangId { get; set; }
        public string EngText { get; set; }
        public string TranslatedText { get; set; }
        public string LangCode { get; set; }

    }
}
