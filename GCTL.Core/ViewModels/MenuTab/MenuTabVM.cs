using GCTL.Data.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.MenuTab
{
    public class MenuTabVM : BaseViewModel
    {
        public int MenuTabId { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Type is required.")]
        public string Type { get; set; }

        public int? ParentId { get; set; }

        public int? OrderBy { get; set; }

        public string? ControllerName { get; set; }

        public string? ViewName { get; set; }

        public string? Icon { get; set; }

        public bool IsActive { get; set; } = default!;
    }
}
