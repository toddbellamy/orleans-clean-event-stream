using DomainBase;

namespace DomainModel.DomainEvents
{
    public class PostTransaction : DomainEventBase
    {
        public string AccountNumber { get; set; } = "";
        public decimal Amount { get; set; } = 0.0m;
    }
}
