using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using LegacyWrapperClient.Architecture;
using LegacyWrapperClient.DynamicProxy;

namespace LegacyWrapperClient.Client
{

    /// <summary>
    /// WrapperClientFactory provides a method to generate a new proxy instance to the wrapper executable.
    /// </summary>
    /// <typeparam name="TFunctions">Interface type to use for calls to the DLL.</typeparam>
    public static class WrapperClientFactory<TFunctions> where TFunctions : class, IDisposable
    {
        /// <summary>
        /// Creates a new instance of TFunctions.
        /// All calls will be proxied over a named pipe to the wrapper executable.
        /// </summary>
        /// <param name="targetArchitecture">Architecture of the library to load (X86 / AMD64). Defaults to X86.</param>
        /// <exception cref="ArgumentException">An ArgumentException is thrown if the supplied generic type parameter is not an interface.</exception>
        /// <returns>Returns a new instance of TFunctions.</returns>
        public static TFunctions CreateWrapperClient(TargetArchitecture targetArchitecture = TargetArchitecture.X86)
        {
            if (!typeof(TFunctions).IsInterface)
            {
                throw new ArgumentException("Generic parameter type <TFunctions> must be an interface.", nameof(TFunctions));
            }

            IProxyGenerator generator = new ProxyGenerator(new PersistentProxyBuilder());
            IInterceptor interceptor = new WrapperClientInterceptor(typeof(TFunctions), targetArchitecture);

            return generator.CreateInterfaceProxyWithoutTarget<TFunctions>(interceptor);
        }
    }
}
