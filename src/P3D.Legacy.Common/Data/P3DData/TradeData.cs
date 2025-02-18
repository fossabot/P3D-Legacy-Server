﻿using System;

namespace P3D.Legacy.Common.Data.P3DData
{
    public sealed record TradeData : P3DData
    {
        public string MonsterData { get; }

        public TradeData(in ReadOnlySpan<char> data) : base(in data)
        {
            MonsterData = data.ToString();
        }

        public override string ToP3DString() => MonsterData;
    }
}