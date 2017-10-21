using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using LegacyWrapper.Common.Attributes;
using LegacyWrapper.ErrorHandling;
using LegacyWrapperClient.Architecture;
using LegacyWrapperClient.Client;

namespace LegacyWrapperClient.DynamicProxy
{
    /// <summary>
    /// Interceptor to receive calls to a proxy generated from an interface.
    /// </summary>
    internal class WrapperClientInterceptor : IInterceptor, IDisposable
    {
        /// <summary>
        /// This is an internal Property that is used for testing purposes.
        /// </summary>
        internal static string OverrideLibraryName = null;

        private bool _isDisposed = false;

        private readonly WrapperClient _wrapperClient;
        private readonly Type _interfaceType;

        public WrapperClientInterceptor(Type interfaceType, TargetArchitecture targetArchitecture)
        {
            _wrapperClient = new WrapperClient(targetArchitecture);
            _interfaceType = interfaceType;
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

            var dllImportAttribute = GetLegacyAttribute<LegacyDllImportAttribute>(_interfaceType, $"{_interfaceType.Name} must contain exactly one [LegacyDllImport] attribute.");
            var dllMethodAttribute = GetLegacyAttribute<LegacyDllMethodAttribute>(invocation.Method, $"{invocation.Method.Name} must contain exactlly one [LegacyDllMethod] attribute.");

            string libraryName = dllImportAttribute.LibraryName;
            if (OverrideLibraryName != null)
            {
                libraryName = OverrideLibraryName;
            }

            invocation.ReturnValue = _wrapperClient.InvokeInternal(libraryName, methodName, parameters, parameterTypes, returnType, dllMethodAttribute);
        }

        private static T GetLegacyAttribute<T>(ICustomAttributeProvider attributeProvider, string message)
        {
            var dllImportAttributes = attributeProvider.GetCustomAttributes(typeof(T), false)
                .Cast<T>()
                .ToArray();

            if (dllImportAttributes.Length != 1)
            {
                throw new LegacyWrapperException(message);
            }
            return dllImportAttributes[0];
        }

        private void AssertIsNotDesposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(_wrapperClient));
            }
        }

        #region IDisposable-Pattern
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
                _wrapperClient.Dispose();
                // Free any other managed objects here.
                //
            }

            // Free any unmanaged objects here.
            //
            _isDisposed = true;
        }

        ~WrapperClientInterceptor()
        {
            Dispose(false);
        }
        #endregion
    }
}
