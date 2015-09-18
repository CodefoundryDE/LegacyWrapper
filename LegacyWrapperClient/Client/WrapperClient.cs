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
using LegacyWrapper.Dll;

namespace LegacyWrapperClient.Client
{
    public class WrapperClient : IDisposable
    {
        private bool _disposed;
        private NamedPipeClientStream _pipe;

        public WrapperClient()
        {
            string token = Guid.NewGuid().ToString();

            // Pass token to child process
            Process.Start("LegacyWrapper", token);

            _pipe = new NamedPipeClientStream(".", token, PipeDirection.InOut);
            _pipe.Connect();
            _pipe.ReadMode = PipeTransmissionMode.Message;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dllName"></param>
        /// <param name="function"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public object Call<T>(string dllName, string function, object[] args) where T : class
        {
            var info = new CallData
            {
                Library = dllName,
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
