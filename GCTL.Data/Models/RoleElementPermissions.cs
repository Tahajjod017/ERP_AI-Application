using System;
using System.Collections.Generic;

namespace GCTL.Data.Models;

public partial class RoleElementPermissions
{
    public int Id { get; set; }

    public string RoleId { get; set; }

    public int PageId { get; set; }

    public string ElementKey { get; set; }

    public virtual Pages Page { get; set; }

    public virtual ApplicationRole Role { get; set; }
}
