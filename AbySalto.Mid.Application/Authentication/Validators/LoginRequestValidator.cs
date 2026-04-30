using AbySalto.Mid.Application.Authentication.Models;
using FluentValidation;

namespace AbySalto.Mid.Application.Authentication.Validators
{
    public class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator()
        {
            RuleFor(r => r.Username).NotEmpty();
            RuleFor(r => r.Password).NotEmpty();
        }
    }
}
