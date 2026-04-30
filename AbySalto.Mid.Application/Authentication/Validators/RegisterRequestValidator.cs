using AbySalto.Mid.Application.Authentication.Models;
using FluentValidation;

namespace AbySalto.Mid.Application.Authentication.Validators
{
    public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
    {
        public RegisterRequestValidator()
        {
            RuleFor(r => r.Username).NotEmpty().MaximumLength(40);
            RuleFor(r => r.Email).NotEmpty().EmailAddress().MaximumLength(255);
            RuleFor(r => r.Password).NotEmpty().MinimumLength(8);
            RuleFor(r => r.FirstName).NotEmpty().MaximumLength(40);
            RuleFor(r => r.LastName).NotEmpty().MaximumLength(40);
        }
    }
}
