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
        private readonly IConfiguration _config;
        private readonly IServiceProvider _serviceProvider;
        private readonly IHttpClientFactory _clientFactory;

        public KafkaConsumerService(
            IConfiguration config,
            IServiceProvider serviceProvider,
            IHttpClientFactory clientFactory)
        {
            _config = config;
            _topic = config["Kafka:TopicName"];
            _serviceProvider = serviceProvider;
            _clientFactory = clientFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            // ⚠️ Всё, что связано с Kafka — внутри фонового потока
            IConsumer<Null, string> consumer = null;
            try
            {
                var consumerConfig = new ConsumerConfig();
                _config.GetSection("Kafka:ConsumerSettings").Bind(consumerConfig);
                consumer = new ConsumerBuilder<Null, string>(consumerConfig).Build();

                consumer.Subscribe(_topic);

                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var cr = consumer.Consume(cancellationToken);

                        var inputData = JsonSerializer.Deserialize<DataInputVariant>(cr.Message.Value);
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
                        Console.WriteLine($"[KAFKA] Ошибка Consume: {e.Error.Reason}");
                        // ⚠️ НЕ break — пусть продолжает пытаться
                        await Task.Delay(5000, cancellationToken);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"[KAFKA] Ошибка: {e.Message}");
                        await Task.Delay(5000, cancellationToken);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"[KAFKA] Фатальная ошибка: {e.Message}");
            }
            finally
            {
                consumer?.Close();
                consumer?.Dispose();
            }
        }
    }
}