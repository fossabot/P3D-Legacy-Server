﻿using MediatR;

using NStack;

using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Abstractions.Notifications;
using P3D.Legacy.Server.Application.Commands.Administration;
using P3D.Legacy.Server.Application.Services;
using P3D.Legacy.Server.GUI.Utils;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Terminal.Gui;

namespace P3D.Legacy.Server.GUI.Views
{
    public sealed class PlayerTabView : View,
        INotificationHandler<PlayerJoinedNotification>,
        INotificationHandler<PlayerLeftNotification>
    {
        private readonly IMediator _mediator;

        private readonly ListView _playerListView;
        private readonly TextView _playerInfoTextView;
        private readonly Button _kickButton;
        private readonly Button _banButton;

        private readonly PlayerListDataSource _currentPlayers = new(new());
        private IPlayer? _selectedPlayer;

        public PlayerTabView(IMediator mediator, IPlayerContainerReader playerContainer)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));


            Width = Dim.Fill();
            Height = Dim.Fill();

            var onlineView = new FrameView("Online") { X = 0, Y = 0, Width = 20, Height = Dim.Fill() };
            _currentPlayers.Players.AddRange(playerContainer.GetAll().Where(x => x.Permissions > PermissionFlags.UnVerified));
            _playerListView = new ListView(_currentPlayers) { X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill() };
            onlineView.Add(_playerListView);

            var infoView = new FrameView("Info") { X = 20, Y = 0, Width = Dim.Fill(), Height = Dim.Fill() };
            _playerInfoTextView = new TextView { X = 0, Y = 0, Width = Dim.Fill(), Height = 7, ReadOnly = true };
            _kickButton = new Button("Kick") { X = 0, Y = Pos.Bottom(_playerInfoTextView), Height = 1, Visible = false };
            _banButton = new Button("Ban") { X = Pos.Right(_kickButton), Y = Pos.Bottom(_playerInfoTextView), Height = 1, Visible = false };
            infoView.Add(_playerInfoTextView, _kickButton, _banButton);

            _playerListView.OpenSelectedItem += args =>
            {
                if (args.Value is IPlayer player)
                {
                    _selectedPlayer = player;

                    _kickButton.Visible = true;
                    _banButton.Visible = true;

                    _playerInfoTextView.Text = @$"
Connection Id: {player.ConnectionId}
Id: {player.Id}
Origin: {player.Origin}
Name: {player.Name}
Permissions: {player.Permissions}
IP: {player.IPEndPoint}";
                }
                else
                {
                    RemovePlayerInfo();
                }
            };
            _kickButton.Clicked += () =>
            {
                if (_selectedPlayer is not null)
                    Terminal.Gui.Application.Run(Kick(_selectedPlayer));
            };
            _banButton.Clicked += () =>
            {
                if (_selectedPlayer is not null)
                    Terminal.Gui.Application.Run(Ban(_selectedPlayer));
            };

            Add(onlineView, infoView);
        }

        private Dialog Kick(IPlayer player)
        {
            var dialog = new Dialog("Kick", 40, 6);
            var reasonInfoFrameView = new FrameView("Write a reason for kicking:") { X = 0, Y = 0, Width = Dim.Fill(), Height = 3 };
            var reasonTextField = new TextField { X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill() };
            reasonInfoFrameView.Add(reasonTextField);
            var kick = new Button("Kick") { X = 0, Y = Pos.AnchorEnd() - 1 };
            var cancel = new Button("Cancel") { X = Pos.AnchorEnd() - "Cancel".Length - 4, Y = Pos.AnchorEnd() - 1 };
            kick.Clicked += async () =>
            {
                var reason = ustring.IsNullOrEmpty(reasonTextField.Text) ? "Kicked by a Moderator or Admin." : reasonTextField.Text.ToString() ?? string.Empty;
                RemovePlayerInfo();
                await _mediator.Send(new KickPlayerCommand(player, reason), CancellationToken.None);
                Terminal.Gui.Application.RequestStop();
            };
            cancel.Clicked += () => Terminal.Gui.Application.RequestStop();
            dialog.Add(reasonInfoFrameView, kick, cancel);
            return dialog;
        }
        private Dialog Ban(IPlayer player)
        {
            var dialog = new Dialog("Ban", 40, 11);
            var reasonInfoFrameView = new FrameView("Write a reason for banning:") { X = 0, Y = 0, Width = Dim.Fill(), Height = 3 + 2 };
            var reasonTextView = new ComboBox("Select a reason for banning") { X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill() };
            reasonTextView.Source = new ListWrapper(new[] { "Test1", "Test2", "Test3", "Test4" });
            reasonInfoFrameView.Add(reasonTextView);
            var durationInfoFrameView = new FrameView("Duration in minutes:") { X = 0, Y = Pos.Bottom(reasonInfoFrameView), Width = Dim.Fill(), Height = 3 };
            var durationTextField = new TextField { X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill() };
            durationTextField.TextChanged += text =>
            {
                if (!ulong.TryParse(durationTextField.Text.ToString(), out _))
                    durationTextField.Text = ustring.Empty;
            };
            durationInfoFrameView.Add(durationTextField);

            var ban = new Button("Ban") { X = 0, Y = Pos.AnchorEnd() - 1 };
            var cancel = new Button("Cancel") { X = Pos.AnchorEnd() - "Cancel".Length - 4, Y = Pos.AnchorEnd() - 1 };
            ban.Clicked += async () =>
            {
                var reason = ustring.IsNullOrEmpty(reasonTextView.Text) ? "Kicked by a Moderator or Admin." : reasonTextView.Text.ToString() ?? string.Empty;
                RemovePlayerInfo();
                await _mediator.Send(new KickPlayerCommand(player, reason), CancellationToken.None);
                Terminal.Gui.Application.RequestStop();
            };
            cancel.Clicked += () => Terminal.Gui.Application.RequestStop();
            dialog.Add(ban, cancel, reasonInfoFrameView, durationInfoFrameView);
            return dialog;
        }

        private void RemovePlayerInfo()
        {
            _selectedPlayer = null;

            _kickButton.Visible = false;
            _banButton.Visible = false;

            _playerInfoTextView.Text = ustring.Empty;
        }

        public Task Handle(PlayerJoinedNotification notification, CancellationToken ct)
        {
            Terminal.Gui.Application.MainLoop.Invoke(() =>
            {
                _currentPlayers.Players.Add(notification.Player);
                _playerListView.Source = _currentPlayers;
            });
            return Task.CompletedTask;
        }

        public Task Handle(PlayerLeftNotification notification, CancellationToken ct)
        {
            Terminal.Gui.Application.MainLoop.Invoke(() =>
            {
                _currentPlayers.Players.RemoveAll(x => x.Id == notification.Id);
                _playerListView.Source = _currentPlayers;
            });
            return Task.CompletedTask;
        }
    }
}