using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LegacyWrapper.Common.Attributes;
using LegacyWrapper.Common.Serialization;
using LegacyWrapper.Common.Token;
using LegacyWrapper.ErrorHandling;
using LegacyWrapperClient.Architecture;
using LegacyWrapperClient.Client;
using LegacyWrapperClient.Configuration;
using LegacyWrapperClient.ProcessHandling;
using LegacyWrapperClient.Token;
using PommaLabs.Thrower;

namespace LegacyWrapperClient.Transport
{
    internal class PipeConnector : IPipeConnector
    {
        private const string LocalPipeUrl = ".";

        private bool _isDisposed;

        private NamedPipeClientStream _pipe;

        private readonly IFormatter _formatter;
        private readonly IWrapperProcessStarter _wrapperProcessStarter;
        private readonly PipeToken _pipeToken;

        /// <summary>
        /// Creates a new WrapperClient instance.
        /// </summary>
        /// <param name="formatter">Formatter instance for data serialization to the pipe.</param>
        /// <param name="wrapperProcessStarter">WrapperProcessStarter instance for invoking the appropriate wrapper executable.</param>
        /// <param name="pipeToken">PipeToken instance for creating pipe connections.</param>
        public PipeConnector(IFormatter formatter, IWrapperProcessStarter wrapperProcessStarter, PipeToken pipeToken)
        {
            Raise.ArgumentNullException.IfIsNull(formatter, nameof(formatter));
            Raise.ArgumentNullException.IfIsNull(wrapperProcessStarter, nameof(wrapperProcessStarter));
            Raise.ArgumentNullException.IfIsNull(pipeToken, nameof(pipeToken));
            
            _formatter = formatter;
            _wrapperProcessStarter = wrapperProcessStarter;
            _pipeToken = pipeToken;

            _wrapperProcessStarter.StartWrapperProcess();
            OpenPipe();
        }

        public void SendCallRequest(CallData callData)
        {
            _formatter.Serialize(_pipe, callData);
        }

        public CallResult ReceiveCallResponse()
        {
            CallResult callResult = (CallResult)_formatter.Deserialize(_pipe);

            if (callResult.Exception != null)
            {
                throw callResult.Exception;
            }

            return callResult;
        }

        private void OpenPipe()
        {
            _pipe = new NamedPipeClientStream(LocalPipeUrl, _pipeToken.Token, PipeDirection.InOut);
            _pipe.Connect();
            _pipe.ReadMode = PipeTransmissionMode.Message;
        }

        private void ClosePipe()
        {
            CallData info = new CallData { Status = KeepAliveStatus.Close };

            try
            {
                _formatter.Serialize(_pipe, info);
            }
            catch { } // This means the wrapper eventually crashed and doesn't need a clean shutdown anyways

            if (_pipe.IsConnected)
            {
                _pipe.Close();
            }
        }

        #region IDisposable-Implementation
        ~PipeConnector()
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
                ClosePipe();
                _pipe.Dispose();

                _wrapperProcessStarter.Dispose();
            }

            // Free any unmanaged objects here.

            _isDisposed = true;
        }
        #endregion

    }

}
