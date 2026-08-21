using ERP.Domain.Common;

namespace ERP.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    // Foreign Key & Navigation
    public Guid RoleId { get; set; }
    public virtual Role Role { get; set; } = null!;
}
