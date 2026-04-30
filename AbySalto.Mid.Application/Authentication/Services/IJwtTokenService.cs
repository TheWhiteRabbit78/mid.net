using System.Security.Claims;
using AbySalto.Mid.Application.Authentication.Models;

namespace AbySalto.Mid.Application.Authentication.Services
{
    /// <summary>
    /// Service responsible for issuing JWT access tokens and accompanying refresh tokens.
    /// </summary>
    public interface IJwtTokenService
    {
        /// <summary>
        /// Generates a signed JWT access token along with a fresh refresh token for the given claims.
        /// </summary>
        /// <param name="claims">Claims to embed into the access token.</param>
        /// <returns>A <see cref="SecurityTokenResponse"/> with access token, refresh token, and expirations.</returns>
        SecurityTokenResponse GenerateToken(IEnumerable<Claim> claims);
    }
}
