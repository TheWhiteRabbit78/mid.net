namespace AbySalto.Mid.Application.Authentication.Models
{
    /// <summary>
    /// Public representation of a user, returned to clients.
    /// Excludes sensitive fields like password hash and security stamps.
    /// </summary>
    public class UserDto
    {
        public string Id { get; set; } = string.Empty;
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
    }
}
