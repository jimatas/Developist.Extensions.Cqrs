using Developist.Core.Cqrs;
using Developist.Core.Cqrs.Commands;
using Developist.Core.Cqrs.Infrastructure.DependencyInjection;
using Developist.Core.Cqrs.Queries;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Developist.Extensions.Cqrs.Infrastructure.DependencyInjection
{
    public static partial class CqrsBuilderExtensions
    {
        /// <summary>
        /// Add a composite dispatcher that can both statically and dynamically dispatch messages.
        /// </summary>
        /// <remarks>
        /// Note: Calls both <see cref="Core.Cqrs.Infrastructure.DependencyInjection.CqrsBuilderExtensions.AddDispatcher(CqrsBuilder, ServiceLifetime)"/> and 
        /// <see cref="Core.Cqrs.Infrastructure.DependencyInjection.CqrsBuilderExtensions.AddDynamicDispatcher(CqrsBuilder, ServiceLifetime)"/> under the hood, so you do no need to call them separately.
        /// </remarks>
        /// <param name="builder"></param>
        /// <param name="lifetime"></param>
        /// <returns></returns>
        public static CqrsBuilder AddCompositeDispatcher(this CqrsBuilder builder, ServiceLifetime lifetime = ServiceLifetime.Scoped)
        {
            builder.AddDispatcher(lifetime);
            builder.AddDynamicDispatcher(lifetime);

            var service = new ServiceDescriptor(typeof(ICompositeDispatcher), typeof(CompositeDispatcher), lifetime);
            builder.Services.TryAdd(service);

            return builder;
        }

        /// <summary>
        /// Add a command interceptor that is implemented as a generic function delegate.
        /// </summary>
        /// <remarks>
        /// Note: The service lifetime of the interceptor will be singleton using this overload.
        /// </remarks>
        /// <typeparam name="TCommand"></typeparam>
        /// <param name="builder"></param>
        /// <param name="interceptAsync"></param>
        /// <returns></returns>
        public static CqrsBuilder AddCommandInterceptor<TCommand>(this CqrsBuilder builder, Func<TCommand, HandlerDelegate, CancellationToken, Task> interceptAsync)
            where TCommand : ICommand
        {
            var service = new ServiceDescriptor(typeof(ICommandInterceptor<TCommand>), new DelegatingCommandInterceptor<TCommand>(interceptAsync));
            builder.Services.Add(service);
            return builder;
        }

        /// <summary>
        /// Add a command interceptor that is implemented as a generic function delegate. 
        /// This version of the delegate supports dependency resolution by accepting a service provider as one of the function arguments.
        /// </summary>
        /// <typeparam name="TCommand"></typeparam>
        /// <param name="builder"></param>
        /// <param name="interceptAsync"></param>
        /// <param name="lifetime"></param>
        /// <returns></returns>
        public static CqrsBuilder AddCommandInterceptor<TCommand>(this CqrsBuilder builder, Func<TCommand, HandlerDelegate, IServiceProvider, CancellationToken, Task> interceptAsync, ServiceLifetime lifetime = ServiceLifetime.Scoped)
            where TCommand : ICommand
        {
            var service = new ServiceDescriptor(typeof(ICommandInterceptor<TCommand>), provider => new DelegatingCommandInterceptor<TCommand>(interceptAsync, provider), lifetime);
            builder.Services.Add(service);
            return builder;
        }

        /// <summary>
        /// Add a query interceptor that is implemented as a generic function delegate.
        /// </summary>
        /// <remarks>
        /// Note: The service lifetime of the interceptor will be singleton using this overload.
        /// </remarks>
        /// <typeparam name="TQuery"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="builder"></param>
        /// <param name="interceptAsync"></param>
        /// <returns></returns>
        public static CqrsBuilder AddQueryInterceptor<TQuery, TResult>(this CqrsBuilder builder, Func<TQuery, HandlerDelegate<TResult>, CancellationToken, Task<TResult>> interceptAsync)
            where TQuery : IQuery<TResult>
        {
            var service = new ServiceDescriptor(typeof(IQueryInterceptor<TQuery, TResult>), new DelegatingQueryInterceptor<TQuery, TResult>(interceptAsync));
            builder.Services.Add(service);
            return builder;
        }

        /// <summary>
        /// Add a query interceptor that is implemented as a generic function delegate.
        /// This version of the delegate supports dependency resolution by accepting a service provider as one of the function arguments.
        /// </summary>
        /// <typeparam name="TQuery"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="builder"></param>
        /// <param name="interceptAsync"></param>
        /// <param name="lifetime"></param>
        /// <returns></returns>
        public static CqrsBuilder AddQueryInterceptor<TQuery, TResult>(this CqrsBuilder builder, Func<TQuery, HandlerDelegate<TResult>, IServiceProvider, CancellationToken, Task<TResult>> interceptAsync, ServiceLifetime lifetime = ServiceLifetime.Scoped)
            where TQuery : IQuery<TResult>
        {
            var service = new ServiceDescriptor(typeof(IQueryInterceptor<TQuery, TResult>), provider => new DelegatingQueryInterceptor<TQuery, TResult>(interceptAsync, provider), lifetime);
            builder.Services.Add(service);
            return builder;
        }

        private class DelegatingCommandInterceptor<TCommand> : ICommandInterceptor<TCommand>
            where TCommand : ICommand
        {
            private readonly Func<TCommand, HandlerDelegate, IServiceProvider, CancellationToken, Task> interceptAsync;
            private readonly IServiceProvider? serviceProvider;

            public DelegatingCommandInterceptor(Func<TCommand, HandlerDelegate, IServiceProvider, CancellationToken, Task> interceptAsync, IServiceProvider provider)
            {
                this.interceptAsync = interceptAsync ?? throw new ArgumentNullException(nameof(interceptAsync));
                serviceProvider = provider;
            }

            public DelegatingCommandInterceptor(Func<TCommand, HandlerDelegate, CancellationToken, Task> interceptAsync)
            {
                if (interceptAsync is null)
                {
                    throw new ArgumentNullException(nameof(interceptAsync));
                }

                this.interceptAsync = (command, next, _, cancellationToken) => interceptAsync(command, next, cancellationToken);
            }

            public Task InterceptAsync(TCommand command, HandlerDelegate next, CancellationToken cancellationToken)
            {
                return interceptAsync(command, next, serviceProvider!, cancellationToken);
            }
        }

        private class DelegatingQueryInterceptor<TQuery, TResult> : IQueryInterceptor<TQuery, TResult>
            where TQuery : IQuery<TResult>
        {
            private readonly Func<TQuery, HandlerDelegate<TResult>, IServiceProvider, CancellationToken, Task<TResult>> interceptAsync;
            private readonly IServiceProvider? serviceProvider;

            public DelegatingQueryInterceptor(Func<TQuery, HandlerDelegate<TResult>, IServiceProvider, CancellationToken, Task<TResult>> interceptAsync, IServiceProvider provider)
            {
                this.interceptAsync = interceptAsync ?? throw new ArgumentNullException(nameof(interceptAsync));
                serviceProvider = provider;
            }

            public DelegatingQueryInterceptor(Func<TQuery, HandlerDelegate<TResult>, CancellationToken, Task<TResult>> interceptAsync)
            {
                if (interceptAsync is null)
                {
                    throw new ArgumentNullException(nameof(interceptAsync));
                }

                this.interceptAsync = (query, next, _, cancellationToken) => interceptAsync(query, next, cancellationToken);
            }

            public Task<TResult> InterceptAsync(TQuery query, HandlerDelegate<TResult> next, CancellationToken cancellationToken)
            {
                return interceptAsync(query, next, serviceProvider!, cancellationToken);
            }
        }
    }
}
