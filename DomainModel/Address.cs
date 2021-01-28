
namespace DomainModel
{
    public class Address
    {
        public string Street { get; protected set; }

        public string Street2 { get; protected set; }

        public string City { get; protected set; }
        
        public string StateOrProvince { get; protected set; }
        
        public string Country { get; protected set; }

        public string PostalCode { get; protected set; }

        public Address(string street, string city, string stateOrProvince, string postalCode, string country)
        {
            Street = street;
            City = city;
            StateOrProvince = stateOrProvince;
            PostalCode = postalCode;
            Country = country;
        }
    }
}
