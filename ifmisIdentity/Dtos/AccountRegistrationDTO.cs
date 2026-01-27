namespace ifmisIdentity.Dtos
{
    using System.ComponentModel.DataAnnotations;

    public class AccountRegistrationDTO
    {
        // User Information
        [Required(ErrorMessage = "Username is required.")]
        [MinLength(3, ErrorMessage = "Username must be at least 3 characters long.")]
        [MaxLength(50, ErrorMessage = "Username cannot exceed 50 characters.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Password confirmation is required.")]
        [Compare("Password", ErrorMessage = "Password and confirmation do not match.")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "GlobalUserId is required.")]
       
        public Guid GlobalUserId { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        // Organization Information
        [Required(ErrorMessage = "Organization name is required.")]
        [MinLength(3, ErrorMessage = "Organization name must be at least 3 characters long.")]
        [MaxLength(200, ErrorMessage = "Organization name cannot exceed 200 characters.")]
        public string OrganizationName { get; set; }

        [Required(ErrorMessage = "Organization database name is required.")]
        [MinLength(3, ErrorMessage = "Organization database name must be at least 3 characters long.")]
        [MaxLength(100, ErrorMessage = "Organization database name cannot exceed 100 characters.")]
        public string OrganizationDatabaseName { get; set; }

        [MaxLength(500, ErrorMessage = "Organization description cannot exceed 500 characters.")]
        public string OrganizationDescription { get; set; }

        [Required(ErrorMessage = "Organization URL is required.")]
        [Url(ErrorMessage = "Invalid URL format.")]
        [MaxLength(500, ErrorMessage = "Organization URL cannot exceed 500 characters.")]
        public string OrganizationUrl { get; set; }

        public List<string> Roles { get; set; } = new List<string>();
    }
}
