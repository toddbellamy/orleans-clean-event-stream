using System;

namespace DomainBase
{
    public abstract class AggregateRoot : Entity
    {
        private Guid id;
        public Guid Id
        {
            get { return id; }
            set { id = value; }
        }

        protected AggregateRoot() { }

        public AggregateRoot(Guid id)
            : base()
        {
            Id = id;
        }

    }
}
