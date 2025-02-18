﻿using FluentValidation;

using System;

namespace P3D.Legacy.Server.Infrastructure.Options
{
    public sealed class LockoutOptionsValidator : AbstractValidator<LockoutOptions>
    {
        public LockoutOptionsValidator()
        {
            RuleFor(x => x.MaxFailedAccessAttempts).NotEmpty();
            RuleFor(x => x.DefaultLockoutTimeSpan).NotEmpty();
        }
    }

    public record LockoutOptions
    {
        public int MaxFailedAccessAttempts { get; init; } = 5;

        public TimeSpan DefaultLockoutTimeSpan { get; init; } = TimeSpan.FromMinutes(5);
    }
}