using DomainModel;
using Orleans;
using System;
using System.Threading.Tasks;

namespace CustomerService
{
    public interface ICustomerCommands 
        : IGrainWithIntegerKey
    {
        Task<APIResult<Customer>> NewCustomer(Guid id, Person primaryAccountHolder, Address mailingAddress);
        Task<APIResult<Customer>> UpdateSpouse(Guid id, Person spouse);
        Task<APIResult<Customer>> AddAccount(Guid id, Account account);
        Task<APIResult<Customer>> RemoveAccount(Guid id, string accountNumber);
        Task<APIResult<Customer>> UpdateMailingAddress(Guid id, Address address);
        Task<APIResult<Customer>> UpdatePrimaryResidence(Guid id, Address address);
        Task<APIResult<Customer>> UpdateSpouseResidence(Guid id, Address address);
        Task<APIResult<Customer>> PostAccountTransaction(Guid id, string accountNumber, decimal amount);
    }
}
