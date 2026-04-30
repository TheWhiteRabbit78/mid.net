using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AbySalto.Mid.Domain.Identity
{
    /// <summary>
    /// Represents a refresh token issued to a user, used to obtain a new access token
    /// without re-authenticating with credentials.
    /// </summary>
    public class RefreshToken
    {
        public int Id { get; set; }

        /// <summary>
        /// Opaque refresh token value handed back to the client.
        /// </summary>
        [StringLength(50)]
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// UTC timestamp after which this refresh token can no longer be used to mint a new access token.
        /// </summary>
        public DateTime Expiration { get; set; }

        /// <summary>
        /// UTC timestamp after which the user must re-authenticate with credentials,
        /// regardless of refresh token validity.
        /// </summary>
        public DateTime LoginExpiration { get; set; }

        /// <summary>
        /// Navigation property for the related <see cref="ApplicationUser"/>.
        /// </summary>
        public virtual ApplicationUser? ApplicationUserFK { get; set; }
    }

    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.HasKey(rt => rt.Id);
        }
    }
}
