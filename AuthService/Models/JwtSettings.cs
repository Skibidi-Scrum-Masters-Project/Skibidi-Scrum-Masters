namespace AuthService.Models
{
    public class JwtSettings
    {
        public string Secret { get; set; } = string.Empty;
        public string Issuer { get; set; } = "FitnessApp";
        public string Audience { get; set; } = "FitnessApp-Users";
        public int ExpirationMinutes { get; set; } = 30;
        
        public int ExpirationRefresh {  get; set; } = 10000;
    }
}