using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LegacyWrapper.Common.Serialization;

namespace LegacyWrapperClient.Client
{
    public class WrapperClient : IDisposable
    {
        private bool _disposed;
        private readonly NamedPipeClientStream _pipe;

        public WrapperClient()
        {
            string token = Guid.NewGuid().ToString();

            // Pass token to child process
            Process.Start("Codefoundry.LegacyWrapper.exe", token);

            _pipe = new NamedPipeClientStream(".", token, PipeDirection.InOut);
            _pipe.Connect();
            _pipe.ReadMode = PipeTransmissionMode.Message;
        }

        /// <summary>
        /// Executes a call to a library.
        /// </summary>
        /// <typeparam name="T">Delegate Type to call.</typeparam>
        /// <param name="library">Name of the library to load.</param>
        /// <param name="function">Name of the function to call.</param>
        /// <param name="args">Array of args to pass to the function.</param>
        /// <returns>Result object returned by the library.</returns>
        /// <exception cref="Exception">This Method will rethrow all exceptions thrown by the wrapper.</exception>
        public object Invoke<T>(string library, string function, object[] args) where T : class
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(WrapperClient));

            if (!typeof(T).IsSubclassOf(typeof(Delegate)))
                throw new ArgumentException("Type parameter must be a delegate type.", nameof(T));

            var info = new CallData
            {
                Library = library,
                ProcedureName = function,
                Parameters = args,
                Delegate = typeof(T),
            };
            var formatter = new BinaryFormatter();
            // Write request to server
            formatter.Serialize(_pipe, info);

            // Receive result from server
            object result = formatter.Deserialize(_pipe);

            var exception = result as Exception;
            if (exception != null)
            {
                throw exception;
            }

            return result;
        }

        /// <summary>
        /// Gracefully close connection to server
        /// </summary>
        protected virtual void Close()
        {
            var info = new CallData { Status = KeepAliveStatus.Close };
            var formatter = new BinaryFormatter();
            formatter.Serialize(_pipe, info);

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
                return;

            if (disposing)
            {
                Close();
                _pipe.Dispose();
            }

            // Free any unmanaged objects here.

            _disposed = true;
        }
        #endregion

    }

}
