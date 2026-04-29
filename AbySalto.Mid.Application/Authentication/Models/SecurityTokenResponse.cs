namespace AbySalto.Mid.Application.Authentication.Models
{
    /// <summary>
    /// Represents the response containing security token information returned to the client
    /// after a successful authentication or refresh operation.
    /// </summary>
    public class SecurityTokenResponse
    {
        public SecurityTokenResponse()
        {
            AccessToken = string.Empty;
            RefreshToken = string.Empty;
        }

        /// <summary>
        /// The signed JWT access token used to authorize subsequent requests.
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// The token type. Always "Bearer".
        /// </summary>
        public static string TokenType => "Bearer";

        /// <summary>
        /// Access token expiration as Unix epoch seconds.
        /// </summary>
        public long ExpiresIn { get; set; }

        /// <summary>
        /// Opaque refresh token used to obtain a new access token without re-authenticating.
        /// </summary>
        public string RefreshToken { get; set; }

        /// <summary>
        /// UTC expiration timestamp of the refresh token.
        /// </summary>
        public DateTime RefreshTokenExpiration { get; set; }
    }
}
