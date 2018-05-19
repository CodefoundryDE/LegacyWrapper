using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LegacyWrapper.Common.Token;

namespace LegacyWrapperClient.Token
{
    internal class GuidTokenGenerator : ITokenGenerator
    {
        public PipeToken GenerateToken()
        {
            string token = Guid.NewGuid().ToString();
            return new PipeToken(token);
        }
    }
}
