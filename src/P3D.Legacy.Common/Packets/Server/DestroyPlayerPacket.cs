﻿namespace P3D.Legacy.Common.Packets.Server
{
    public sealed record DestroyPlayerPacket() : P3DPacket(P3DPacketType.DestroyPlayer)
    {
        public Origin PlayerId { get => DataItemStorage.GetOrigin(0); init => DataItemStorage.Set(0, value); }

        public void Deconstruct(out Origin playerId)
        {
            playerId = PlayerId;
        }
    }
}