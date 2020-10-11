﻿using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using ProgImage.Compress.Helpers;
using ProgImage.Compress.RabbitMQ.Services;

namespace ProgImage.Compress
{
    public static class ApplicationBuilderExtensions
    {
        private static IConsumer Consumer { get; set; }

        public static IApplicationBuilder UseRabbitListener(this IApplicationBuilder app)
        {
            Consumer = (Consumer) app.ApplicationServices.GetService(typeof(IConsumer));

            IApplicationLifetime life = app.ApplicationServices.GetService<IApplicationLifetime>();

            life.ApplicationStarted.Register(OnStarted);

            life.ApplicationStopping.Register(OnStopping);

            return app;
        }

        private static void OnStarted()
        {
            Consumer.Receive();
        }
        
        private static void OnStopping()
        {
            Consumer.Stop();
        }
    }
}