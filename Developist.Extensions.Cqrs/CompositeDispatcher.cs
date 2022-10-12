using Developist.Core.Cqrs;
using Developist.Core.Cqrs.Commands;
using Developist.Core.Cqrs.Events;
using Developist.Core.Cqrs.Queries;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Developist.Extensions.Cqrs
{
    public sealed class CompositeDispatcher : ICompositeDispatcher
    {
        private readonly IDispatcher dispatcher;
        private readonly IDynamicDispatcher dynamicDispatcher;

        public CompositeDispatcher(IDispatcher dispatcher, IDynamicDispatcher dynamicDispatcher)
        {
            this.dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            this.dynamicDispatcher = dynamicDispatcher ?? throw new ArgumentNullException(nameof(dynamicDispatcher));
        }

        Task ICommandDispatcher.DispatchAsync<TCommand>(TCommand command, CancellationToken cancellationToken)
        {
            return dispatcher.DispatchAsync(command, cancellationToken);
        }

        Task IEventDispatcher.DispatchAsync<TEvent>(TEvent @event, CancellationToken cancellationToken)
        {
            return dispatcher.DispatchAsync(@event, cancellationToken);
        }

        Task<TResult> IQueryDispatcher.DispatchAsync<TQuery, TResult>(TQuery query, CancellationToken cancellationToken)
        {
            return dispatcher.DispatchAsync<TQuery, TResult>(query, cancellationToken);
        }

        Task IDynamicCommandDispatcher.DispatchAsync(ICommand command, CancellationToken cancellationToken)
        {
            return dynamicDispatcher.DispatchAsync(command, cancellationToken);
        }

        Task IDynamicEventDispatcher.DispatchAsync(IEvent @event, CancellationToken cancellationToken)
        {
            return dynamicDispatcher.DispatchAsync(@event, cancellationToken);
        }

        Task<TResult> IDynamicQueryDispatcher.DispatchAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken)
        {
            return dynamicDispatcher.DispatchAsync(query, cancellationToken);
        }
    }
}
