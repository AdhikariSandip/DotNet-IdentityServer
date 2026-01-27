namespace ifmisIdentity.Dtos
{
    public class ApplicationDTO
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string DisplayName { get; set; }
        public List<string> Permissions { get; set; } = new();
    }

}
