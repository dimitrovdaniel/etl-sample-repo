using Azure.Core;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodatExtractor.DAL.Services
{
    // calls Azure Service Bus to communicate with Azure Functions
    public class ServiceBusService
    {
        private ServiceBusClient _client;
        private ServiceBusSender _sender;
        private string _connString;

        public ServiceBusService(string connString)
        {
            _connString = connString;
        }

        // initialize with credentials
        public void InitServiceBus()
        {
            var clientOptions = new ServiceBusClientOptions
            {
                TransportType = ServiceBusTransportType.AmqpWebSockets
            };
            _client = new ServiceBusClient(_connString,
                clientOptions);
            _sender = _client.CreateSender("processperiodtopic");
        }

        // send a message to Topic for AFs to listen to
        public async Task SendMessage(string message)
        {
            using ServiceBusMessageBatch messageBatch = await _sender.CreateMessageBatchAsync();
            await _sender.SendMessageAsync(new ServiceBusMessage(message));
        }

        public async Task DisposeServiceBus()
        {
            await _sender.DisposeAsync();
            await _client.DisposeAsync();
        }
    }
}
