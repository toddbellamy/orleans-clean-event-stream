using Orleans.EventSourcing;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Reflection;

namespace EventSourcingInfrastructure
{
    public class EventSourcedGrain<TDomainState, TEventBase>
        : JournaledGrain<TDomainState, TEventBase>
        , IEventSourcedGrain<TDomainState, TEventBase>
        where TDomainState : class, new()
        where TEventBase : class
    {
        public Task<TDomainState> GetManagedState()
            => Task.FromResult(State as TDomainState);

        public new Task RaiseEvent<TEvent>(TEvent @event)
            where TEvent : TEventBase
        {
            if (CallDomainEventMethod(@event))
            {
                base.RaiseEvent(@event);
            }

            return Task.CompletedTask;
        }

        public new Task RaiseEvents<TEvent>(IEnumerable<TEvent> events)
            where TEvent : TEventBase
        {
            base.RaiseEvents(events);
            return Task.CompletedTask;
        }

        public new Task<bool> RaiseConditionalEvent<TEvent>(TEvent @event)
            where TEvent : TEventBase
            => base.RaiseConditionalEvent(@event);

        public new Task<bool> RaiseConditionalEvents<TEvent>(IEnumerable<TEvent> events)
            where TEvent : TEventBase
            => base.RaiseConditionalEvents(events);

        public new async Task ConfirmEvents()
            => await base.ConfirmEvents();

        private bool CallDomainEventMethod<TEvent>(TEvent @event)
        {
            var domainType = typeof(TDomainState);
            var eventType = typeof(TEvent);
            var eventMethod = domainType.GetMethod(eventType.Name, BindingFlags.Public | BindingFlags.Instance);
            
            if (eventMethod != null)
            {
                return (bool)eventMethod.Invoke(this.State, new object[] { @event });
            }

            return true;
        }
    }
}
