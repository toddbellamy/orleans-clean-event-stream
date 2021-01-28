using DomainBase;

namespace DomainModel.DomainEvents
{
    public class ChangeMailingAddress : DomainEventBase
    {
        public Address Address;
    }
}
