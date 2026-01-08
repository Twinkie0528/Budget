using Budget.Core.Application.Commands;
using Budget.Core.Application.Dtos;
using Budget.Core.Application.Queries;
using Budget.Core.Domain.Entities;
using Budget.Core.Interfaces;
using MediatR;

namespace Budget.Core.Application.Handlers;

public class GetFieldSchemasQueryHandler : IRequestHandler<GetFieldSchemasQuery, IReadOnlyList<FieldSchemaDto>>
{
    private readonly IFieldSchemaRepository _repository;

    public GetFieldSchemasQueryHandler(IFieldSchemaRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<FieldSchemaDto>> Handle(
        GetFieldSchemasQuery request,
        CancellationToken cancellationToken)
    {
        var schemas = await _repository.GetActiveAsync(request.AppliesTo, cancellationToken);

        return schemas.Select(s => new FieldSchemaDto(
            s.Id,
            s.FieldKey,
            s.Label,
            s.FieldType,
            s.IsRequired,
            s.AppliesTo,
            s.EnumKey,
            s.SortOrder,
            s.Description,
            s.DefaultValue,
            s.ValidationPattern,
            s.MaxLength,
            s.IsActive
        )).ToList();
    }
}

public class UpdateFieldSchemaCommandHandler : IRequestHandler<UpdateFieldSchemaCommand, IReadOnlyList<FieldSchemaDto>>
{
    private readonly IFieldSchemaRepository _repository;
    private readonly ICurrentUser _currentUser;

    public UpdateFieldSchemaCommandHandler(IFieldSchemaRepository repository, ICurrentUser currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<FieldSchemaDto>> Handle(
        UpdateFieldSchemaCommand request,
        CancellationToken cancellationToken)
    {
        var results = new List<FieldSchemaDto>();

        foreach (var dto in request.Schemas)
        {
            FieldSchema schema;

            if (dto.Id.HasValue)
            {
                schema = await _repository.GetByIdAsync(dto.Id.Value, cancellationToken)
                    ?? throw new InvalidOperationException($"Field schema {dto.Id} not found");

                schema.FieldKey = dto.FieldKey;
                schema.Label = dto.Label;
                schema.FieldType = dto.FieldType;
                schema.IsRequired = dto.IsRequired;
                schema.AppliesTo = dto.AppliesTo;
                schema.EnumKey = dto.EnumKey;
                schema.SortOrder = dto.SortOrder;
                schema.Description = dto.Description;
                schema.DefaultValue = dto.DefaultValue;
                schema.ValidationPattern = dto.ValidationPattern;
                schema.MaxLength = dto.MaxLength;
                schema.IsActive = dto.IsActive;
                schema.UpdatedAt = DateTime.UtcNow;
                schema.UpdatedBy = _currentUser.UserId;

                await _repository.UpdateAsync(schema, cancellationToken);
            }
            else
            {
                schema = new FieldSchema
                {
                    FieldKey = dto.FieldKey,
                    Label = dto.Label,
                    FieldType = dto.FieldType,
                    IsRequired = dto.IsRequired,
                    AppliesTo = dto.AppliesTo,
                    EnumKey = dto.EnumKey,
                    SortOrder = dto.SortOrder,
                    Description = dto.Description,
                    DefaultValue = dto.DefaultValue,
                    ValidationPattern = dto.ValidationPattern,
                    MaxLength = dto.MaxLength,
                    IsActive = dto.IsActive,
                    CreatedBy = _currentUser.UserId
                };

                await _repository.AddAsync(schema, cancellationToken);
            }

            results.Add(new FieldSchemaDto(
                schema.Id,
                schema.FieldKey,
                schema.Label,
                schema.FieldType,
                schema.IsRequired,
                schema.AppliesTo,
                schema.EnumKey,
                schema.SortOrder,
                schema.Description,
                schema.DefaultValue,
                schema.ValidationPattern,
                schema.MaxLength,
                schema.IsActive
            ));
        }

        return results;
    }
}

