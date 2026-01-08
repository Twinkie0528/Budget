using Budget.Core.Application.Commands;
using Budget.Core.Application.Dtos;
using Budget.Core.Application.Queries;
using Budget.Core.Domain.Entities;
using Budget.Core.Interfaces;
using MediatR;

namespace Budget.Core.Application.Handlers;

public class GetDimensionQueryHandler : IRequestHandler<GetDimensionQuery, DimensionListDto>
{
    private readonly IDimensionValueRepository _repository;

    public GetDimensionQueryHandler(IDimensionValueRepository repository)
    {
        _repository = repository;
    }

    public async Task<DimensionListDto> Handle(GetDimensionQuery request, CancellationToken cancellationToken)
    {
        var values = await _repository.GetByEnumKeyAsync(request.EnumKey, request.ActiveOnly, cancellationToken);

        var dtos = values.Select(v => new DimensionValueDto(
            v.Id,
            v.EnumKey,
            v.Code,
            v.Name,
            v.Description,
            v.SortOrder,
            v.IsActive,
            v.ParentId
        )).ToList();

        return new DimensionListDto(request.EnumKey, dtos);
    }
}

public class GetDimensionKeysQueryHandler : IRequestHandler<GetDimensionKeysQuery, IReadOnlyList<string>>
{
    private readonly IDimensionValueRepository _repository;

    public GetDimensionKeysQueryHandler(IDimensionValueRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<string>> Handle(GetDimensionKeysQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetEnumKeysAsync(cancellationToken);
    }
}

public class UpdateDimensionCommandHandler : IRequestHandler<UpdateDimensionCommand, DimensionListDto>
{
    private readonly IDimensionValueRepository _repository;
    private readonly ICurrentUser _currentUser;

    public UpdateDimensionCommandHandler(IDimensionValueRepository repository, ICurrentUser currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<DimensionListDto> Handle(UpdateDimensionCommand request, CancellationToken cancellationToken)
    {
        var results = new List<DimensionValueDto>();

        foreach (var dto in request.Values)
        {
            DimensionValue value;

            if (dto.Id.HasValue)
            {
                value = await _repository.GetByIdAsync(dto.Id.Value, cancellationToken)
                    ?? throw new InvalidOperationException($"Dimension value {dto.Id} not found");

                value.Code = dto.Code;
                value.Name = dto.Name;
                value.Description = dto.Description;
                value.SortOrder = dto.SortOrder;
                value.IsActive = dto.IsActive;
                value.ParentId = dto.ParentId;
                value.UpdatedAt = DateTime.UtcNow;
                value.UpdatedBy = _currentUser.UserId;

                await _repository.UpdateAsync(value, cancellationToken);
            }
            else
            {
                value = new DimensionValue
                {
                    EnumKey = request.EnumKey,
                    Code = dto.Code,
                    Name = dto.Name,
                    Description = dto.Description,
                    SortOrder = dto.SortOrder,
                    IsActive = dto.IsActive,
                    ParentId = dto.ParentId,
                    CreatedBy = _currentUser.UserId
                };

                await _repository.AddAsync(value, cancellationToken);
            }

            results.Add(new DimensionValueDto(
                value.Id,
                value.EnumKey,
                value.Code,
                value.Name,
                value.Description,
                value.SortOrder,
                value.IsActive,
                value.ParentId
            ));
        }

        return new DimensionListDto(request.EnumKey, results);
    }
}

