using Budget.Core.Application.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Budget.Core;

public static class DependencyInjection
{
    public static IServiceCollection AddCore(this IServiceCollection services)
    {
        // MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
        });

        // FluentValidation
        services.AddValidatorsFromAssemblyContaining<UploadFileCommandValidator>();

        return services;
    }
}

