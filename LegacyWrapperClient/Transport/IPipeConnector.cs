using System;
using LegacyWrapper.Common.Serialization;

namespace LegacyWrapperClient.Transport
{
    internal interface IPipeConnector : IDisposable
    {
        void SendCallRequest(CallData callData);
        CallResult ReceiveCallResponse();
    }
}