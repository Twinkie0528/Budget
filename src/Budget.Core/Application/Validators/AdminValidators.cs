using Budget.Core.Application.Commands;
using Budget.Core.Application.Dtos;
using FluentValidation;

namespace Budget.Core.Application.Validators;

public class UpdateFieldSchemaCommandValidator : AbstractValidator<UpdateFieldSchemaCommand>
{
    public UpdateFieldSchemaCommandValidator()
    {
        RuleFor(x => x.Schemas)
            .NotEmpty().WithMessage("At least one schema is required");

        RuleForEach(x => x.Schemas).SetValidator(new FieldSchemaUpdateDtoValidator());
    }
}

public class FieldSchemaUpdateDtoValidator : AbstractValidator<FieldSchemaUpdateDto>
{
    public FieldSchemaUpdateDtoValidator()
    {
        RuleFor(x => x.FieldKey)
            .NotEmpty().WithMessage("Field key is required")
            .MaximumLength(100).WithMessage("Field key must be 100 characters or less")
            .Matches(@"^[a-zA-Z_][a-zA-Z0-9_]*$").WithMessage("Field key must be a valid identifier");

        RuleFor(x => x.Label)
            .NotEmpty().WithMessage("Label is required")
            .MaximumLength(200).WithMessage("Label must be 200 characters or less");

        RuleFor(x => x.EnumKey)
            .NotEmpty().When(x => x.FieldType == Domain.Entities.FieldType.Dropdown || x.FieldType == Domain.Entities.FieldType.MultiSelect)
            .WithMessage("Enum key is required for dropdown fields");
    }
}

public class UpdateDimensionCommandValidator : AbstractValidator<UpdateDimensionCommand>
{
    public UpdateDimensionCommandValidator()
    {
        RuleFor(x => x.EnumKey)
            .NotEmpty().WithMessage("Enum key is required")
            .MaximumLength(50).WithMessage("Enum key must be 50 characters or less");

        RuleFor(x => x.Values)
            .NotEmpty().WithMessage("At least one value is required");

        RuleForEach(x => x.Values).SetValidator(new DimensionValueUpdateDtoValidator());
    }
}

public class DimensionValueUpdateDtoValidator : AbstractValidator<DimensionValueUpdateDto>
{
    public DimensionValueUpdateDtoValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required")
            .MaximumLength(50).WithMessage("Code must be 50 characters or less");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name must be 200 characters or less");
    }
}

