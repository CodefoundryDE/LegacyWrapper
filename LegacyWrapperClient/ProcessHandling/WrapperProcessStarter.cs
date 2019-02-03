using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LegacyWrapper.Common.Token;
using LegacyWrapperClient.Configuration;
using PommaLabs.Thrower;

namespace LegacyWrapperClient.ProcessHandling
{
    internal class WrapperProcessStarter : IWrapperProcessStarter
    {
        private bool _isDisposed;

        private MockableProcess _wrapperProcess;

        private readonly IWrapperExecutableNameProvider _wrapperExecutableNameProvider;
        private readonly PipeToken _pipeToken;
        private readonly IProcessFactory _processFactory;

        /// <summary>
        /// Creates a new WrapperProcessStarter instance.
        /// </summary>
        /// <param name="wrapperExecutableNameProvider">Provides the name for the wrapper executable to start.</param>
        /// <param name="pipeToken">PipeToken instance for creating pipe connections.</param>
        /// <param name="processFactory">IProcessFactory instance for creating a new wrapper process</param>
        public WrapperProcessStarter(IWrapperExecutableNameProvider wrapperExecutableNameProvider, PipeToken pipeToken, IProcessFactory processFactory)
        {
            Raise.ArgumentNullException.IfIsNull(wrapperExecutableNameProvider, nameof(wrapperExecutableNameProvider));
            Raise.ArgumentNullException.IfIsNull(pipeToken, nameof(pipeToken));
            Raise.ArgumentNullException.IfIsNull(processFactory);

            _wrapperExecutableNameProvider = wrapperExecutableNameProvider;
            _pipeToken = pipeToken;
            _processFactory = processFactory;
        }

        public void StartWrapperProcess()
        {
            string wrapperName = _wrapperExecutableNameProvider.GetWrapperExecutableName();

            _wrapperProcess = _processFactory.GetProcess(wrapperName, _pipeToken.Token);
            _wrapperProcess.Start();
        }

        private void StopWrapperProcess()
        {
            if (!_wrapperProcess.HasExited) 
            {
                _wrapperProcess.Close();
            }
        }

        #region IDisposable-Implementation
        ~WrapperProcessStarter()
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
                StopWrapperProcess();
                _wrapperProcess.Dispose();
            }

            // Free any unmanaged objects here.

            _isDisposed = true;
        }
        #endregion
    }
}
