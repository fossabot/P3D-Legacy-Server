﻿using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.DependencyInjection;

using System;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Application.Services
{
    public class ConnectionContextHandlerFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public ConnectionContextHandlerFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public async Task<TConnectionContextHandler?> CreateAsync<TConnectionContextHandler>(ConnectionContext connectionContext) where TConnectionContextHandler : ConnectionContextHandler
        {
            if (_serviceProvider.GetRequiredService<TConnectionContextHandler>() is { } connectionContextHandler)
            {
                await connectionContextHandler.SetConnectionContextAsync(connectionContext);
                return connectionContextHandler;
            }

            return null;
        }
    }
}