using FluentValidation;

namespace social_media9.Api
{
    public class UpdateUserProfileCommandValidator : AbstractValidator<UpdateUserProfileCommand>
    {
        public UpdateUserProfileCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required.");

            RuleFor(x => x.Bio)
                .MaximumLength(200).WithMessage("Bio cannot exceed 200 characters.");

            RuleFor(x => x.ProfilePictureUrl)
                .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _)).When(x => !string.IsNullOrEmpty(x.ProfilePictureUrl))
                .WithMessage("Profile picture URL must be a valid URI.");
        }
    }
}