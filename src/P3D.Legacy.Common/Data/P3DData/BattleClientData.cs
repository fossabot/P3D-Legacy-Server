﻿using System;

namespace P3D.Legacy.Common.Data.P3DData
{
    public sealed record BattleClientData : P3DData
    {
        public string Action { get; }
        public string ActionValue { get; }

        public BattleClientData(in ReadOnlySpan<char> data) : base(in data)
        {
            var idxAction = data.IndexOf('|');
            Action = data.Slice(0, idxAction).ToString();
            ActionValue = data.Slice(idxAction + 1).ToString();
        }

        public override string ToP3DString() => $"{Action}|{ActionValue}";
    }
}