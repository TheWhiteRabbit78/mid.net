using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AbySalto.Mid.Application.Authentication.Models;
using AbySalto.Mid.Application.Authentication.Services;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AbySalto.Mid.Infrastructure.Authentication
{
    /// <summary>
    /// Default implementation of <see cref="IJwtTokenService"/> using HS256-signed JWTs
    /// and an opaque base64-encoded GUID for the refresh token.
    /// </summary>
    public class JwtTokenService : IJwtTokenService
    {
        private readonly JwtSettings _settings;

        public JwtTokenService(IOptions<JwtSettings> options)
        {
            _settings = options.Value;
        }

        public SecurityTokenResponse GenerateToken(IEnumerable<Claim> claims)
        {
            SymmetricSecurityKey signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));

            JwtSecurityToken token = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                expires: DateTime.UtcNow.AddMinutes(_settings.ExpirationMinutes),
                claims: claims,
                signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256));

            return new SecurityTokenResponse
            {
                AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
                ExpiresIn = (long)token.Payload["exp"]!,
                RefreshToken = GenerateRefreshTokenValue(),
                RefreshTokenExpiration = DateTime.UtcNow.AddDays(_settings.RefreshTokenExpirationDays),
            };
        }

        private static string GenerateRefreshTokenValue()
        {
            return Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        }
    }
}
