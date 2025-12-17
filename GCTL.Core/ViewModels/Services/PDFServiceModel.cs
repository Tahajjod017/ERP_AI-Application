using System.Collections.Generic;

namespace GCTL.Core.ViewModels.Services
{
    public class PDFServiceModel
    {
        public List<String>? Title { get; set; }
        // 
        public string? View { get; set; }
        public string? SortBy { get; set; }
        public List<HeaderInfo> Headers { get; set; } = new();
        public List<List<CellInfo>> Rows { get; set; } = new();

        public TopBox TopBox { get; set; }
        public LeftBox LeftBox { get; set; }
        public RightBox RightBox { get; set; }
        public FooterBox FooterBox { get; set; }
    }

    public class HeaderInfo
    {
        public string Text { get; set; }
        public float Width { get; set; } = 0; // 0 means auto (relative)
        public bool WrapText { get; set; } = false;
    }

    public class CellInfo
    {
        public string Text { get; set; }
        public string Align { get; set; } = "left";
        // column will expanded or not
        public bool WrapText { get; set; } = false;

    }

    public class KeyAndValue
    {
        public string Key { get; set; }
        public string Value { get; set; }
        // both or only Value
        public string? Show { get; set; }
        // delcear font size
        public int? FontSize { get; set; }
    }

    public class TopBox
    {
        public string? Title { get; set; }
        public List<KeyAndValue> KeyAndValues { get; set; } = new();
    }
    public class LeftBox
    {
        public string? Title { get; set; }
        public List<KeyAndValue> KeyAndValues { get; set; } = new();
    }

    public class RightBox
    {
        public string? Title { get; set; }
        public List<KeyAndValue> KeyAndValues { get; set; } = new();
    }

    public class FooterBox
    {
        public string? Title { get; set; }
        public List<KeyAndValue> KeyAndValues { get; set; } = new();
    }
}
