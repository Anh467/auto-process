using System;
using System.Collections.Generic;
using System.Text;

namespace AutoProcess.Application.Features.CQRS
{
    public interface IQueryHandler<in TQuery, TResult> where TQuery : IQuery
    {
        Task<TResult> HandleAsync(TQuery command, CancellationToken cancellationToken);
    }
}
