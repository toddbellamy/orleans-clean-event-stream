using DomainBase;

namespace DomainModel.DomainEvents
{
    public class ChangeSpouse : DomainEventBase
    {
        public Person Spouse;
    }
}
