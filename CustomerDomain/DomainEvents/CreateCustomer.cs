using DomainBase;

namespace DomainModel.DomainEvents
{
    public class CreateCustomer : DomainEventBase
    {
        public Person PrimaryAccountHolder;
        public Address MailingAddress;
    }
}
