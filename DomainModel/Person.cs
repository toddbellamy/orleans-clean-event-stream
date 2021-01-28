using System;

namespace DomainModel
{
    public class Person
    {
        public string FullName { get; protected set; }

        public string FirstName { get; protected set; }

        public string LastName { get; protected set; }

        public Address Residence { get; set; }

        public string TaxId { get; protected set; }

        public DateTimeOffset DateOfBirth { get; protected set; }

        public Person(string fullName, string firstName, string lastName, Address residence, string taxId, DateTimeOffset dateOfBirth)
        {
            if (string.IsNullOrWhiteSpace(lastName) || string.IsNullOrWhiteSpace(firstName))
                throw new InvalidOperationException("First and Last Names must be provided for Person.");

            if (dateOfBirth == null || dateOfBirth > DateTimeOffset.Now.AddHours(12))
                throw new InvalidOperationException("Person DateOfBirth must be valid date in the past.");

            FullName = fullName;
            FirstName = firstName;
            LastName = lastName;
            Residence = residence;
            TaxId = taxId;
            DateOfBirth = dateOfBirth;
        }
    }
}
