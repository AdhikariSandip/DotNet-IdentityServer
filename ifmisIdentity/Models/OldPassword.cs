namespace ifmisIdentity.Models
{
    public class OldPassword
    {
        public int Id { get; set; }
        
        public string PasswordHash { get; set; }
        public DateTime ChangedDate { get; set; }
        public int UserId { get; set; }
        public virtual User User { get; set; }
    }
}
