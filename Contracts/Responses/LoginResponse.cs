namespace APIAgroConnect.Contracts.Responses
{
    public class LoginResponse
    {
        public long UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public List<string> Roles { get; set; } = new();

        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
}
