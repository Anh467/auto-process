using AutoProcess.Application.Features.CQRS;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutoProcess.Application.Common.Extensions
{
    public static class ApplicationExtensions
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.RegisterCommandQueryHandlers();
            return services;
        }
    }
}
