using GCTL.Core.Enums;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.Login;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.Helpers
{
    //public static class AuditExtensions
    //{
    //    public static TestVM ToAudit(this TestVM model, LoginViewModel info, bool isUpdate = false)
    //    {
    //        model.LIP = info.LIP;
    //        model.LMAC = info.LMAC;
    //        model.Email = info.Email;
           

    //        return model;
    //    }
    //}
}

public class TestVM
{
    [Required]
    [EmailAddress]
    public string? Email { get; set; }

    [Required]
    [DataType(DataType.Password)]
    public string? Password { get; set; }

    [Display(Name = "Remember me?")]
    public bool RememberMe { get; set; }

    public string? LIP { get; set; }
    public string? LMAC { get; set; }
} 