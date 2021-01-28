using DomainBase;

namespace DomainModel.DomainEvents
{
    public class RemoveAccount : DomainEventBase
    {
        public string AccountNumber;
    }
}
