namespace ifmisIdentity.Models
{
    public class Organization
    {
        public int Id { get; set; } 
        public string Name { get; set; } 
        public string DatabaseName { get; set; } 
        public string Description { get; set; }

        public string OrgUrl { get; set; }
    }
}
