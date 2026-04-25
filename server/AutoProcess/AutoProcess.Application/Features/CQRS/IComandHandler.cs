using System;
using System.Collections.Generic;
using System.Text;

namespace AutoProcess.Application.Features.CQRS;

public interface IComandHandler<in TCommand> where TCommand : IComand
{
    Task HandleAsync(TCommand command, CancellationToken cancellationToken);
}

public interface IComandHandler<in TCommand, TResult> where TCommand : IComand
{
    Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken);
}

