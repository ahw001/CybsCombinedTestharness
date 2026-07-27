namespace CybsClient.Services.DTOs
{
    public class TokenizeRequestDto
    {
        public string? AccountNumber { get; set; }
        public string? ExpMonth { get; set; }
        public string? ExpYear { get; set; }
        public string? CardType { get; set; }
        public int B2cCustomerId { get; set; }
    }
}
