using AutoProcess.Application.Common.Interfaces.Repositories;
using AutoProcess.Application.Common.Interfaces.Repositories.Base;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace AutoProcess.Infrastructure.Extensions
{
    public static class RepositoryExtensions
    {
        public static IServiceCollection RegisterRepositories(this IServiceCollection services)
        {
            // ✅ Scan Infrastructure assembly (chứa implementations)
            var infrastructureAssembly = Assembly.GetExecutingAssembly();

            // ✅ Scan Application assembly (chứa interfaces)
            var applicationAssembly = typeof(IAudioRepository).Assembly;

            var baseInterfaces = new[]
            {
                typeof(IRepository<,>).Name,
                typeof(IGetAllRepository<,>).Name,
            };

            // Tìm interfaces từ Application assembly
            var repositoryInterfaces = applicationAssembly.DefinedTypes
                .Where(x => x.IsInterface
                    && x.Name.EndsWith("Repository")
                    && baseInterfaces.All(e => e != x.Name));

            foreach (var repoInterface in repositoryInterfaces)
            {
                // Tìm implementation từ Infrastructure assembly
                var implementation = infrastructureAssembly.GetTypes()
                    .FirstOrDefault(x => x is { IsInterface: false, IsAbstract: false }
                        && x.IsAssignableTo(repoInterface));

                if (implementation is not null)
                    services.AddScoped(repoInterface, implementation);
            }

            return services;
        }
    }
}
