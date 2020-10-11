﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using ProgImage.Resize.Events;
using ProgImage.Transformation.Controllers.DTO.Response;
using ProgImage.Transformation.Domain.Models;
using ProgImage.Transformation.Domain.Services;
using ProgImage.Transformation.Helpers;
using ProgImage.Transformation.Models.DTO;
using ProgImage.Transformation.RabbitMQ.Services;

namespace ProgImage.Transformation.Services
{
    public class RotateService : IRotateService
    {
        private readonly IStatusService _statusService;
        private readonly IProducer _producer;
        
        public RotateService(IStatusService statusService, IProducer producer)
        {
            _statusService = statusService;
            _producer = producer;
        }

        public async Task<TransformationStatusResponse> TransformImage(Guid imageId, int degrees)
        {
            Guid statusId = Guid.NewGuid();
            
            TransformationRotateStartEvent @event = new TransformationRotateStartEvent
            {
                StatusId = statusId,
                Url = $"http://progimage-storage:8080/api/v1/progimage/storage/{imageId}",
                Degrees = degrees
            };
            
            _producer.Push(@event, "progimage.transformation.blur");
            
            TransformationStatusResponse transformationStatusResponse = await CreateStatus(statusId, "Queued");

            return transformationStatusResponse;
        }
        
        public async Task<TransformationStatusResponse> TransformImage(IFormFile image, int degrees)
        {
            Guid statusId = Guid.NewGuid();
            Image uploadedImage = await HttpHelper.PostImageAsync(image, "http://progimage-storage:8080/api/v1/progimage/storage");
            
            TransformationRotateStartEvent @event = new TransformationRotateStartEvent
            {
                StatusId = statusId,
                Url = $"http://progimage-storage:8080/api/v1/progimage/storage/{uploadedImage.ImageId}",
                Degrees = degrees
            };
            
            _producer.Push(@event, "progimage.transformation.blur");
            
            TransformationStatusResponse transformationStatusResponse = await CreateStatus(statusId, "Queued");

            return transformationStatusResponse;            
        }

        public async Task<TransformationStatusResponse> TransformImage(string url, int degrees)
        {
            Guid statusId = Guid.NewGuid();
            
            TransformationRotateStartEvent @event = new TransformationRotateStartEvent
            {
                StatusId = statusId,
                Url = url,
                Degrees = degrees
            };
            
            _producer.Push(@event, "progimage.transformation.blur");
            
            TransformationStatusResponse transformationStatusResponse = await CreateStatus(statusId, "Queued");

            return transformationStatusResponse;
        }
        
        private async Task<TransformationStatusResponse> CreateStatus(Guid statusId, string status)
        {
            TransformationStatus transformationStatus = new TransformationStatus
            {
                StatusId = statusId,
                ImageId = null,
                Status = status
            };

            TransformationStatusResponse transformationStatusResponse = await _statusService.AddStatusAsync(transformationStatus);
            
            return transformationStatusResponse;
        }
    }
}