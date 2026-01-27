namespace ifmisIdentity.Dtos
{
   
        using System.ComponentModel.DataAnnotations;

        public class ClientDTO
        {
            [Required(ErrorMessage = "Client ID is required.")]
            [MaxLength(100, ErrorMessage = "Client ID cannot exceed 100 characters.")]
            public string ClientId { get; set; }

            [MaxLength(100, ErrorMessage = "Client secret cannot exceed 100 characters.")]
            public string ClientSecret { get; set; }

            [Required(ErrorMessage = "Display name is required.")]
            [MaxLength(200, ErrorMessage = "Display name cannot exceed 200 characters.")]
            public string DisplayName { get; set; }

            [Required(ErrorMessage = "Type is required.")]
            [MaxLength(50, ErrorMessage = "Type cannot exceed 50 characters.")]
            public string Type { get; set; } // Example: "confidential" or "public"

            public List<string> Permissions { get; set; } = new List<string>();

            public List<string> RedirectUris { get; set; } = new List<string>();

            public List<string> PostLogoutRedirectUris { get; set; } = new List<string>();
        }
    }


