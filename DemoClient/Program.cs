using DomainModel;
using Microsoft.Extensions.Logging;
using Orleans;
using System;
using System.Threading.Tasks;

using CustomerService;
using System.Diagnostics;
using System.Linq;

namespace DemoClient
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("Connecting client.");
            var clusterClient = await GetOrleansClusterClient();
            
            Console.WriteLine("Retrieving CQRS grains.");
            var cmd = clusterClient.GetGrain<ICustomerCommands>(0);
            var query = clusterClient.GetGrain<ICustomerQueries>(0);

            Guid id = new Guid("81B3DEE3-407B-40A9-A89E-C06FDA8C711C"); 

            var exists = await query.CustomerExists(id);
            Console.WriteLine($"Customer exists? {exists.Output}");

            APIResult<Customer> snapshot;

            if(!exists.Output)
            {
                var residence = new Address(
                    street: "10 Main St.",
                    city: "Anytown",
                    stateOrProvince: "TX",
                    postalCode: "90210",
                    country: "USA"
                );

                var person = new Person(
                    fullName: "John Doe",
                    firstName: "John",
                    lastName: "Doe",
                    residence: residence,
                    taxId: "555-55-1234",
                    dateOfBirth: DateTimeOffset.Parse("05/01/1960")
                );

                Console.WriteLine("Creating new customer.");
                snapshot = await cmd.NewCustomer(id, person, residence);
            }
            else
            {
                Console.WriteLine("Retrieving customer.");
                snapshot = await query.FindCustomer(id);
            }

            if(snapshot.Success && snapshot.Output != null)
            {
                Console.WriteLine($"Customer name: {snapshot.Output.PrimaryAccountHolder.FullName}");

                var customer = snapshot.Output;
                var accountNumber = "1234567";
                var account = customer.Accounts
                    .Where(a => a.AccountNumber == accountNumber)
                    .FirstOrDefault();

                if(account == null)
                {
                    snapshot = await cmd.AddAccount(customer.Id, new Account(accountNumber, true));
 
                    customer = snapshot.Output;
                }

                snapshot = await cmd.PostAccountTransaction(customer.Id, accountNumber, 0.99m);
                var msg = snapshot.Success ? "Successful!" : snapshot.Message;
                Console.WriteLine($"Post Account Transaction result: {msg}");
                customer = snapshot.Output;
            }
            else
            {
                Console.WriteLine($"Exception:\n{snapshot.Message ?? "Obtaining Customer failed."}");
            }

            await clusterClient.Close();
            clusterClient.Dispose();

            if(!Debugger.IsAttached)
            {
                Console.WriteLine("\n\nPress any key to exit...");
                Console.ReadKey(true);
            }
        }

        static async Task<IClusterClient> GetOrleansClusterClient()
        {
            var client = new ClientBuilder()
                .ConfigureLogging(logging => {
                    logging
                    .AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("Orleans", LogLevel.Warning)
                    .AddFilter("Runtime", LogLevel.Warning)
                    .AddConsole();
                })
                .UseLocalhostClustering() // cluster and service IDs default to "dev"
                .ConfigureApplicationParts(parts =>
                {
                    parts.AddApplicationPart(typeof(CustomerCommands).Assembly).WithReferences();
                    parts.AddApplicationPart(typeof(CustomerQueries).Assembly).WithReferences();
                    parts.AddApplicationPart(typeof(CustomerJournal).Assembly).WithReferences();
                })
                .Build();
            
            await client.Connect();

            return client;
        }
    }
}
