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
        private bool _isDisposed = false;

        private readonly WrapperClient _wrapperClient;
        private readonly Type _interfaceType;
        private readonly ILibraryNameProvider _libraryNameProvider;

        public WrapperClientInterceptor(Type interfaceType, WrapperClient wrapperClient, ILibraryNameProvider libraryNameProvider)
        {
            Raise.ArgumentNullException.IfIsNull(interfaceType, nameof(interfaceType));
            Raise.ArgumentNullException.IfIsNull(wrapperClient, nameof(wrapperClient));
            Raise.ArgumentNullException.IfIsNull(libraryNameProvider, nameof(libraryNameProvider));

            _interfaceType = interfaceType;
            _wrapperClient = wrapperClient;
            _libraryNameProvider = libraryNameProvider;
        }

        public void Intercept(IInvocation invocation)
        {
            Raise.ObjectDisposedException.If(_isDisposed, nameof(WrapperClientInterceptor));

            // Early out if it's a call to Dispose()
            if (invocation.Method.Name == nameof(IDisposable.Dispose))
            {
                Dispose();
                return;
            }

            Type[] parameterTypes = GetParameterTypesFromInvocation(invocation);
            Type returnType = invocation.Method.ReturnType;

            LegacyDllImportAttribute dllImportAttribute = GetLegacyAttribute<LegacyDllImportAttribute>(_interfaceType);
            LegacyDllMethodAttribute dllMethodAttribute = GetLegacyAttribute<LegacyDllMethodAttribute>(invocation.Method);

            string libraryName = _libraryNameProvider.GetLibraryName(dllImportAttribute);

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

        private Type[] GetParameterTypesFromInvocation(IInvocation invocation)
        {
            return invocation.Method
                .GetParameters()
                .Select(x => x.ParameterType)
                .ToArray();
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
