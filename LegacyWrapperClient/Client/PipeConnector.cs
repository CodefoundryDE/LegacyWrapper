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
using LegacyWrapper.ErrorHandling;
using LegacyWrapperClient.Architecture;
using LegacyWrapperClient.Configuration;
using LegacyWrapperClient.Token;
using PommaLabs.Thrower;

namespace LegacyWrapperClient.Client
{
    internal class PipeConnector : IPipeConnector
    {
        private const string LocalPipeUrl = ".";

        private bool _isDisposed;

        private NamedPipeClientStream _pipe;
        private Process _wrapperProcess;

        private readonly IWrapperConfig _configuration;
        private readonly IFormatter _formatter;
        private readonly string _token;
        private readonly IWrapperExecutableNameProvider _wrapperExecutableNameProvider;

        /// <summary>
        /// Creates a new WrapperClient instance.
        /// </summary>
        /// <param name="configuration">WrapperConfiguration object holding configuration info.</param>
        /// <param name="formatter">Formatter instance for data serialization to the pipe.</param>
        /// <param name="tokenGenerator">ITokenGenerator instance for generating connection tokens.</param>
        /// <param name="wrapperExecutableNameProvider">Provides the name for the wrapper executable to start.</param>
        public PipeConnector(IWrapperConfig configuration, IFormatter formatter, ITokenGenerator tokenGenerator, IWrapperExecutableNameProvider wrapperExecutableNameProvider)
        {
            Raise.ArgumentNullException.IfIsNull(configuration, nameof(configuration));
            Raise.ArgumentNullException.IfIsNull(formatter, nameof(formatter));
            Raise.ArgumentNullException.IfIsNull(tokenGenerator, nameof(tokenGenerator));
            Raise.ArgumentNullException.IfIsNull(wrapperExecutableNameProvider, nameof(wrapperExecutableNameProvider));

            _configuration = configuration;
            _formatter = formatter;
            _token = tokenGenerator.GenerateToken();
            _wrapperExecutableNameProvider = wrapperExecutableNameProvider;

            StartWrapperProcess();
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

        private void StartWrapperProcess()
        {
            string wrapperName = _wrapperExecutableNameProvider.GetWrapperExecutableName(_configuration);

            _wrapperProcess = Process.Start(wrapperName, _token);
        }

        private void StopWrapperProcess()
        {
            if (!_wrapperProcess.HasExited)
            {
                _wrapperProcess.Close();
            }
        }

        private void OpenPipe()
        {
            _pipe = new NamedPipeClientStream(LocalPipeUrl, _token, PipeDirection.InOut);
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
                StopWrapperProcess();
                _pipe.Dispose();
                _wrapperProcess.Dispose();
            }

            // Free any unmanaged objects here.

            _isDisposed = true;
        }
        #endregion

    }

}
