﻿using BetterHostedServices;

using MediatR;
using MediatR.Pipeline;
using MediatR.Registration;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using P3D.Legacy.Server.Abstractions.Utils;
using P3D.Legacy.Server.Application.Options;
using P3D.Legacy.Server.Behaviours;
using P3D.Legacy.Server.Options;

using System;
using System.Net.Http.Headers;
using System.Threading;

namespace P3D.Legacy.Server.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddHostMediatR(this IServiceCollection services, IConfiguration configuration, RequestRegistrar requestRegistrar, NotificationRegistrar notificationRegistrar)
        {
            return services;
        }
        public static IServiceCollection AddHost(this IServiceCollection services, IConfiguration configuration)
        {
            var isOfficial = configuration["Server:IsOfficial"]?.Equals("true", StringComparison.OrdinalIgnoreCase) == true;

            services.Configure<ServerOptions>(configuration.GetSection("Server"));
            services.Configure<P3DSiteOptions>(configuration.GetSection("P3D"));


            services.AddBetterHostedServices();

            if (isOfficial)
            {
                services.AddHttpClient("P3D.API")
                    .ConfigureHttpClient((sp, client) =>
                    {
                        var backendOptions = sp.GetRequiredService<IOptions<P3DSiteOptions>>().Value;
                        client.BaseAddress = new Uri(backendOptions.APIEndpointV1);
                        client.DefaultRequestHeaders.Authorization =
                            new AuthenticationHeaderValue("Bearer", backendOptions.APIToken);
                        client.Timeout = Timeout.InfiniteTimeSpan;
                    })
                    .GenerateCorrelationId()
                    .AddPolly();
            }

            return services;
        }

        public static IServiceCollection AddMediatRInternal(this IServiceCollection services)
        {
            ServiceRegistrar.AddRequiredServices(services, new MediatRServiceConfiguration().AsTransient());

            services.AddTransient(typeof(IRequestPreProcessor<>), typeof(LoggingBehaviour<>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TracingBehaviour<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));

            return services;
        }
    }
}