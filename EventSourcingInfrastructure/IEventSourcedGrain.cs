using System.Collections.Generic;
using System.Threading.Tasks;

namespace Orleans.EventSourcing
{
    public interface IEventSourcedGrain<TDomainState, TEventBase>
        where TDomainState : class
        where TEventBase : class
    {
        Task<TDomainState> GetManagedState();
        Task RaiseEvent<TEvent>(TEvent @event) where TEvent : TEventBase;
        Task RaiseEvents<TEvent>(IEnumerable<TEvent> events) where TEvent : TEventBase;
        Task<bool> RaiseConditionalEvent<TEvent>(TEvent @event) where TEvent : TEventBase;
        Task<bool> RaiseConditionalEvents<TEvent>(IEnumerable<TEvent> events) where TEvent : TEventBase;
        Task ConfirmEvents();
    }
}
