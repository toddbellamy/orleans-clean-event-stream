using System;
using System.Collections.Generic;
using DomainBase;
using DomainModel;

namespace DomainModel
{
    public class Customer : AggregateRoot
    {
        public Person PrimaryAccountHolder { get; protected set; }

        public Person Spouse { get; protected set; }

        public Address MailingAddress { get; protected set; }

        public List<Account> Accounts { get; protected set; } = new List<Account>();

        public new Guid Id
        {
            get { return base.Id; }
            set { base.Id = value; }
        }

        public bool CreateCustomer(DomainEvents.CreateCustomer @event)
        {
            if (@event.MailingAddress == null)
            {
                throw new DomainValidationException("Mailing Address not provided to Create Customer.");
            }
            else if (@event.PrimaryAccountHolder == null)
            {
                throw new DomainValidationException("Primary Account Holder not provided to Create Customer.");
            }

            return true;
        }

        public bool AddAccount(DomainEvents.AddAccount @event)
        {
            if(string.IsNullOrWhiteSpace(@event.Account.AccountNumber))
                throw new DomainValidationException("Account Number not provided to Add Account.");

            return true;
        }

        public bool ChangeMailingAddress(DomainEvents.ChangeMailingAddress @event)
        {
            if (@event.Address == null)
            {
                throw new DomainValidationException("Address not provided to Change Mailing Address.");
            }
            else if (string.IsNullOrWhiteSpace(@event.Address.Street))
            {
                throw new DomainValidationException("Street Address not provided to Change Mailing Address.");
            }

            return true;
        }

        public bool ChangePrimaryResidence(DomainEvents.ChangePrimaryResidence @event)
        {
            return true;
        }

        public bool PostTransaction(DomainEvents.PostTransaction @event)
        {
            var acct = Accounts.Find(a => a.AccountNumber.Equals(@event.AccountNumber));

            if (acct == null)
                throw new DomainValidationException($"Invalid Account {@event.AccountNumber}.");

            decimal newBalance = acct.Balance + @event.Amount;

            if (acct.Balance >= 0 && newBalance < 0)
                throw new DomainValidationException("Insufficient funds.");

            return true;
        }

        public void Apply(DomainEvents.AddAccount e)
        {
            var acct = Accounts.Find(a => a.AccountNumber.Equals(e.Account.AccountNumber));
            if (acct == null) Accounts.Add(e.Account);
        }

        public void Apply(DomainEvents.RemoveAccount e)
        {
            Accounts.RemoveAll(a => a.AccountNumber.Equals(e.AccountNumber));
        }

        public void Apply(DomainEvents.CreateCustomer e)
        {
            PrimaryAccountHolder = e.PrimaryAccountHolder;
            MailingAddress = e.MailingAddress;
        }

        public void Apply(DomainEvents.ChangeMailingAddress e)
        {
            MailingAddress = e.Address;
        }

        public void Apply(DomainEvents.ChangePrimaryResidence e)
        {
            PrimaryAccountHolder.Residence = e.Address;
        }

        public void Apply(DomainEvents.ChangeSpouseResidence e)
        {
            Spouse.Residence = e.Address;
        }

        public void Apply(DomainEvents.ChangeSpouse e)
        {
            Spouse = e.Spouse;
        }

        public void Apply(DomainEvents.RemoveSpouse e)
        {
            Spouse = null;
        }

        public void Apply(DomainEvents.PostTransaction e)
        {
            var acct = Accounts.Find(a => a.AccountNumber.Equals(e.AccountNumber));

            decimal newBalance = acct.Balance + e.Amount;

            if (acct != null) acct.Balance = newBalance;
        }
    }
}
