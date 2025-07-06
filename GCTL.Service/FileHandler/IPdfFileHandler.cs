using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace GCTL.Service.FileHandler
{
    public interface IPdfFileHandler
    {
        void ComposeHeader(IContainer container, int companyId, bool showOnce = false);
        void ComposeWatermark(IContainer container, int companyId, float opacity = 0.1f);
    }
}
