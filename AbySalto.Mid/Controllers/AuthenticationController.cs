using System.Security.Claims;
using AbySalto.Mid.Application.Authentication.Extensions;
using AbySalto.Mid.Application.Authentication.Models;
using AbySalto.Mid.Application.Authentication.Services;
using AbySalto.Mid.Application.Common.Persistence;
using AbySalto.Mid.Domain.Identity;
using AbySalto.Mid.Infrastructure.Authentication;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AbySalto.Mid.WebApi.Controllers
{
    /// <summary>
    /// Controller responsible for user registration, authentication, and token refresh.
    /// </summary>
    [ApiController]
    [Route("v1/authentication")]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IApplicationDbContext _dbContext;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly JwtSettings _jwtSettings;
        private readonly IValidator<RegisterRequest> _registerValidator;
        private readonly IValidator<LoginRequest> _loginValidator;
        private readonly IValidator<RefreshTokenRequest> _refreshValidator;
        private readonly ILogger<AuthenticationController> _logger;

        public AuthenticationController(
            UserManager<ApplicationUser> userManager,
            IApplicationDbContext dbContext,
            IJwtTokenService jwtTokenService,
            IOptions<JwtSettings> jwtSettings,
            IValidator<RegisterRequest> registerValidator,
            IValidator<LoginRequest> loginValidator,
            IValidator<RefreshTokenRequest> refreshValidator,
            ILogger<AuthenticationController> logger)
        {
            _userManager = userManager;
            _dbContext = dbContext;
            _jwtTokenService = jwtTokenService;
            _jwtSettings = jwtSettings.Value;
            _registerValidator = registerValidator;
            _loginValidator = loginValidator;
            _refreshValidator = refreshValidator;
            _logger = logger;
        }

        #region Register

        /// <summary>
        /// Registers a new user account.
        /// </summary>
        /// <param name="request">The registration details.</param>
        /// <returns>The created user.</returns>
        /// <response code="201">User registered successfully.</response>
        /// <response code="400">Validation or registration failure.</response>
        [HttpPost]
        [Route("users")]
        [Produces("application/json")]
        [ProducesResponseType<UserDto>(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            FluentValidation.Results.ValidationResult validation = await _registerValidator.ValidateAsync(request);
            if (!validation.IsValid)
            {
                return Problem(statusCode: StatusCodes.Status400BadRequest, title: validation.Errors.First().ErrorMessage);
            }

            ApplicationUser user = request.MapToApplicationUser();
            IdentityResult result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                string firstError = result.Errors.FirstOrDefault()?.Description ?? "Registration failed";
                return Problem(statusCode: StatusCodes.Status400BadRequest, title: firstError);
            }

            return CreatedAtAction(nameof(Register), user.MapToUserDto());
        }

        #endregion Register

        #region Login

        /// <summary>
        /// Authenticates a user with username and password and issues an access + refresh token pair.
        /// </summary>
        /// <param name="request">The login credentials.</param>
        /// <returns>An access token, refresh token, and expirations.</returns>
        /// <response code="201">Token created successfully.</response>
        /// <response code="400">Validation failure.</response>
        /// <response code="401">Invalid username or password.</response>
        /// <response code="500">Failed to update user state.</response>
        [HttpPost]
        [Route("users/tokens")]
        [Produces("application/json")]
        [ProducesResponseType<SecurityTokenResponse>(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateToken([FromBody] LoginRequest request)
        {
            FluentValidation.Results.ValidationResult validation = await _loginValidator.ValidateAsync(request);
            if (!validation.IsValid)
            {
                return Problem(statusCode: StatusCodes.Status400BadRequest, title: validation.Errors.First().ErrorMessage);
            }

            ApplicationUser? user = await _userManager.FindByNameAsync(request.Username);
            if (user == null || user.PasswordHash == null)
            {
                return Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Invalid username or password");
            }

            PasswordVerificationResult verificationResult = _userManager.PasswordHasher
                .VerifyHashedPassword(user, user.PasswordHash, request.Password);

            if (verificationResult != PasswordVerificationResult.Success)
            {
                return Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Invalid username or password");
            }

            IEnumerable<Claim> claims = GetClaims(user);
            SecurityTokenResponse tokenResponse = _jwtTokenService.GenerateToken(claims);

            user.RefreshTokenFK ??= new RefreshToken();
            user.RefreshTokenFK.Value = tokenResponse.RefreshToken;
            user.RefreshTokenFK.Expiration = tokenResponse.RefreshTokenExpiration;
            user.RefreshTokenFK.LoginExpiration = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenLoginExpirationDays);

            try
            {
                await _userManager.UpdateAsync(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to persist refresh token for user {user}", user.UserName);
                return Problem(statusCode: StatusCodes.Status500InternalServerError, title: "Failed to update user");
            }

            return CreatedAtAction(nameof(CreateToken), tokenResponse);
        }

        #endregion Login

        #region Refresh

        /// <summary>
        /// Issues a new access token using a valid refresh token.
        /// </summary>
        /// <param name="request">The refresh token request.</param>
        /// <returns>A new access token, refresh token, and expirations.</returns>
        /// <response code="201">Token refreshed successfully.</response>
        /// <response code="400">Validation failure.</response>
        /// <response code="401">Refresh token expired or login session expired.</response>
        /// <response code="404">No user matches the supplied refresh token.</response>
        /// <response code="500">Failed to update user state.</response>
        [HttpPost]
        [Route("users/tokens/refresh")]
        [Produces("application/json")]
        [ProducesResponseType<SecurityTokenResponse>(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            FluentValidation.Results.ValidationResult validation = await _refreshValidator.ValidateAsync(request);
            if (!validation.IsValid)
            {
                return Problem(statusCode: StatusCodes.Status400BadRequest, title: validation.Errors.First().ErrorMessage);
            }

            ApplicationUser? user = await _dbContext.Users
                .Include(u => u.RefreshTokenFK)
                .Where(u => u.RefreshTokenFK != null
                         && u.RefreshTokenFK.Value == request.RefreshToken
                         && u.RefreshTokenFK.Expiration >= DateTime.UtcNow)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return Problem(statusCode: StatusCodes.Status404NotFound, title: "User not found or refresh token expired");
            }

            // Force re-authentication if the long-lived login window has expired
            if (user.RefreshTokenFK!.LoginExpiration <= DateTime.UtcNow)
            {
                return Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Refresh token expired, please re-authenticate");
            }

            IEnumerable<Claim> claims = GetClaims(user);
            SecurityTokenResponse tokenResponse = _jwtTokenService.GenerateToken(claims);

            user.RefreshTokenFK.Value = tokenResponse.RefreshToken;
            user.RefreshTokenFK.Expiration = tokenResponse.RefreshTokenExpiration;

            try
            {
                await _userManager.UpdateAsync(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to persist refreshed token for user {user}", user.UserName);
                return Problem(statusCode: StatusCodes.Status500InternalServerError, title: "Failed to update user");
            }

            return CreatedAtAction(nameof(RefreshToken), tokenResponse);
        }

        #endregion Refresh

        #region Current User

        /// <summary>
        /// Returns information about the currently authenticated user.
        /// </summary>
        /// <returns>The current user.</returns>
        /// <response code="200">Current user returned successfully.</response>
        /// <response code="401">No valid bearer token supplied.</response>
        /// <response code="404">The token references a user that no longer exists.</response>
        [HttpGet]
        [Route("users/me")]
        [Authorize]
        [Produces("application/json")]
        [ProducesResponseType<UserDto>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Me()
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Invalid token");
            }

            ApplicationUser? user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Problem(statusCode: StatusCodes.Status404NotFound, title: "User not found");
            }

            return Ok(user.MapToUserDto());
        }

        #endregion Current User

        #region Private

        private static IEnumerable<Claim> GetClaims(ApplicationUser user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
            };

            if (!string.IsNullOrEmpty(user.Email))
            {
                claims.Add(new Claim(ClaimTypes.Email, user.Email));
            }

            return claims;
        }

        #endregion Private
    }
}
