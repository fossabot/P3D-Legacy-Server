﻿using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Application.Commands.Administration;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.GameCommands.CommandManagers.Player
{
    internal class KickCommandManager : CommandManager
    {
        public override string Name => "kick";
        public override string Description => "Kick a Player.";
        public override IEnumerable<string> Aliases => new[] { "k" };
        public override PermissionFlags Permissions => PermissionFlags.ModeratorOrHigher;

        public KickCommandManager(IServiceProvider serviceProvider) : base(serviceProvider) { }

        public override async Task HandleAsync(IPlayer player, string alias, string[] arguments, CancellationToken ct)
        {
            if (arguments.Length == 1)
            {
                var targetName = arguments[0];
                if (await GetPlayerAsync(targetName, ct) is not { } targetPlayer)
                {
                    await SendMessageAsync(player, $"Player {targetName} not found!", ct);
                    return;
                }

                if (targetPlayer.Id == player.Id)
                {
                    await SendMessageAsync(player, "You can't kick yourself!", ct);
                    return;
                }

                await Mediator.Send(new KickPlayerCommand(targetPlayer, "Kicked by a Moderator or Admin."), ct);
            }
            else if (arguments.Length > 1)
            {
                var targetName = arguments[0];
                if (await GetPlayerAsync(targetName, ct) is not { } targetPlayer)
                {
                    await SendMessageAsync(player, $"Player {targetName} not found!", ct);
                    return;
                }

                if (targetPlayer.Id == player.Id)
                {
                    await SendMessageAsync(player, "You can't kick yourself!", ct);
                    return;
                }

                var reason = string.Join(" ", arguments.Skip(1).ToArray());
                var result = await Mediator.Send(new KickPlayerCommand(targetPlayer, reason), ct);
                if (!result.Success)
                    await SendMessageAsync(player, $"Failed to kick player {targetName}!", ct);
            }
            else
                await SendMessageAsync(player, "Invalid arguments given.", ct);
        }

        public override async Task HelpAsync(IPlayer player, string alias, CancellationToken ct)
        {
            await SendMessageAsync(player, $"Correct usage is /{alias} <playername> [<reason>]", ct);
        }
    }
}