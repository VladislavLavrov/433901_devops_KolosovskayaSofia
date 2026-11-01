using Confluent.Kafka;
using Calculator.Data;
using Calculator.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Calculator.Services
{
    public class KafkaConsumerService : BackgroundService
    {
        private readonly string _topic;
        private readonly IConsumer<Null, string> _kafkaConsumer;
        private readonly IServiceProvider _serviceProvider;
        private readonly IHttpClientFactory _clientFactory;

        public KafkaConsumerService(
            IConfiguration config,
            IServiceProvider serviceProvider,
            IHttpClientFactory clientFactory)
        {
            _topic = config["Kafka:TopicName"];

            var consumerConfig = new ConsumerConfig();
            config.GetSection("Kafka:ConsumerSettings").Bind(consumerConfig);
            _kafkaConsumer = new ConsumerBuilder<Null, string>(consumerConfig).Build();

            _serviceProvider = serviceProvider;
            _clientFactory = clientFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _kafkaConsumer.Subscribe(_topic);

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var cr = _kafkaConsumer.Consume(cancellationToken);

                        // Исходные данные
                        var inputData = JsonSerializer.Deserialize<DataInputVariant>(cr.Message.Value);

                        // Выполнение расчета
                        var result = CalculatorLibrary.CalculateOperation(
                            inputData.Operand_1,
                            inputData.Operand_2,
                            inputData.Type_operation);
                        inputData.Result = result.ToString();

                        var httpClient = _clientFactory.CreateClient();
                        await httpClient.PostAsJsonAsync(
                            "http://localhost:5004/Calculator/Callback",
                            inputData);

                        Console.WriteLine($"Message key: {cr.Message.Key}, value: {cr.Message.Value}");
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (ConsumeException e)
                    {
                        if (e.Error.IsFatal)
                        {
                            break;
                        }
                    }
                    catch (Exception e)
                    {
                        break;
                    }
                }
            }
            finally
            {
                _kafkaConsumer.Close();
            }
        }

        public override void Dispose()
        {
            _kafkaConsumer.Close();
            _kafkaConsumer.Dispose();
            base.Dispose();
        }
    }
}