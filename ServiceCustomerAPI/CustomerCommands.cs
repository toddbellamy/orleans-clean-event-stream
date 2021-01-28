using DomainModel;
using DomainModel.DomainEvents;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Concurrency;
using System;
using System.Threading.Tasks;

namespace CustomerService
{
    [Reentrant]
    [StatelessWorker]
    public class CustomerCommands 
        : Grain, ICustomerCommands
    {
        private readonly IClusterClient OrleansClient;
        private readonly ILogger<CustomerCommands> Log;

        public CustomerCommands(IClusterClient clusterClient, ILogger<CustomerCommands> log)
        {
            OrleansClient = clusterClient;
            Log = log;
        }

        public async Task<APIResult<Customer>> AddAccount(Guid id, Account account)
        {
            try
            {
                var mgr = OrleansClient.GetGrain<ICustomerJournal>(id);
                await mgr.RaiseEvent(new AddAccount { 
                    Account = account 
                });
                await mgr.ConfirmEvents();
                return new APIResult<Customer>(await mgr.GetManagedState());
            }
            catch (Exception ex)
            {
                return new APIResult<Customer>(ex);
            }
        }

        public async Task<APIResult<Customer>> NewCustomer(Guid customerId, Person primaryAccountHolder, Address mailingAddress)
        {
            Log.LogInformation("NewCustomer: start");
            try
            {
                Log.LogInformation("NewCustomer: get manager");
                var mgr = OrleansClient.GetGrain<ICustomerJournal>(customerId);
                Log.LogInformation($"NewCustomer: manager is null? {mgr is null}");
                Log.LogInformation("NewCustomer: raising CustomerCreated event");

                await mgr.RaiseEvent(new CreateCustomer { 
                    PrimaryAccountHolder = primaryAccountHolder, 
                    MailingAddress = mailingAddress 
                });

                Log.LogInformation("NewCustomer: confirming events");
                await mgr.ConfirmEvents();

                Log.LogInformation("NewCustomer: returning GetManagedState");
                return new APIResult<Customer>(await mgr.GetManagedState());
            }
            catch (Exception ex)
            {
                Log.LogError(ex, "NewCustomer: exception");
                return new APIResult<Customer>(ex?.InnerException ?? ex);
            }
        }

        public async Task<APIResult<Customer>> RemoveAccount(Guid customerId, string accountNumber)
        {
            try
            {
                var mgr = OrleansClient.GetGrain<ICustomerJournal>(customerId);
                await mgr.RaiseEvent(new RemoveAccount { 
                    AccountNumber = accountNumber 
                });
                await mgr.ConfirmEvents();
                return new APIResult<Customer>(await mgr.GetManagedState());
            }
            catch (Exception ex)
            {
                return new APIResult<Customer>(ex?.InnerException ?? ex);
            }
        }

        public async Task<APIResult<Customer>> UpdateMailingAddress(Guid customerId, Address address)
        {
            try
            {
                var mgr = OrleansClient.GetGrain<ICustomerJournal>(customerId);
                await mgr.RaiseEvent(new ChangeMailingAddress { 
                    Address = address
                });
                await mgr.ConfirmEvents();
                return new APIResult<Customer>(await mgr.GetManagedState());
            }
            catch (Exception ex)
            {
                return new APIResult<Customer>(ex?.InnerException ?? ex);
            }
        }

        public async Task<APIResult<Customer>> UpdatePrimaryResidence(Guid customerId, Address address)
        {
            try
            {
                var mgr = OrleansClient.GetGrain<ICustomerJournal>(customerId);
                await mgr.RaiseEvent(new ChangePrimaryResidence { 
                    Address = address
                });
                await mgr.ConfirmEvents();
                return new APIResult<Customer>(await mgr.GetManagedState());
            }
            catch (Exception ex)
            {
                return new APIResult<Customer>(ex?.InnerException ?? ex);
            }
        }

        public async Task<APIResult<Customer>> UpdateSpouseResidence(Guid customerId, Address address)
        {
            try
            {
                var mgr = OrleansClient.GetGrain<ICustomerJournal>(customerId);
                await mgr.RaiseEvent(new ChangeSpouseResidence { 
                    Address = address
                });
                await mgr.ConfirmEvents();
                return new APIResult<Customer>(await mgr.GetManagedState());
            }
            catch (Exception ex)
            {
                return new APIResult<Customer>(ex?.InnerException ?? ex);
            }
        }

        public async Task<APIResult<Customer>> UpdateSpouse(Guid customerId, Person spouse)
        {
            try
            {
                var mgr = OrleansClient.GetGrain<ICustomerJournal>(customerId);
                if (spouse != null)
                {
                    await mgr.RaiseEvent(new ChangeSpouse
                    {
                        Spouse = spouse
                    });
                }
                else
                {
                    await mgr.RaiseEvent(new RemoveSpouse());
                }
                await mgr.ConfirmEvents();
                return new APIResult<Customer>(await mgr.GetManagedState());
            }
            catch (Exception ex)
            {
                return new APIResult<Customer>(ex?.InnerException ?? ex);
            }
        }

        public async Task<APIResult<Customer>> PostAccountTransaction(Guid customerId, string accountNumber, decimal amount)
        {
            try
            {
                var mgr = OrleansClient.GetGrain<ICustomerJournal>(customerId);

                var acct = (await mgr.GetManagedState()).Accounts.Find(a => a.AccountNumber.Equals(accountNumber));
                if (acct == null) return new APIResult<Customer>("Account not found");

                await mgr.RaiseEvent(new PostTransaction
                {
                    AccountNumber = accountNumber,
                    Amount = amount
                });
                await mgr.ConfirmEvents();

                return new APIResult<Customer>(await mgr.GetManagedState());
            }
            catch (Exception ex)
            {
                return new APIResult<Customer>(ex?.InnerException ?? ex);
            }
        }

    }
}
