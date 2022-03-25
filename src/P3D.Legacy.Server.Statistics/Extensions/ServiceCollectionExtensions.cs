﻿using MediatR;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using P3D.Legacy.Server.Abstractions.Notifications;
using P3D.Legacy.Server.Abstractions.Utils;
using P3D.Legacy.Server.Statistics.NotificationHandlers;

namespace P3D.Legacy.Server.Statistics.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddStatisticsMediatR(this IServiceCollection services, IConfiguration configuration, RequestRegistrar requestRegistrar, NotificationRegistrar notificationRegistrar)
        {
            notificationRegistrar.Add(sp => sp.GetRequiredService<MetricsHandler>() as INotificationHandler<PlayerUpdatedStateNotification>);
            notificationRegistrar.Add(sp => sp.GetRequiredService<MetricsHandler>() as INotificationHandler<PlayerTriggeredEventNotification>);
            notificationRegistrar.Add(sp => sp.GetRequiredService<MetricsHandler>() as INotificationHandler<PlayerSentGlobalMessageNotification>);
            notificationRegistrar.Add(sp => sp.GetRequiredService<MetricsHandler>() as INotificationHandler<PlayerSentLocalMessageNotification>);
            notificationRegistrar.Add(sp => sp.GetRequiredService<MetricsHandler>() as INotificationHandler<PlayerSentPrivateMessageNotification>);
            notificationRegistrar.Add(sp => sp.GetRequiredService<MetricsHandler>() as INotificationHandler<PlayerSentCommandNotification>);
            notificationRegistrar.Add(sp => sp.GetRequiredService<MetricsHandler>() as INotificationHandler<PlayerJoinedNotification>);
            notificationRegistrar.Add(sp => sp.GetRequiredService<MetricsHandler>() as INotificationHandler<PlayerLeftNotification>);

            notificationRegistrar.Add(sp => sp.GetRequiredService<StatisticsHandler>() as INotificationHandler<PlayerUpdatedStateNotification>);
            notificationRegistrar.Add(sp => sp.GetRequiredService<StatisticsHandler>() as INotificationHandler<PlayerTriggeredEventNotification>);
            notificationRegistrar.Add(sp => sp.GetRequiredService<StatisticsHandler>() as INotificationHandler<PlayerSentGlobalMessageNotification>);
            notificationRegistrar.Add(sp => sp.GetRequiredService<StatisticsHandler>() as INotificationHandler<PlayerSentLocalMessageNotification>);
            notificationRegistrar.Add(sp => sp.GetRequiredService<StatisticsHandler>() as INotificationHandler<PlayerSentPrivateMessageNotification>);
            notificationRegistrar.Add(sp => sp.GetRequiredService<StatisticsHandler>() as INotificationHandler<PlayerSentCommandNotification>);
            notificationRegistrar.Add(sp => sp.GetRequiredService<StatisticsHandler>() as INotificationHandler<PlayerJoinedNotification>);
            notificationRegistrar.Add(sp => sp.GetRequiredService<StatisticsHandler>() as INotificationHandler<PlayerLeftNotification>);

            return services;
        }
        public static IServiceCollection AddStatistics(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<MetricsHandler>();
            services.AddTransient<StatisticsHandler>();

            return services;
        }
    }
}