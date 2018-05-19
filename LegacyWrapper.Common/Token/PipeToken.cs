using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PommaLabs.Thrower;

namespace LegacyWrapper.Common.Token
{
    public class PipeToken
    {
        public string Token { get; }

        public PipeToken(string token)
        {
            Raise.ArgumentException.IfIsNullOrWhiteSpace(token, nameof(token));

            Token = token;
        }
    }
}
