using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using System;

namespace Calculator.Services
{
    public class KafkaProducerHandler : IDisposable
    {
        private IProducer<byte[], byte[]> kafkaProducer;

        public KafkaProducerHandler(IConfiguration config)
        {
            var producerConfig = new ProducerConfig();
            config.GetSection("Kafka:ProducerSettings").Bind(producerConfig);
            kafkaProducer = new ProducerBuilder<byte[], byte[]>(producerConfig).Build();
        }

        public Handle Handle => kafkaProducer.Handle;

        public void Dispose()
        {
            kafkaProducer.Flush(TimeSpan.FromSeconds(10));
            kafkaProducer.Dispose();
        }
    }
}