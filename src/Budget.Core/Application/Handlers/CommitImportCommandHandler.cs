using System.Text.Json;
using Budget.Core.Application.Commands;
using Budget.Core.Application.Dtos;
using Budget.Core.Domain.Entities;
using Budget.Core.Interfaces;
using MediatR;

namespace Budget.Core.Application.Handlers;

public class CommitImportCommandHandler : IRequestHandler<CommitImportCommand, CommitResultDto>
{
    private readonly IImportRunRepository _importRunRepository;
    private readonly IBudgetRequestRepository _budgetRequestRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentUser _currentUser;

    public CommitImportCommandHandler(
        IImportRunRepository importRunRepository,
        IBudgetRequestRepository budgetRequestRepository,
        IAuditLogRepository auditLogRepository,
        ICurrentUser currentUser)
    {
        _importRunRepository = importRunRepository;
        _budgetRequestRepository = budgetRequestRepository;
        _auditLogRepository = auditLogRepository;
        _currentUser = currentUser;
    }

    public async Task<CommitResultDto> Handle(CommitImportCommand request, CancellationToken cancellationToken)
    {
        var importRun = await _importRunRepository.GetByIdAsync(request.ImportRunId, cancellationToken)
            ?? throw new InvalidOperationException($"Import run {request.ImportRunId} not found");

        if (importRun.Status != ImportStatus.Parsed)
        {
            throw new InvalidOperationException($"Import run is in invalid state: {importRun.Status}");
        }

        if (importRun.ErrorCount > 0)
        {
            throw new InvalidOperationException("Cannot commit import with validation errors");
        }

        importRun.Status = ImportStatus.Committing;
        await _importRunRepository.UpdateAsync(importRun, cancellationToken);

        try
        {
            // Deserialize parsed data
            var header = !string.IsNullOrEmpty(importRun.ParsedHeaderJson)
                ? JsonSerializer.Deserialize<ParsedHeaderDto>(importRun.ParsedHeaderJson)
                : null;

            var items = !string.IsNullOrEmpty(importRun.ParsedItemsJson)
                ? JsonSerializer.Deserialize<List<ParsedItemDto>>(importRun.ParsedItemsJson) ?? new List<ParsedItemDto>()
                : new List<ParsedItemDto>();

            // Generate request number
            var requestNumber = GenerateRequestNumber();

            // Create budget request
            var budgetRequest = new BudgetRequest
            {
                RequestNumber = header?.RequestNumber ?? requestNumber,
                Title = header?.Title ?? $"Import {importRun.FileName}",
                Description = header?.Description,
                Channel = header?.Channel,
                Owner = header?.Owner,
                Frequency = header?.Frequency,
                Vendor = header?.Vendor,
                TotalAmount = header?.TotalAmount ?? items.Sum(i => i.Amount ?? 0),
                Currency = header?.Currency ?? "USD",
                FiscalYear = header?.FiscalYear ?? DateTime.UtcNow.Year,
                FiscalQuarter = header?.FiscalQuarter,
                ExtrasJson = header?.Extras != null ? JsonSerializer.Serialize(header.Extras) : null,
                ImportRunId = importRun.Id,
                CreatedBy = _currentUser.UserId,
                Status = BudgetRequestStatus.Draft
            };

            // Create budget items
            foreach (var item in items)
            {
                budgetRequest.Items.Add(new BudgetItem
                {
                    BudgetRequestId = budgetRequest.Id,
                    RowNumber = item.RowNumber,
                    LineDescription = item.LineDescription,
                    Category = item.Category,
                    SubCategory = item.SubCategory,
                    Amount = item.Amount ?? 0,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    CostCenter = item.CostCenter,
                    AccountCode = item.AccountCode,
                    Jan = item.Jan,
                    Feb = item.Feb,
                    Mar = item.Mar,
                    Apr = item.Apr,
                    May = item.May,
                    Jun = item.Jun,
                    Jul = item.Jul,
                    Aug = item.Aug,
                    Sep = item.Sep,
                    Oct = item.Oct,
                    Nov = item.Nov,
                    Dec = item.Dec,
                    ExtrasJson = item.Extras != null ? JsonSerializer.Serialize(item.Extras) : null,
                    CreatedBy = _currentUser.UserId
                });
            }

            await _budgetRequestRepository.AddAsync(budgetRequest, cancellationToken);

            // Update import run
            importRun.BudgetRequestId = budgetRequest.Id;
            importRun.Status = ImportStatus.Committed;
            importRun.CommittedAt = DateTime.UtcNow;
            await _importRunRepository.UpdateAsync(importRun, cancellationToken);

            // Write audit log
            await _auditLogRepository.AddAsync(new AuditLog
            {
                EntityType = nameof(BudgetRequest),
                EntityId = budgetRequest.Id,
                Action = "Created",
                UserId = _currentUser.UserId,
                UserName = _currentUser.UserName,
                PayloadJson = JsonSerializer.Serialize(new
                {
                    Source = "Import",
                    ImportRunId = importRun.Id,
                    FileName = importRun.FileName,
                    ItemCount = budgetRequest.Items.Count
                })
            }, cancellationToken);

            return new CommitResultDto(
                importRun.Id,
                budgetRequest.Id,
                budgetRequest.RequestNumber,
                budgetRequest.Items.Count);
        }
        catch (Exception ex)
        {
            importRun.Status = ImportStatus.CommitFailed;
            importRun.ErrorMessage = ex.Message;
            await _importRunRepository.UpdateAsync(importRun, cancellationToken);
            throw;
        }
    }

    private static string GenerateRequestNumber()
    {
        return $"BR-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpperInvariant()}";
    }
}

