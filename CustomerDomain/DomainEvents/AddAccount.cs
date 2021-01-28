using DomainBase;

namespace DomainModel.DomainEvents
{
    public class AddAccount : DomainEventBase
    {
        public Account Account;
    }
}
