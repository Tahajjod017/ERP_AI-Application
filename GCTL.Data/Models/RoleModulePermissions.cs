using System;
using System.Collections.Generic;

namespace GCTL.Data.Models;

public partial class RoleModulePermissions
{
    public int Id { get; set; }

    public string RoleId { get; set; }

    public int MenuTabId { get; set; }

    public int PermissionId { get; set; }

    public bool IsGranted { get; set; }

    public virtual MenuTab MenuTab { get; set; }

    public virtual Permissions Permission { get; set; }

    public virtual ApplicationRole Role { get; set; }
}
