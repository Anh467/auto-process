using Microsoft.Extensions.DependencyInjection;

namespace AutoProcess.Application.Features.CQRS
{
    public static class CqrsExtensions
    {
        public static IServiceCollection RegisterCommandQueryHandlers(this IServiceCollection services)
        {
            services.Scan(scan => scan
                .FromAssemblyOf<IComand>()
                    .AddClasses(classes => classes.AssignableTo(typeof(IQueryHandler<,>)))
                        .AsImplementedInterfaces()
                        .WithScopedLifetime()
                    .AddClasses(classes => classes.AssignableTo(typeof(IComandHandler<>)))
                        .AsImplementedInterfaces()
                        .WithScopedLifetime()
                    .AddClasses(classes => classes.AssignableTo(typeof(IComandHandler<,>)))
                        .AsImplementedInterfaces()
                        .WithScopedLifetime()
            );

            return services;
        }
    }
}
