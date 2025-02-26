namespace Application.Features.Role.Validators
{
    using Application.Features.Role.Commands;
    using Core.Messages;
    using FluentValidation;

    public class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
    {
        public CreateRoleCommandValidator()
        {
            RuleFor(entity => entity.Description)
                .NotNull().WithMessage(Message.NullError)
                .NotEmpty().WithMessage(Message.EmptyError)
                .WithName("Description");
        }
	}
}