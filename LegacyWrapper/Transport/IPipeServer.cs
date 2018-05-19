using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LegacyWrapper.Common.Serialization;

namespace LegacyWrapper.Transport
{
    interface IPipeServer : IDisposable
    {
        void SendCallResponse(CallResult callResult);
        CallData ReceiveCallRequest();
    }
}
