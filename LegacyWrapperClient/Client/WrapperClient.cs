using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LegacyWrapper.Common.Serialization;
using LegacyWrapperClient.Transport;
using PommaLabs.Thrower;

namespace LegacyWrapperClient.Client
{
    internal class WrapperClient : IDisposable
    {
        private readonly IPipeConnector _pipeConnector;

        private bool _isDisposed = false;

        public WrapperClient(IPipeConnector pipeConnector)
        {
            Raise.ArgumentNullException.IfIsNull(pipeConnector, nameof(pipeConnector));

            _pipeConnector = pipeConnector;
        }

        /// <summary>
        /// Executes a call to a library.
        /// </summary>
        /// <param name="callData"><see cref="CallData">CallData</see> object with information about invocation.</param>
        /// <returns>Result object returned by the library.</returns>
        /// <exception cref="Exception">This Method will rethrow all exceptions thrown by the wrapper.</exception>
        protected internal virtual object InvokeInternal(CallData callData)
        {
            _pipeConnector.SendCallRequest(callData);

            CallResult callResult = _pipeConnector.ReceiveCallResponse();

            CopyParameters(callResult, callData);

            return callResult.Result;
        }

        private void CopyParameters(CallResult callResult, CallData callData)
        {
            string errorMessage = "Returned parameters differ in length from passed parameters";
            Raise.InvalidDataException.If(callData.Parameters.Length != callResult.Parameters.Length, errorMessage);

            Array.Copy(callResult.Parameters, callData.Parameters, callResult.Parameters.Length);
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
                _pipeConnector.Dispose();
            }

            // Free any unmanaged objects here.

            _isDisposed = true;
        }
        #endregion

    }
}
