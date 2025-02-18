﻿using System.Collections.Generic;
using System.Linq;

namespace P3D.Legacy.Server.Infrastructure.Models
{
    public class AccountResult
    {
        public static AccountResult Success { get; } = new() { Succeeded = true };
        public static AccountResult Failed(params GenericError[] errors)
        {
            var result = new AccountResult { Succeeded = false };
            if (errors is not null)
            {
                result._errors.AddRange(errors);
            }
            return result;
        }

        private readonly List<GenericError> _errors = new();

        public bool Succeeded { get; protected set; }
        public IEnumerable<GenericError> Errors => _errors;

        public override string ToString() => Succeeded
            ? "Succeeded"
            : string.Format("{0} : {1}", "Failed", string.Join(",", Errors.Select(x => x.Code).ToList()));
    }
}