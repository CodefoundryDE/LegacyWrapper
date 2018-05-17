using System;
using LegacyWrapper.Common.Serialization;

namespace LegacyWrapperClient.Client
{
    internal interface IPipeConnector : IDisposable
    {
        void SendCallRequest(CallData callData);
        CallResult ReceiveCallResponse();
    }
}