﻿using System;
using System.Threading.Tasks;
using ProgImage.Blur.Domain.Events;
using ProgImage.Blur.Helpers;
using ProgImage.Blur.Models.DTO;
using ProgImage.Blur.RabbitMQ.Connection;
using ProgImage.Blur.Services;
using ProgImage.Resize.Events;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog;

namespace ProgImage.Blur.RabbitMQ.Services
{
    public class Consumer : IConsumer
    {
        private readonly IProducer _producer;
        private readonly IConnection _rabbitMqConnection;
        private readonly IModel _channel;
        private readonly AsyncEventingBasicConsumer _consumer;
        private string _consumerTag;

        public Consumer(IRabbitMqConnection rabbitMq, IProducer producer)
        {
            _producer = producer;
            _rabbitMqConnection = rabbitMq.CreateConnection();
            _channel = _rabbitMqConnection.CreateModel();
            _consumer = new AsyncEventingBasicConsumer(_channel);

            _channel.BasicQos(0, 1, false);
            _channel.ExchangeDeclare(EnvVariables.RabbitMqExchangeName, EnvVariables.RabbitMqExchangeType, true);
            _channel.QueueDeclare(EnvVariables.RabbitMqQueueName, true, false,false);
            _channel.QueueBind(EnvVariables.RabbitMqQueueName, EnvVariables.RabbitMqExchangeName, EnvVariables.RabbitMqConsumerBindingKey);
        }
        
        public void Receive()
        {
            _consumer.Received += OnConsumerOnReceived;
            _consumerTag = _channel.BasicConsume(EnvVariables.RabbitMqQueueName,
                false,
                _consumer);
        }

        public void Stop()
        {
            // Gracefully shutdown.
            _channel.BasicCancel(_consumerTag);
            _channel.Close(200, "Quitting.");
            _rabbitMqConnection.Close();
        }

        private async Task OnConsumerOnReceived(object sender, BasicDeliverEventArgs ea)
        {
            byte[] imageBytes;
            TransformationBlurStartEvent @event = ea.Body.ToArray().ToObject<TransformationBlurStartEvent>();

            _channel.BasicAck(ea.DeliveryTag, false);

            try
            {
                imageBytes = await HttpHelper.GetImageAsync(@event.Url);
            }
            catch (Exception)
            {
                UpdateEventAsync(@event.StatusId, null, "Error: Unable to fetch image by id");
                throw;
            }
            
            byte[] resize = new BlurService().BlurImage(imageBytes, @event.Radius, @event.Sigma); 
            Image resizedImage = await HttpHelper.PostImageAsync(resize);
        
            UpdateEventAsync(@event.StatusId, resizedImage.ImageId, "Processed");

            Log.Information("[Blur] Consumed message: " + @event.ToString<TransformationBlurStartEvent>());
        }

        private async Task UpdateEventAsync(Guid statusId, Guid? imageId, string status)
        {
            TransformationStatusEvent @event = new TransformationStatusEvent
            {
                StatusId = statusId,
                ImageId = imageId,
                Status = status
            };
            
            _producer.Push(@event, EnvVariables.RabbitMqProducerBindingKey);
        } 
    }
}