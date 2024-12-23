using FluentValidation;
using Web.Models.Endpoints.UrlShort;

namespace Web.Models.Validators.Endpoint;

public class ShortUrlValidator : AbstractValidator<ShortUrlRequest>
{
    public ShortUrlValidator()
    {
        RuleFor(x => x.Url)
            .Must(x => !string.IsNullOrWhiteSpace(x))
            .WithMessage("Url is required");

        RuleFor(x => x.Url)
            .Must(x => Uri.TryCreate(x, UriKind.RelativeOrAbsolute, out _))
            .WithMessage("Url is invalid");

        RuleFor(x => x.ExpireDay)
            .GreaterThan(0)
            .WithMessage("Expire day must be greater than 0");
    }
}