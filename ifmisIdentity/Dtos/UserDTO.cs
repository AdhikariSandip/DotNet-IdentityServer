namespace ifmisIdentity.Dtos
{
    public class UserDTO
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }
        public string PasswordHash { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLogin { get; set; }
        public Guid GlobalUserId { get; set; }
        public int OrganizationID { get; set; }
        public List<string> Roles { get; set; }
    }

}
