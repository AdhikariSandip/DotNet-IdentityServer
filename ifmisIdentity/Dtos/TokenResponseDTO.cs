namespace ifmisIdentity.Dtos
{
    public class TokenResponseDTO
    {
        public string AccessToken { get; set; }
        public string IdToken { get; set; }
        public string TokenType { get; set; } = "Bearer";
        public int ExpiresIn { get; set; }
        public string Scope { get; set; }
    }

}
