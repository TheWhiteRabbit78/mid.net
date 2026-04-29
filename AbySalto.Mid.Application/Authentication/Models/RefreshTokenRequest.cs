namespace AbySalto.Mid.Application.Authentication.Models
{
    /// <summary>
    /// Request body for exchanging a refresh token for a new access token.
    /// </summary>
    public class RefreshTokenRequest
    {
        public string RefreshToken { get; set; } = string.Empty;
    }
}
