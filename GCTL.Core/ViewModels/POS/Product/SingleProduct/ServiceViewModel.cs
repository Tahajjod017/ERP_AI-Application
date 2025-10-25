using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.POS.Product.SingleProduct
{
    public class ServiceViewModel
    {
        [Required]
        public string ServiceSelectService { get; set; }
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Hourly rate must be greater than 0.")]
        public decimal? ServiceHourlyRate { get; set; }
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Daily rate must be greater than 0.")]
        public decimal? ServiceDailyRate { get; set; }
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Per job rate must be greater than 0.")]
        public decimal? ServicePerJobRate { get; set; }
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Per meter rate must be greater than 0.")]
        public decimal? ServicePerMeterRate { get; set; }
        public string ServiceSearch { get; set; }
        public string ServiceSort { get; set; }
        public List<ServiceListViewModel> ServiceServices { get; set; } = new List<ServiceListViewModel>();
    }
    public class ServiceListViewModel
    {
        public string ServiceServiceName { get; set; }
        public decimal ServiceHourlyRate { get; set; }
        public decimal ServiceDailyRate { get; set; }
        public decimal ServicePerJobRate { get; set; }
        public decimal ServicePerMeterRate { get; set; }
        public string ServiceCreatedDate { get; set; }
        public string ServiceCreatedTime { get; set; }
    }
}
