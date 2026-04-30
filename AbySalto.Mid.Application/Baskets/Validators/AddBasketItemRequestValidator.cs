using AbySalto.Mid.Application.Baskets.Models;
using FluentValidation;

namespace AbySalto.Mid.Application.Baskets.Validators
{
    public class AddBasketItemRequestValidator : AbstractValidator<AddBasketItemRequest>
    {
        public AddBasketItemRequestValidator()
        {
            RuleFor(r => r.ProductId).GreaterThan(0);
            RuleFor(r => r.Quantity).GreaterThanOrEqualTo(1);
        }
    }
}
