using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using LegacyWrapper.Common.Serialization;
using LegacyWrapper.Common.Token;
using PommaLabs.Thrower;

namespace LegacyWrapper.Transport
{
    internal class PipeServer : IPipeServer
    {
        private bool _isDisposed = false;

        private readonly IFormatter _formatter;
        private readonly PipeToken _pipeToken;

        private NamedPipeServerStream _pipe;

        public PipeServer(IFormatter formatter, PipeToken pipeToken)
        {
            Raise.ArgumentNullException.IfIsNull(formatter);
            Raise.ArgumentNullException.IfIsNull(pipeToken, nameof(pipeToken));

            _formatter = formatter;
            _pipeToken = pipeToken;

            OpenPipeServer();
        }

        private void OpenPipeServer()
        {
            PipeDirection pipeDirection = PipeDirection.InOut;
            PipeTransmissionMode pipeTransmissionMode = PipeTransmissionMode.Message;
            int maxNumberOfServerInstances = 1;

            _pipe = new NamedPipeServerStream(_pipeToken.Token, pipeDirection, maxNumberOfServerInstances, pipeTransmissionMode);
            _pipe.WaitForConnection();
        }

        private void ClosePipeServer()
        {
            if (_pipe.IsConnected)
            {
                _pipe.Close();
            }
        }

        public void SendCallResponse(CallResult callResult)
        {
            _formatter.Serialize(_pipe, callResult);
        }

        public CallData ReceiveCallRequest()
        {
            CallData callData = (CallData)_formatter.Deserialize(_pipe);

            return callData;
        }

        #region IDisposable-Implementation
        ~PipeServer()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            if (disposing)
            {
                ClosePipeServer();
                _pipe.Dispose();
            }

            // Free any unmanaged objects here.

            _isDisposed = true;
        }
        #endregion
    }

}
