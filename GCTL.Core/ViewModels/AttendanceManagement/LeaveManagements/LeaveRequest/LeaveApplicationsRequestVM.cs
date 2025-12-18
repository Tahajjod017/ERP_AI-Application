using GCTL.Data.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AttendanceManagement.LeaveManagements.LeaveRequest
{
    


    public class LeaveApplicationsRequestVM : BaseViewModel
    {
        
        [Required(ErrorMessage = "This Field is Required")]
        public int? EmployeeID { get; set; }
        [Required(ErrorMessage = "This Field is Required")]
        public int? LeaveTypeID { get; set; }
        public bool IsFullDay { get; set; } = true;

        [RequiredIf(nameof(IsFullDay), true, ErrorMessage = "Required From Date")]
        public DateOnly? FromDate { get; set; }

        [RequiredIf(nameof(IsFullDay), true, ErrorMessage = "Required To Date")]
        public DateOnly? ToDate { get; set; }

        [RequiredIf(nameof(IsFullDay), false, ErrorMessage = "Start Time is required")]
        public TimeOnly? PartialFromTime { get; set; }

        [RequiredIf(nameof(IsFullDay), false, ErrorMessage = "End Time is required")]
        public TimeOnly? PartialToTime { get; set; }

        [RequiredIf(nameof(IsFullDay), false, ErrorMessage = "Required To Date")]
        public DateOnly? ToDateFromDateCombined { get; set; }
        public int? StatusID { get; set; }
        public string? Reason { get; set; }
        public decimal? LeaveDays { get; set; }

        public bool? IsGroupApplication { get; set; }
        public int? GroupApplicationID { get; set; }
        public class RequiredIfAttribute : ValidationAttribute
        {
            private readonly string _propertyName;
            private readonly object _desiredValue;

            public RequiredIfAttribute(string propertyName, object desiredValue)
            {
                _propertyName = propertyName;
                _desiredValue = desiredValue;
            }

            protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
            {
                var property = validationContext.ObjectType.GetProperty(_propertyName);
                if (property == null)
                    return new ValidationResult($"Unknown property: {_propertyName}");

                var propertyValue = property.GetValue(validationContext.ObjectInstance);

                if (propertyValue?.ToString() == _desiredValue.ToString())
                {
                    if (value == null || (value is string s && string.IsNullOrWhiteSpace(s)))
                        return new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} is required.");
                }

                return ValidationResult.Success;
            }
        }
    }

}
