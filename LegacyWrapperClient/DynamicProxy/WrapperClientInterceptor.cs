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
    internal class WrapperClientInterceptor : IInterceptor, IDisposable
    {
        private readonly WrapperClient _wrapperClient;

        public WrapperClientInterceptor(string libraryName, TargetArchitecture targetArchitecture)
        {
            _wrapperClient = new WrapperClient(libraryName, targetArchitecture);
        }

        public void Intercept(IInvocation invocation)
        {
            string methodName = invocation.Method.Name;
            object[] parameters = invocation.Arguments;
            Type[] parameterTypes = invocation.Method.GetParameters().Select(x => x.ParameterType).ToArray();
            Type returnType = invocation.Method.ReturnType;

            if (invocation.Method.CustomAttributes.Count() != 1)
            {
                throw new LegacyWrapperException($"Interface method {methodName} must contain exactly one [LegacyDllImport] attribute!");
            }

            var attribute = (LegacyDllImportAttribute)invocation.Method.GetCustomAttributes(typeof(LegacyDllImportAttribute), false).Single();

            invocation.ReturnValue = _wrapperClient.InvokeInternal(methodName, parameters, parameterTypes, returnType, attribute);
        }

        #region IDisposable-Implementation
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _wrapperClient?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~WrapperClientInterceptor()
        {
            Dispose(false);
        }
        #endregion
    }
}
