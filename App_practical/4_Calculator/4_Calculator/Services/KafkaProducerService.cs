using Confluent.Kafka;
using System;
using System.Threading.Tasks;

namespace Calculator.Services
{
    public class KafkaProducerService<K, V>
    {
        private readonly IProducer<K, V> kafkaHandle;

        public KafkaProducerService(KafkaProducerHandler kafkaProducerHandler)
        {
            kafkaHandle = new DependentProducerBuilder<K, V>(kafkaProducerHandler.Handle).Build();
        }

        /// <summary>
        /// Асинхронная отправка сообщения и ожидание результата.
        /// </summary>
        public Task ProduceAsync(string topic, Message<K, V> message)
            => kafkaHandle.ProduceAsync(topic, message);

        /// <summary>
        /// Асинхронная отправка сообщения без ожидания (через callback).
        /// </summary>
        public void Produce(string topic, Message<K, V> message, Action<DeliveryReport<K, V>> deliveryHandler = null)
            => kafkaHandle.Produce(topic, message, deliveryHandler);

        public void Flush(TimeSpan timeout)
            => kafkaHandle.Flush(timeout);
    }
}