using Microsoft.AspNetCore.Identity;

namespace ifmisIdentity.Models
{
    public class User : IdentityUser<int>
    {
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLogin { get; set; }
        public Guid GlobalUserId { get; set; }
        public int OrganizationId { get; set; }
        public Organization Organization { get; set; }
        public ICollection<UserRole> UserRoles { get; set; }
       
        // Store a list of old password hashes
        public virtual List<OldPassword> OldPasswords { get; set; } = new List<OldPassword>();
    }
}
