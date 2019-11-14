using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Features;
using NServiceBus.Persistence.Sql;

namespace TestSaga
{
    public class Program
    {
        private static IEndpointInstance _endpointInstance;

        private static IEndpointInstance EndpointInstance => _endpointInstance ?? (_endpointInstance = InitializeInstance());
        static void Main(string[] args)
        {

            try
            {
                while (true)
                {
                    InitializeInstance();

                    TestMessage testMessage = new TestMessage();

                    var options = new SendOptions();
                    options.SetDestination("TestSagaSubscriber");

                    EndpointInstance.Send(testMessage, options).GetAwaiter().GetResult();

                    Console.ReadLine();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.ReadLine();
            }
        }

        private static IEndpointInstance InitializeInstance()
        {
            Console.Write("Bootstrapping NServiceBus publisher...");

            AppSettings appSettings=new AppSettings();

            //publisher starts
            var publisherendpointConfiguration = new EndpointConfiguration("TestSagaPublisher");
            publisherendpointConfiguration.SendFailedMessagesTo("TestSagaPublisher.error");
            //publisherendpointConfiguration.EnableInstallers("BagheA");

            var transportPub = publisherendpointConfiguration.UseTransport<MsmqTransport>();

            publisherendpointConfiguration.DisableFeature<Sagas>();

            //license
            publisherendpointConfiguration.LicensePath(AppSettings.LicensePath);

            publisherendpointConfiguration.UsePersistence<InMemoryPersistence, StorageType.Timeouts>();


            var persistencePub = publisherendpointConfiguration.UsePersistence<MsmqPersistence, StorageType.Subscriptions>();
            persistencePub.SubscriptionQueue("TestSagaPublisher.subscriptions");

            publisherendpointConfiguration.Conventions()
                .DefiningCommandsAs(t => t == typeof(TestMessage));

            var endpointInstancePub = Endpoint.Start(publisherendpointConfiguration).GetAwaiter().GetResult();
            Thread.Sleep(3000);
            Console.WriteLine(" publisher started.");


            // publisher ends




            //subscriber starts

            Console.Write("Bootstrapping NServiceBus subscriber...");

            var subscriberendpointConfiguration = new EndpointConfiguration("TestSagaSubscriber");


            //subscriberendpointConfiguration.EnableInstallers("BagheA");
            subscriberendpointConfiguration.SendFailedMessagesTo("TestSagaSubscriber.error");

            //
            var transport = subscriberendpointConfiguration.UseTransport<MsmqTransport>();
            //transport.DisableInstaller();
            transport.Transactions(TransportTransactionMode.TransactionScope);

            // timeouts
            var persistence = subscriberendpointConfiguration.UsePersistence<SqlPersistence, StorageType.Timeouts>();
            persistence.SqlDialect<SqlDialect.MsSqlServer>();
            persistence.ConnectionBuilder(() => new SqlConnection(appSettings.NServiceBusPersistence));
            //persistence.DisableInstaller();
                //ends

            // sagas
            var persistenceSaga = subscriberendpointConfiguration.UsePersistence<SqlPersistence, StorageType.Sagas>();
            persistenceSaga.SqlDialect<SqlDialect.MsSqlServer>();
            persistenceSaga.ConnectionBuilder(() => new SqlConnection(appSettings.NServiceBusPersistence));
            //persistenceSaga.DisableInstaller();
            //ends

            //subscription
            subscriberendpointConfiguration.UsePersistence<InMemoryPersistence, StorageType.Subscriptions>();

            //sendonly
            subscriberendpointConfiguration.SendOnly();

            //
            //subscriberendpointConfiguration.Recoverability().Immediate(settings => 
            //    { settings.NumberOfRetries(appSettings.MaxImmediateRetries);});
            //subscriberendpointConfiguration.Recoverability().Delayed(settings =>
            //    {
            //        settings.NumberOfRetries(appSettings.MaxDelayedRetries);
            //    });


            subscriberendpointConfiguration.Conventions()
                .DefiningEventsAs(t => t == typeof(TestMessage));

            //license
            subscriberendpointConfiguration.LicensePath(AppSettings.LicensePath);

            var endpointInstance = Endpoint.Start(subscriberendpointConfiguration).GetAwaiter().GetResult();
            Thread.Sleep(3000);
            Console.WriteLine(" subscriber done.");
            // subscriber ends
            _endpointInstance = endpointInstancePub;

            return _endpointInstance;
        }
    }
}
