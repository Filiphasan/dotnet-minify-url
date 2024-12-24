using FluentValidation;
using Web.Common.Models.Endpoints.UrlShort;

namespace Web.Common.Models.Validators.Endpoint;

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
    }
}