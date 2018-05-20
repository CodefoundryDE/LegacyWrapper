using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegacyWrapperClient.Transport
{
    internal interface ITransportImplementation
    {
        void Connect();
    }
}
