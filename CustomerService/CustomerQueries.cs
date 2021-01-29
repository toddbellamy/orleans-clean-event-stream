using DomainModel;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Concurrency;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace CustomerService
{
    [Reentrant]
    [StatelessWorker]
    public class CustomerQueries 
        : Grain, ICustomerQueries
    {
        private static string ConnectionString = string.Empty;

        private readonly IClusterClient OrleansClient;
        private readonly ILogger<CustomerQueries> Log;

        static CustomerQueries()
        {
            var connect = Environment.GetEnvironmentVariable("OrleansCESConnection");
            if (string.IsNullOrWhiteSpace(connect))
            {
                throw new ApplicationException($"Connection string not found for {nameof(CustomerQueries)} component.");
            }
            ConnectionString = connect;
        }

        public CustomerQueries(IClusterClient clusterClient, ILogger<CustomerQueries> log)
        {
            OrleansClient = clusterClient;
            Log = log;
        }

        public async Task<APIResult<Customer>> FindCustomer(Guid customerId)
        {
            try
            {
                var mgr = OrleansClient.GetGrain<ICustomerJournal>(customerId);
                
                var customer = await mgr.GetManagedState();
                return new APIResult<Customer>(customer);
            }
            catch (Exception ex)
            {
                Log.LogError(ex, "FindCustomer exception");
                return new APIResult<Customer>(ex);
            }
        }

        public async Task<APIResult<List<Guid>>> FindAllCustomerIds()
        {
            try
            {
                List<Guid> results = new List<Guid>();
                using var conn = new SqlConnection(ConnectionString);
                await conn.OpenAsync();
                using var cmd = new SqlCommand("SELECT DISTINCT Id FROM CustomerEventStream;", conn);
                using var reader = await cmd.ExecuteReaderAsync();
                if(reader.HasRows)
                {
                    while(await reader.ReadAsync())
                        results.Add(reader.GetGuid(0));
                }
                await reader.CloseAsync();
                await conn.CloseAsync();
                return new APIResult<List<Guid>>(results);
            }
            catch(Exception ex)
            {
                Log.LogError(ex, "FindAllCustomerIds exception");
                return new APIResult<List<Guid>>(ex);
            }
        }

        public async Task<APIResult<bool>> CustomerExists(Guid customerId)
        {
            try
            {
                using var conn = new SqlConnection(ConnectionString);
                await conn.OpenAsync();
                using var cmd = new SqlCommand("SELECT COUNT(*) AS ScalarVal FROM CustomerEventStream WHERE Id = @customerId;", conn);
                cmd.Parameters.AddWithValue("@customerId", customerId);
                int count = (int)await cmd.ExecuteScalarAsync();
                await conn.CloseAsync();
                return new APIResult<bool>(count > 0);
            }
            catch (Exception ex)
            {
                Log.LogError(ex, "CustomerExists exception");
                return new APIResult<bool>(ex);
            }
        }
    }
}
