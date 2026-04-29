using AbySalto.Mid.Application.Authentication.Models;
using AbySalto.Mid.Domain.Identity;

namespace AbySalto.Mid.Application.Authentication.Extensions;

public static class ApplicationUserExtensions
{
    extension(ApplicationUser user)
    {
        /// <summary>
        /// Maps an <see cref="ApplicationUser"/> to a <see cref="UserDto"/> safe for client consumption.
        /// </summary>
        public UserDto MapToUserDto()
        {
            return new UserDto
            {
                Id = user.Id,
                Username = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = user.FullName,
            };
        }
    }

    extension(RegisterRequest request)
    {
        /// <summary>
        /// Maps a <see cref="RegisterRequest"/> to a new <see cref="ApplicationUser"/> instance.
        /// Password is not mapped here — it must be set via UserManager.CreateAsync.
        /// </summary>
        public ApplicationUser MapToApplicationUser()
        {
            return new ApplicationUser
            {
                UserName = request.Username,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
            };
        }
    }
}
