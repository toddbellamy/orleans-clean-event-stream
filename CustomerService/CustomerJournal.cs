using DomainModel;
using Microsoft.Extensions.Logging;
using EventSourcingInfrastructure;

namespace CustomerService
{
    public class CustomerJournal
       : AggregateJournal<Customer>,
       ICustomerJournal
    {
        public CustomerJournal(ILogger<AggregateJournal<Customer>> log)
            : base(log)
        {
        }
    }
}
