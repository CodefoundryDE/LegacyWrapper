using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using LegacyWrapper.Common.Attributes;
using LegacyWrapper.Common.Serialization;
using LegacyWrapper.ErrorHandling;
using LegacyWrapperClient.Architecture;
using LegacyWrapperClient.Client;
using LegacyWrapperClient.Configuration;
using PommaLabs.Thrower;

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

        public WrapperClientInterceptor(Type interfaceType, IWrapperConfig configuration)
        {
            _wrapperClient = new WrapperClient(configuration);
            _interfaceType = interfaceType;
        }

        public void Intercept(IInvocation invocation)
        {
            Raise.ObjectDisposedException.If(_isDisposed, nameof(WrapperClientInterceptor));

            // Early out if it'a call to Dispose()
            if (invocation.Method.Name == nameof(IDisposable.Dispose))
            {
                Dispose();
                return;
            }

            Type[] parameterTypes = invocation.Method.GetParameters().Select(x => x.ParameterType).ToArray();
            Type returnType = invocation.Method.ReturnType;

            var dllImportAttribute = GetLegacyAttribute<LegacyDllImportAttribute>(_interfaceType);
            var dllMethodAttribute = GetLegacyAttribute<LegacyDllMethodAttribute>(invocation.Method);

            string libraryName = dllImportAttribute.LibraryName;
            if (OverrideLibraryName != null)
            {
                libraryName = OverrideLibraryName;
            }

            var callData = new CallData
            {
                LibraryName = libraryName,
                ProcedureName = invocation.Method.Name,
                Parameters = invocation.Arguments,
                ParameterTypes = parameterTypes,
                ReturnType = returnType,
                CallingConvention = dllMethodAttribute.CallingConvention,
                CharSet = dllMethodAttribute.CharSet,
            };

            invocation.ReturnValue = _wrapperClient.InvokeInternal(callData);
        }

        private static T GetLegacyAttribute<T>(MemberInfo attributeProvider) where T : Attribute
        {
            var dllImportAttributes = attributeProvider.GetCustomAttributes(typeof(T), false)
                .Cast<T>()
                .ToArray();

            Raise<LegacyWrapperException>.IfNot(dllImportAttributes.Length == 1, $"{attributeProvider.Name} must contain exactly one {typeof(T).Name}");

            return dllImportAttributes[0];
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
