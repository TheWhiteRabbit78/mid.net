using AbySalto.Mid.Application.Authentication.Models;
using FluentValidation;

namespace AbySalto.Mid.Application.Authentication.Validators
{
    public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
    {
        public RefreshTokenRequestValidator()
        {
            RuleFor(r => r.RefreshToken).NotEmpty();
        }
    }
}
