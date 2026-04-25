using AutoProcess.Persistence;
using AutoProcess.Persistence.Interceptors;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace AutoProcess.Infrastructure.Extensions
{
    public static class InfrastructureExtensions
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddScoped<AuditInterceptor>();
            services.AddScoped<DbContextSeeder>();

            services.AddDbContext<AppDbContext>((sp, options) =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.RegisterRepositories();

            return services;
        }
    }
}
