﻿using MediatR;

using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.Abstractions.Notifications;
using P3D.Legacy.Server.Application.Commands.Player;
using P3D.Legacy.Server.Application.Services;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Application.CommandHandlers.Player
{
    internal class PlayerFinalizingCommandHandler : IRequestHandler<PlayerFinalizingCommand>
    {
        private readonly ILogger _logger;
        private readonly IMediator _mediator;
        private readonly IPlayerContainerWriter _playerContainer;

        public PlayerFinalizingCommandHandler(ILogger<PlayerFinalizingCommandHandler> logger, IMediator mediator, IPlayerContainerWriter playerContainer)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _playerContainer = playerContainer ?? throw new ArgumentNullException(nameof(playerContainer));
        }

        public async Task<Unit> Handle(PlayerFinalizingCommand request, CancellationToken ct)
        {
            var player = request.Player;

            if (await _playerContainer.RemoveAsync(player, ct))
            {
                if (!player.Id.IsEmpty)
                {
                    await _mediator.Publish(new PlayerLeavedNotification(player.Id, player.Origin, player.Name), ct);
                }
            }

            return Unit.Value;
        }
    }
}