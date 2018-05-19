using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LegacyWrapper.Common.Token;
using LegacyWrapperClient.Configuration;
using PommaLabs.Thrower;

namespace LegacyWrapperClient.Client
{
    internal class WrapperProcessStarter : IWrapperProcessStarter
    {
        private bool _isDisposed;

        private Process _wrapperProcess;

        private readonly IWrapperConfig _configuration;
        private readonly IWrapperExecutableNameProvider _wrapperExecutableNameProvider;
        private readonly PipeToken _pipeToken;

        /// <summary>
        /// Creates a new WrapperProcessStarter instance.
        /// </summary>
        /// <param name="wrapperExecutableNameProvider">Provides the name for the wrapper executable to start.</param>
        /// <param name="configuration">WrapperConfiguration object holding configuration info.</param>
        /// <param name="pipeToken">PipeToken instance for creating pipe connections.</param>
        public WrapperProcessStarter(IWrapperExecutableNameProvider wrapperExecutableNameProvider, IWrapperConfig configuration, PipeToken pipeToken)
        {
            Raise.ArgumentNullException.IfIsNull(wrapperExecutableNameProvider, nameof(wrapperExecutableNameProvider));
            Raise.ArgumentNullException.IfIsNull(configuration, nameof(configuration));
            Raise.ArgumentNullException.IfIsNull(pipeToken, nameof(pipeToken));

            _wrapperExecutableNameProvider = wrapperExecutableNameProvider;
            _configuration = configuration;
            _pipeToken = pipeToken;
        }

        public void StartWrapperProcess()
        {
            string wrapperName = _wrapperExecutableNameProvider.GetWrapperExecutableName(_configuration);

            _wrapperProcess = Process.Start(wrapperName, _pipeToken.Token);
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
