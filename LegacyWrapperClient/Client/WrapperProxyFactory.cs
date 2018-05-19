using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using LegacyWrapper.Common.Token;
using LegacyWrapperClient.Architecture;
using LegacyWrapperClient.Configuration;
using LegacyWrapperClient.DynamicProxy;
using LegacyWrapperClient.ProcessHandling;
using LegacyWrapperClient.Token;
using LegacyWrapperClient.Transport;
using Ninject;
using Ninject.Modules;
using PommaLabs.Thrower;

namespace LegacyWrapperClient.Client
{

    /// <summary>
    /// WrapperProxyFactory provides a method to generate a new proxy instance to the wrapper executable.
    /// </summary>
    public static class WrapperProxyFactory<TFunctions> where TFunctions : class, IDisposable
    {
        // ReSharper disable once StaticMemberInGenericType
        // It's ok to have separate injection kernels for different interfaces
        internal static IKernel InjectionKernel { get; }

        static WrapperProxyFactory()
        {
            InjectionKernel = new StandardKernel();

            InjectionKernel.Bind<IFormatter>().To<BinaryFormatter>();
            InjectionKernel.Bind<IInterceptor>().To<WrapperClientInterceptor>();
            InjectionKernel.Bind<IPipeConnector>().To<PipeConnector>();
            InjectionKernel.Bind<ITokenGenerator>().To<GuidTokenGenerator>();
            InjectionKernel.Bind<ILibraryNameProvider>().To<DefaultLibraryNameProvider>();
            InjectionKernel.Bind<IWrapperExecutableNameProvider>().To<DefaultWrapperExecutableNameProvider>();
            InjectionKernel.Bind<IWrapperProcessStarter>().To<WrapperProcessStarter>();
        }

        private static TFunctions CreateProxy()
        {
            IInterceptor interceptor = InjectionKernel.Get<IInterceptor>();

            IProxyGenerator generator = new ProxyGenerator(new DefaultProxyBuilder());
            return generator.CreateInterfaceProxyWithoutTarget<TFunctions>(interceptor);
        }

        /// <summary>
        /// Creates a new instance of TFunctions.
        /// All calls will be proxied over a named pipe to the wrapper executable.
        /// </summary>
        /// <param name="configuration">WrapperConfiguration object holding configuration info.</param>
        /// <exception cref="ArgumentException">An ArgumentException is thrown if the supplied generic type parameter is not an interface.</exception>
        /// <returns>Returns a new instance of TFunctions.</returns>
        public static TFunctions GetInstance(IWrapperConfig configuration)
        {
            Raise.ArgumentException.If(!typeof(TFunctions).IsInterface, nameof(TFunctions), "Generic parameter type <TFunctions> must be an interface.");

            CreateToken();

            InjectionKernel.Rebind<IWrapperConfig>().ToConstant(configuration);
            InjectionKernel.Rebind<Type>().ToConstant(typeof(TFunctions));

            return CreateProxy();
        }

        private static void CreateToken()
        {
            ITokenGenerator tokenGenerator = InjectionKernel.Get<ITokenGenerator>();

            InjectionKernel.Rebind<PipeToken>().ToConstant(tokenGenerator.GenerateToken());
        }
    }
}
