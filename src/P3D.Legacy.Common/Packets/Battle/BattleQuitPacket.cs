﻿namespace P3D.Legacy.Common.Packets.Battle
{
    public sealed record BattleQuitPacket() : P3DPacket(P3DPacketType.BattleQuit)
    {
        public Origin DestinationPlayerOrigin { get => DataItemStorage.GetOrigin(0); init => DataItemStorage.Set(0, value); }

        public void Deconstruct(out Origin destinationPlayerOrigin)
        {
            destinationPlayerOrigin = DestinationPlayerOrigin;
        }
    }
}