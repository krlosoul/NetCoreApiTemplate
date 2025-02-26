namespace Application.Features.User.Validators
{
    using System.Globalization;
    using Application.Features.User.Commands;
    using Core.Messages;
    using FluentValidation;

    public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
    {
		public CreateUserCommandValidator()
		{
            RuleFor(entity => entity.FirstName)
                .NotNull().WithMessage(Message.NullError)
                .NotEmpty().WithMessage(Message.EmptyError)
                .WithName("FirstName");

            RuleFor(entity => entity.FirstName)
                .NotNull().WithMessage(Message.NullError)
                .NotEmpty().WithMessage(Message.EmptyError)
                .WithName("FirstName");

            RuleFor(entity => entity.LastName)
                .NotNull().WithMessage(Message.NullError)
                .NotEmpty().WithMessage(Message.EmptyError)
                .WithName("LastName");

            RuleFor(entity => entity.Email)
                .NotNull().WithMessage(Message.NullError)
                .NotEmpty().WithMessage(Message.EmptyError)
                .EmailAddress().WithMessage(Message.EmailError)
                .WithName("Email");

            RuleFor(entity => entity.Phone)
                .NotNull().WithMessage(Message.NullError)
                .NotEmpty().WithMessage(Message.EmptyError)
                .Length(10).WithMessage(Message.UniqueLengthError)
                .WithName("Phone");

            RuleFor(entity => entity.Password)
                .NotNull().WithMessage(Message.NullError)
                .NotEmpty().WithMessage(Message.EmptyError)
                .WithName("Password");
        }
    }
}