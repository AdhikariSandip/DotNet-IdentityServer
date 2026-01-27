namespace ifmisIdentity.Dtos
{
    using System.ComponentModel.DataAnnotations;

    public class LoginDTO
    {
        [Required(ErrorMessage = "Username is required.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; }
    }

}
