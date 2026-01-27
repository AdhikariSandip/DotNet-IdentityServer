using Microsoft.AspNetCore.Identity;

namespace ifmisIdentity.Models
{
    public class Role : IdentityRole<int>
    {
        public ICollection<UserRole> UserRoles { get; set; }
    }
}
