using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using LegacyWrapper.Common.Attributes;
using LegacyWrapper.ErrorHandling;
using LegacyWrapperClient.Architecture;
using LegacyWrapperClient.Client;

namespace LegacyWrapperClient.DynamicProxy
{
    internal class WrapperClientInterceptor : IInterceptor
    {
        /// <summary>
        /// This is an internal Property that is used for testing purposes.
        /// </summary>
        internal static string OverrideLibraryName = null;


        private bool _isDisposed = false;

        private readonly WrapperClient _wrapperClient;

        public WrapperClientInterceptor(TargetArchitecture targetArchitecture)
        {
            _wrapperClient = new WrapperClient(targetArchitecture);
        }

        public void Intercept(IInvocation invocation)
        {
            AssertIsNotDesposed();

            string methodName = invocation.Method.Name;

            // Early out if it'a call to Dispose()
            if (methodName == nameof(IDisposable.Dispose))
            {
                _wrapperClient.Dispose();
                _isDisposed = true;
                return;
            }

            object[] parameters = invocation.Arguments;
            Type[] parameterTypes = invocation.Method.GetParameters().Select(x => x.ParameterType).ToArray();
            Type returnType = invocation.Method.ReturnType;

            if (invocation.Method.CustomAttributes.Count() != 1)
            {
                throw new LegacyWrapperException($"Interface method {methodName} must contain exactly one [LegacyDllImport] attribute!");
            }

            var attribute = (LegacyDllImportAttribute)invocation.Method.GetCustomAttributes(typeof(LegacyDllImportAttribute), false).Single();

            if (OverrideLibraryName != null)
            {
                attribute.LibraryName = OverrideLibraryName;
            }

            invocation.ReturnValue = _wrapperClient.InvokeInternal(methodName, parameters, parameterTypes, returnType, attribute);
        }

        private void AssertIsNotDesposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(_wrapperClient));
            }
        }
    }
}
