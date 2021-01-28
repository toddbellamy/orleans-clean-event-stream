using DomainBase;
using DomainModel;
using Orleans;
using Orleans.EventSourcing;
using Orleans.EventSourcing.CustomStorage;

namespace CustomerService
{
    public interface ICustomerJournal 
        : IGrainWithGuidKey
        , IEventSourcedGrain<Customer, DomainEventBase>
        , ICustomStorageInterface<Customer, DomainEventBase>
    { }
}
