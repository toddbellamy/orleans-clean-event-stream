using System;

namespace EventSourcingInfrastructure
{
    public interface IEntityState
    {
        public Guid Id { get; set; }
    }
}
