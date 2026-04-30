namespace AbySalto.Mid.Infrastructure.Authentication
{
    /// <summary>
    /// Strongly-typed configuration for JWT issuance, bound from the "Jwt" configuration section.
    /// </summary>
    public class JwtSettings
    {
        public const string SectionName = "Jwt";

        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public string Secret { get; set; } = string.Empty;
        public int ExpirationMinutes { get; set; } = 60;
        public int RefreshTokenExpirationDays { get; set; } = 7;
        public int RefreshTokenLoginExpirationDays { get; set; } = 30;
    }
}
