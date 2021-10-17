﻿using MediatR;

using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Application.Notifications;
using P3D.Legacy.Server.GameCommands.CommandManagers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.GameCommands.NotificationHandlers
{
    public sealed class CommandManagerHandler :
        INotificationHandler<PlayerSentCommandNotification>
    {
        private readonly ILogger _logger;
        private readonly IMediator _mediator;
        private readonly IReadOnlyList<CommandManager> _commandManagers;

        public CommandManagerHandler(ILogger<CommandManagerHandler> logger, IMediator mediator, IEnumerable<CommandManager> commandManagers)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _commandManagers = commandManagers.ToList() ?? throw new ArgumentNullException(nameof(commandManagers));
        }

        private async Task SendMessageAsync(IPlayer player, string message, CancellationToken ct)
        {
            await _mediator.Publish(new MessageToPlayerNotification(IPlayer.Server, player, message), ct);
        }

        private async Task HandleCommandAsync(IPlayer player, string alias, string[] arguments, CancellationToken ct)
        {
            var command = FindByName(alias) ?? FindByAlias(alias);
            if (command == null)
            {
                await SendMessageAsync(player, $@"Invalid command ""{alias}"".", ct);
                return;
            }

            if (command.LogCommand && (player.Permissions & PermissionFlags.UnVerified) == 0)
                _logger.LogInformation("{PlayerName}: /{CommandAlias} {CommandArgs}", player.Name, alias, string.Join(" ", arguments));

            if (command.Permissions == PermissionFlags.None)
            {
                await SendMessageAsync(player, "Command is not found!", ct);
                //await SendMessageAsync(player, @"Command is disabled!", ct);
                return;
            }

            if ((player.Permissions & command.Permissions) == PermissionFlags.None)
            {
                await SendMessageAsync(player, "Command is not found!", ct);
                //await SendMessageAsync(player, @"You have not the permission to use this command!", ct);
                return;
            }

            await command.HandleAsync(player, alias, arguments, ct);
        }

        public CommandManager? FindByName(string name) => _commandManagers
            .Where(x => x.Permissions != PermissionFlags.None)
            .FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        public CommandManager? FindByAlias(string alias) => _commandManagers
            .Where(x => x.Permissions != PermissionFlags.None)
            .FirstOrDefault(x => x.Aliases.Contains(alias, StringComparer.OrdinalIgnoreCase));

        public IReadOnlyList<CommandManager> GetCommands() => _commandManagers;


        public async Task Handle(PlayerSentCommandNotification notification, CancellationToken ct)
        {
            var (player, message) = notification;

            var commandWithoutSlash = message.TrimStart('/');

            var messageArray = new Regex(@"[ ](?=(?:[^""]*""[^""]*"")*[^""]*$)").Split(commandWithoutSlash).Select(str => str.TrimStart('"').TrimEnd('"')).ToArray();
            //var messageArray = commandWithoutSlash.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (messageArray.Length == 0)
            {
                await SendMessageAsync(player, "Command is not found!", ct);
                return;
            }

            var alias = messageArray[0];
            var trimmedMessageArray = messageArray.Skip(1).ToArray();

            if (!_commandManagers.Any(c => c.Name == alias || c.Aliases.Any(a => a == alias)))
            {
                await SendMessageAsync(player, "Command is not found!", ct);
                return;
            }

            await HandleCommandAsync(player, alias, trimmedMessageArray, ct);
        }
    }
}