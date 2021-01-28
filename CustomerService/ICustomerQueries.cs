using DomainModel;
using Orleans;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CustomerService
{
    public interface ICustomerQueries 
        : IGrainWithIntegerKey
    {
        Task<APIResult<Customer>> FindCustomer(Guid id);
        Task<APIResult<List<Guid>>> FindAllCustomerIds();
        Task<APIResult<bool>> CustomerExists(Guid id);
    }
}
