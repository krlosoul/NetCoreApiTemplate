namespace Application.Features.User.Validators
{
    using Application.Features.User.Queries;
    using Core.Messages;
    using FluentValidation;

    public class AuthenticateValidator : AbstractValidator<AuthenticateQuery>
    {
        public AuthenticateValidator()
        {
            RuleFor(entity => entity.Email)
                .NotNull().WithMessage(Message.NullError)
                .NotEmpty().WithMessage(Message.EmptyError)
                .EmailAddress().WithMessage(Message.EmailError)
                .WithName("Email");

            RuleFor(entity => entity.Password)
                .NotNull().WithMessage(Message.NullError)
                .NotEmpty().WithMessage(Message.EmptyError)
                .WithName("Password");
        }
    }
}
