﻿using System;
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
using LegacyWrapper.Common.Serialization;
using LegacyWrapperClient.Architecture;

namespace LegacyWrapperClient.Client
{
    public class WrapperClient : IDisposable
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
        /// <param name="libraryName">Name of the library to load.</param>
        /// <param name="targetArchitecture">Architecture of the library to load (X86 / AMD64). Defaults to X86.</param>
        public WrapperClient(string libraryName, TargetArchitecture targetArchitecture = TargetArchitecture.X86)
        {
            string token = Guid.NewGuid().ToString();

            string wrapperName = WrapperNames[targetArchitecture];
            // Pass token and library name to child process
            _wrapperProcess = Process.Start(wrapperName, $"{token} {libraryName}");

            _formatter = new BinaryFormatter();

            _pipe = new NamedPipeClientStream(".", token, PipeDirection.InOut);
            _pipe.Connect();
            _pipe.ReadMode = PipeTransmissionMode.Message;
        }

        /// <summary>
        /// Executes a call to a library.
        /// </summary>
        /// <typeparam name="T">Delegate Type to call.</typeparam>
        /// <param name="function">Name of the function to call.</param>
        /// <param name="args">Array of args to pass to the function.</param>
        /// <returns>Result object returned by the library.</returns>
        /// <exception cref="Exception">This Method will rethrow all exceptions thrown by the wrapper.</exception>
        public object Invoke<T>(string function, object[] args) where T : class
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(WrapperClient));
            }

            if (!typeof(T).IsSubclassOf(typeof(Delegate)))
            {
                throw new ArgumentException("Type parameter must be a delegate type.", nameof(T));
            }

            var info = new CallData
            {
                ProcedureName = function,
                Parameters = args,
                Delegate = typeof(T),
            };

            // Write request to server
            _formatter.Serialize(_pipe, info);

            // Receive result from server
            CallResult callResult = (CallResult)_formatter.Deserialize(_pipe);

            if (callResult.Exception != null)
            {
                throw callResult.Exception;
            }

            // Exchange ref params
            if (args.Length != callResult.Parameters.Length)
            {
                throw new InvalidDataException("Returned parameters differ in length from passed parameters");
            }

            for (int i = 0; i < args.Length; i++)
            {
                args[i] = callResult.Parameters[i];
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
