using AbySalto.Mid.Application.Baskets.Models;
using FluentValidation;

namespace AbySalto.Mid.Application.Baskets.Validators
{
    public class UpdateBasketItemRequestValidator : AbstractValidator<UpdateBasketItemRequest>
    {
        public UpdateBasketItemRequestValidator()
        {
            RuleFor(r => r.Quantity).GreaterThanOrEqualTo(1);
        }
    }
}
