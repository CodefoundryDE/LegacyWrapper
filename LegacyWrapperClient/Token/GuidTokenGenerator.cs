using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegacyWrapperClient.Token
{
    internal class GuidTokenGenerator : ITokenGenerator
    {
        public string GenerateToken()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
