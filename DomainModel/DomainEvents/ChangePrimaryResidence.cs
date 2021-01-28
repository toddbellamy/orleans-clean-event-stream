using DomainBase;

namespace DomainModel.DomainEvents
{
    public class ChangePrimaryResidence : DomainEventBase
    {
        public Address Address;
    }
}
