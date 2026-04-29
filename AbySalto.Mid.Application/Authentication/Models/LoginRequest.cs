namespace AbySalto.Mid.Application.Authentication.Models
{
    /// <summary>
    /// Request body for username + password authentication.
    /// </summary>
    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
