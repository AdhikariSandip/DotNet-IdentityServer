namespace ifmisIdentity.Dtos
{
    public class UpdateUserDTO
    {
       
        public string? NewUsername { get; set; }  // Optional
        public string? CurrentPassword { get; set; }  // Optional
        public string? NewPassword { get; set; }  // Optional
    }
}
