using FluentValidation;

namespace social_media9.Api.Commands
{
    public class GoogleLoginCommandValidator : AbstractValidator<GoogleLoginCommand>
    {
        public GoogleLoginCommandValidator()
        {
            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Google authorization code is required.");
            RuleFor(x => x.RedirectUri)
                .NotEmpty().WithMessage("Redirect URI is required.")
                .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _)).WithMessage("Redirect URI must be a valid absolute URI.");
        }
    }
}