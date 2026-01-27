namespace ifmisIdentity.Dtos
{
    using System.Collections.Generic;

    public class ErrorResponseDTO
    {
        public string Message { get; set; }
        public List<string> Errors { get; set; } = new();
    }

}
