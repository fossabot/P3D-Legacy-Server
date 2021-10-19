﻿using MediatR;
using MediatR.Pipeline;
using MediatR.Registration;

using Microsoft.Extensions.DependencyInjection;

using P3D.Legacy.Server.Application.CommandHandlers.Administration;
using P3D.Legacy.Server.Application.CommandHandlers.Player;
using P3D.Legacy.Server.Application.CommandHandlers.World;
using P3D.Legacy.Server.Application.Commands;
using P3D.Legacy.Server.Application.Commands.Administration;
using P3D.Legacy.Server.Application.Commands.Player;
using P3D.Legacy.Server.Application.Commands.World;
using P3D.Legacy.Server.Application.Notifications;
using P3D.Legacy.Server.Application.Services;
using P3D.Legacy.Server.Application.Utils;
using P3D.Legacy.Server.BackgroundServices;
using P3D.Legacy.Server.Behaviours;

using System;
using System.Collections.Generic;
using System.Linq;

namespace P3D.Legacy.Server.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMediatRInternal(this IServiceCollection services, params IEnumerable<(Type, (Type, ServiceLifetime))>[] notifications)
        {
            ServiceRegistrar.AddRequiredServices(services, new MediatRServiceConfiguration().AsTransient());

            new RequestRegistrar
            {
                { typeof(IRequestHandler<ChangePlayerPermissionsCommand, CommandResult>), typeof(ChangePlayerPermissionsCommandHandler) },
                { typeof(IRequestHandler<PlayerFinalizingCommand>), typeof(PlayerFinalizingCommandHandler) },
                { typeof(IRequestHandler<PlayerInitializingCommand>), typeof(PlayerInitializingCommandHandler) },
                { typeof(IRequestHandler<PlayerReadyCommand>), typeof(PlayerReadyCommandHandler) },
                { typeof(IRequestHandler<PlayerMutedPlayerCommand, CommandResult>), typeof(PlayerMutedPlayerCommandHandler) },
                { typeof(IRequestHandler<PlayerUnmutedPlayerCommand, CommandResult>), typeof(PlayerUnmutedPlayerCommandHandler) },

                { typeof(IRequestHandler<BanPlayerCommand, CommandResult>), typeof(BanPlayerCommandHandler) },
                { typeof(IRequestHandler<UnbanPlayerCommand, CommandResult>), typeof(UnbanPlayerCommandHandler) },
                { typeof(IRequestHandler<KickPlayerCommand, CommandResult>), typeof(KickPlayerCommandHandler) },

                { typeof(IRequestHandler<ChangeWorldSeasonCommand, CommandResult>), typeof(ChangeWorldSeasonCommandHandler) },
                { typeof(IRequestHandler<ChangeWorldTimeCommand, CommandResult>), typeof(ChangeWorldTimeCommandHandler) },
                { typeof(IRequestHandler<ChangeWorldWeatherCommand, CommandResult>), typeof(ChangeWorldWeatherCommandHandler) },
            }.Register(services);

            new NotificationRegistrar
            {
                { sp => sp.GetRequiredService<IPlayerContainerReader>().GetAll().OfType<INotificationHandler<PlayerJoinedNotification>>() },
                { sp => sp.GetRequiredService<IPlayerContainerReader>().GetAll().OfType<INotificationHandler<PlayerLeavedNotification>>() },
                { sp => sp.GetRequiredService<IPlayerContainerReader>().GetAll().OfType<INotificationHandler<PlayerUpdatedStateNotification>>() },
                { sp => sp.GetRequiredService<IPlayerContainerReader>().GetAll().OfType<INotificationHandler<PlayerSentGlobalMessageNotification>>() },
                { sp => sp.GetRequiredService<IPlayerContainerReader>().GetAll().OfType<INotificationHandler<PlayerSentLocalMessageNotification>>() },
                { sp => sp.GetRequiredService<IPlayerContainerReader>().GetAll().OfType<INotificationHandler<MessageToPlayerNotification>>() },
                { sp => sp.GetRequiredService<IPlayerContainerReader>().GetAll().OfType<INotificationHandler<PlayerSentRawP3DPacketNotification>>() },
                { sp => sp.GetRequiredService<IPlayerContainerReader>().GetAll().OfType<INotificationHandler<ServerMessageNotification>>() },
                { sp => sp.GetRequiredService<IPlayerContainerReader>().GetAll().OfType<INotificationHandler<PlayerTriggeredEventNotification>>() },
                { sp => sp.GetRequiredService<IPlayerContainerReader>().GetAll().OfType<INotificationHandler<PlayerSentCommandNotification>>() },

                { sp => sp.GetRequiredService<DiscordPassthroughService>() as INotificationHandler<PlayerJoinedNotification> },
                { sp => sp.GetRequiredService<DiscordPassthroughService>() as INotificationHandler<PlayerLeavedNotification> },
                { sp => sp.GetRequiredService<DiscordPassthroughService>() as INotificationHandler<PlayerSentGlobalMessageNotification> },
                { sp => sp.GetRequiredService<DiscordPassthroughService>() as INotificationHandler<ServerMessageNotification> },
                { sp => sp.GetRequiredService<DiscordPassthroughService>() as INotificationHandler<PlayerTriggeredEventNotification> },

                notifications.SelectMany(x => x)
            }.Register(services);

            services.AddTransient(typeof(IRequestPreProcessor<>), typeof(LoggingBehaviour<>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TracingBehaviour<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>));
            //services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));

            return services;
        }
    }
}