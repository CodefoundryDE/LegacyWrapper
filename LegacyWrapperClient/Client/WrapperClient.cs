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
    internal class WrapperClient : IDisposable
    {
        private const string LocalPipeUrl = ".";

        private bool _isDisposed;

        private readonly NamedPipeClientStream _pipe;
        private readonly Process _wrapperProcess;

        private readonly IReadOnlyDictionary<TargetArchitecture, string> WrapperNames = new Dictionary<TargetArchitecture, string>
        {
            { TargetArchitecture.X86,   "Codefoundry.LegacyWrapper32.exe" },
            { TargetArchitecture.Amd64, "Codefoundry.LegacyWrapper64.exe" },
        };

        private readonly IFormatter _formatter;

        /// <summary>
        /// Creates a new WrapperClient instance.
        /// </summary>
        /// <param name="configuration">WrapperConfiguration object holding configuration info.</param>
        /// <param name="formatter">Formatter instance for data serialization to the pipe.</param>
        /// <param name="tokenGenerator">ITokenGenerator instance for generating connection tokens.</param>
        public WrapperClient(IWrapperConfig configuration, IFormatter formatter, ITokenGenerator tokenGenerator)
        {
            Raise.ArgumentNullException.IfIsNull(configuration, nameof(configuration));
            Raise.ArgumentNullException.IfIsNull(formatter, nameof(formatter));

            _formatter = formatter;

            string token = tokenGenerator.GenerateToken();

            string wrapperName = WrapperNames[configuration.TargetArchitecture];

            _wrapperProcess = Process.Start(wrapperName, token);

            _pipe = new NamedPipeClientStream(LocalPipeUrl, token, PipeDirection.InOut);
            _pipe.Connect();
            _pipe.ReadMode = PipeTransmissionMode.Message;
        }

        /// <summary>
        /// Executes a call to a library.
        /// </summary>
        /// <param name="callData"><see cref="CallData">CallData</see> object with information about invocation.</param>
        /// <returns>Result object returned by the library.</returns>
        /// <exception cref="Exception">This Method will rethrow all exceptions thrown by the wrapper.</exception>
        internal object InvokeInternal(CallData callData)
        {
            Raise.ObjectDisposedException.If(_isDisposed, nameof(WrapperClient));

            // Write request to server
            SendCallRequest(callData);

            CallResult callResult = ReceiveCallResponse();

            CopyParameters(callResult, callData);

            return callResult.Result;
        }

        private void CopyParameters(CallResult callResult, CallData callData)
        {
            string errorMessage = "Returned parameters differ in length from passed parameters";
            Raise.InvalidDataException.If(callData.Parameters.Length != callResult.Parameters.Length, errorMessage);

            Array.Copy(callResult.Parameters, callData.Parameters, callResult.Parameters.Length);
        }

        private void SendCallRequest(CallData callData)
        {
            _formatter.Serialize(_pipe, callData);
        }

        private CallResult ReceiveCallResponse()
        {
            CallResult callResult = (CallResult)_formatter.Deserialize(_pipe);

            if (callResult.Exception != null)
            {
                throw callResult.Exception;
            }

            return callResult;
        }

        /// <summary>
        /// Gracefully close connection to server
        /// </summary>
        protected virtual void Close()
        {
            var info = new CallData { Status = KeepAliveStatus.Close };

            try
            {
                _formatter.Serialize(_pipe, info);
            }
            catch { } // This means the wrapper eventually crashed and doesn't need a clean shutdown anyways

            if (_pipe.IsConnected)
            {
                _pipe.Close();
            }

            if (!_wrapperProcess.HasExited)
            {
                _wrapperProcess.Close();
            }
        }

        private string CreateToken()
        {
            return Guid.NewGuid().ToString();
        }

        #region IDisposable-Implementation
        ~WrapperClient()
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
                Close();
                _pipe.Dispose();
                _wrapperProcess.Dispose();
            }

            // Free any unmanaged objects here.

            _isDisposed = true;
        }
        #endregion

    }

}
