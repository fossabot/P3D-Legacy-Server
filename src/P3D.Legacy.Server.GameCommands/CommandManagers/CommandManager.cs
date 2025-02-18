﻿using MediatR;

using Microsoft.Extensions.DependencyInjection;

using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Abstractions.Notifications;
using P3D.Legacy.Server.Abstractions.Services;
using P3D.Legacy.Server.Application.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.GameCommands.CommandManagers
{
    internal abstract class CommandManager
    {
        public abstract string Name { get; }
        public abstract string Description { get; }
        public virtual IEnumerable<string> Aliases { get; } = Array.Empty<string>();
        public virtual PermissionFlags Permissions { get; } = PermissionFlags.None;
        public virtual bool LogCommand { get; } = true;

        protected IMediator Mediator { get; }
        protected NotificationPublisher NotificationPublisher { get; }
        private IPlayerContainerReader PlayerContainer { get; }

        protected CommandManager(IServiceProvider serviceProvider)
        {
            if (serviceProvider is null)
                throw new ArgumentNullException(nameof(serviceProvider));

            Mediator = serviceProvider.GetRequiredService<IMediator>();
            NotificationPublisher = serviceProvider.GetRequiredService<NotificationPublisher>();
            PlayerContainer = serviceProvider.GetRequiredService<IPlayerContainerReader>();
        }

        protected async Task<IPlayer?> GetPlayerAsync(string name, CancellationToken ct)
        {
            return await PlayerContainer.GetAllAsync(ct).FirstOrDefaultAsync(x => x.Name.Equals(name, StringComparison.Ordinal), ct);
        }

        protected async Task SendMessageAsync(IPlayer player, string message, CancellationToken ct)
        {
            await NotificationPublisher.Publish(new MessageToPlayerNotification(IPlayer.Server, player, message), ct);
        }

        protected async Task SendServerMessageAsync(string message, CancellationToken ct)
        {
            await NotificationPublisher.Publish(new ServerMessageNotification(message), ct);
        }

        public virtual async Task HandleAsync(IPlayer player, string alias, string[] arguments, CancellationToken ct)
        {
            await HelpAsync(player, alias, ct);
        }

        public virtual async Task HelpAsync(IPlayer player, string alias, CancellationToken ct)
        {
            await SendMessageAsync(player, $@"Command ""{alias}"" is not functional!", ct);
        }
    }
}