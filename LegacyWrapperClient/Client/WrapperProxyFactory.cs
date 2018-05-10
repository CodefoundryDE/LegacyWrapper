using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using LegacyWrapperClient.Architecture;
using LegacyWrapperClient.Configuration;
using LegacyWrapperClient.DynamicProxy;
using PommaLabs.Thrower;

namespace LegacyWrapperClient.Client
{

    /// <summary>
    /// WrapperProxyFactory provides a method to generate a new proxy instance to the wrapper executable.
    /// </summary>
    /// <typeparam name="TFunctions">Interface type to use for calls to the DLL.</typeparam>
    public static class WrapperProxyFactory<TFunctions> where TFunctions : class, IDisposable
    {
        /// <summary>
        /// Creates a new instance of TFunctions.
        /// All calls will be proxied over a named pipe to the wrapper executable.
        /// </summary>
        /// <param name="configuration">WrapperConfiguration object holding configuration info.</param>
        /// <exception cref="ArgumentException">An ArgumentException is thrown if the supplied generic type parameter is not an interface.</exception>
        /// <returns>Returns a new instance of TFunctions.</returns>
        public static TFunctions CreateWrapperProxy(IWrapperConfig configuration)
        {
            Raise.ArgumentException.If(!typeof(TFunctions).IsInterface, nameof(TFunctions), "Generic parameter type <TFunctions> must be an interface.");
            
            IProxyGenerator generator = new ProxyGenerator(new DefaultProxyBuilder());
            IInterceptor interceptor = new WrapperClientInterceptor(typeof(TFunctions), configuration);

            return generator.CreateInterfaceProxyWithoutTarget<TFunctions>(interceptor);
        }
    }
}
