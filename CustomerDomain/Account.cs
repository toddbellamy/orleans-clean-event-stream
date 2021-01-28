using System;

namespace DomainModel
{
    public class Account
    {
        
        public bool IsPrimaryAccount { get; protected set; }

        
        public string AccountType { get; protected set; }

        
        public string AccountNumber { get; protected set; }

        decimal balance = 0.0m;
        public decimal Balance 
        { 
            get { return balance;  }
            set
            {
                if (value < 0) throw new InvalidOperationException("Account Balance may not be negative.");

                balance = value;
            }
        }

        public Account(string accountNumber, bool isPrimaryAccount)
        {
            if (string.IsNullOrWhiteSpace(accountNumber) || accountNumber.Length < 6)
                throw new InvalidOperationException("Account Number must be at least 6 digits.");

            AccountNumber = accountNumber;
            IsPrimaryAccount = isPrimaryAccount;
        }
    }
}
