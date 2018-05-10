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
using LegacyWrapperClient.Architecture;
using LegacyWrapperClient.Configuration;
using PommaLabs.Thrower;

namespace LegacyWrapperClient.Client
{
    internal class WrapperClient : IDisposable
    {
        private bool _disposed;
        private readonly IFormatter _formatter;
        private readonly NamedPipeClientStream _pipe;
        private readonly Process _wrapperProcess;

        private readonly IReadOnlyDictionary<TargetArchitecture, string> WrapperNames = new Dictionary<TargetArchitecture, string>
        {
            { TargetArchitecture.X86,   "Codefoundry.LegacyWrapper32.exe" },
            { TargetArchitecture.Amd64, "Codefoundry.LegacyWrapper64.exe" },
        };

        /// <summary>
        /// Creates a new WrapperClient instance.
        /// </summary>
        /// <param name="configuration">WrapperConfiguration object holding configuration info.</param>
        public WrapperClient(IWrapperConfig configuration)
        {
            string token = Guid.NewGuid().ToString();

            string wrapperName = WrapperNames[configuration.TargetArchitecture];
            // Pass token and library name to child process
            _wrapperProcess = Process.Start(wrapperName, token);

            _formatter = new BinaryFormatter();

            _pipe = new NamedPipeClientStream(".", token, PipeDirection.InOut);
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
            Raise.ObjectDisposedException.If(_disposed, nameof(WrapperClient));
            
            // Write request to server
            _formatter.Serialize(_pipe, callData);

            // Receive result from server
            CallResult callResult = (CallResult)_formatter.Deserialize(_pipe);

            if (callResult.Exception != null)
            {
                throw callResult.Exception;
            }

            Raise.InvalidDataException.If(callData.Parameters.Length != callResult.Parameters.Length, "Returned parameters differ in length from passed parameters");

            for (int i = 0; i < callData.Parameters.Length; i++)
            {
                callData.Parameters[i] = callResult.Parameters[i];
            }

            return callResult.Result;
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
            if (_disposed)
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

            _disposed = true;
        }
        #endregion

    }

}
