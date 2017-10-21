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
    internal class WrapperClientInterceptor : IInterceptor
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

            var dllImportAttributes = ExtractAttributesFromType<LegacyDllImportAttribute>(_interfaceType);
            if (dllImportAttributes.Length != 1)
            {
                throw new LegacyWrapperException();
            }
            LegacyDllImportAttribute dllImportAttribute = dllImportAttributes[0];

            var dllMethodAttributes = ExtractAttributesFromType<LegacyDllMethodAttribute>(invocation.Method);
            if (dllMethodAttributes.Length != 1)
            {
                throw new LegacyWrapperException();
            }
            LegacyDllMethodAttribute dllMethodAttribute = dllMethodAttributes[0];

            string libraryName = dllImportAttribute.LibraryName;
            if (OverrideLibraryName != null)
            {
                libraryName = OverrideLibraryName;
            }

            invocation.ReturnValue = _wrapperClient.InvokeInternal(libraryName, methodName, parameters, parameterTypes, returnType, dllMethodAttribute);
        }

        private static T[] ExtractAttributesFromType<T>(ICustomAttributeProvider attributeProvider)
        {
            return attributeProvider.GetCustomAttributes(typeof(T), false)
                .Cast<T>()
                .ToArray();
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
