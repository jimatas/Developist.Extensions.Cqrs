using Developist.Core.Cqrs;

namespace Developist.Extensions.Cqrs
{
    /// <summary>
    /// Provides combined support for both static and dynamic message dispatch by deriving from the <see cref="IDispatcher"/> interface as well as the <see cref="IDynamicDispatcher"/> interface.
    /// </summary>
    public interface ICompositeDispatcher : IDispatcher, IDynamicDispatcher
    {
    }
}
