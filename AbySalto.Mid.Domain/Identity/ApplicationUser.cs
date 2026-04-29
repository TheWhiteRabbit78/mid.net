using System.ComponentModel.DataAnnotations;
using AbySalto.Mid.Domain.Baskets;
using AbySalto.Mid.Domain.Favorites;
using Microsoft.AspNetCore.Identity;

namespace AbySalto.Mid.Domain.Identity
{
    /// <summary>
    /// Application user that extends <see cref="IdentityUser"/> with profile fields
    /// and navigation to the user's basket and favorites.
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        public ApplicationUser()
        {
            FirstName = string.Empty;
            LastName = string.Empty;
        }

        [PersonalData, StringLength(40)]
        public string FirstName { get; set; }

        [PersonalData, StringLength(40)]
        public string LastName { get; set; }

        public string FullName => $"{FirstName} {LastName}";

        /// <summary>
        /// Navigation property for the user's basket. One basket per user.
        /// </summary>
        public virtual Basket? BasketFK { get; set; }

        /// <summary>
        /// Navigation property for the user's favorited products.
        /// </summary>
        public virtual ICollection<Favorite>? FavoritesFK { get; set; }
    }
}
