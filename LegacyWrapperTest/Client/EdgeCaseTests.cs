using System;
using System.Net;
using LegacyWrapper.ErrorHandling;
using LegacyWrapperClient.Architecture;
using LegacyWrapperClient.Client;
using LegacyWrapperClient.Configuration;
using LegacyWrapperTest.Interface;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LegacyWrapperTest.Client
{
    [TestClass]
    public class EdgeCaseTests : LegacyWrapperTestBase
    {
        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void TestTypeIsNotAnInterface()
        {
            IWrapperConfig configuration = WrapperConfigBuilder.Create()
                .TargetArchitecture(ArchitectureToLoad)
                .Build();

            // Test a random class that's derived from IDisposable but not an interface
            using (WrapperProxyFactory<HttpListener>.GetInstance(configuration))
            {
                // Do nothing
            }
        }

        [TestMethod, ExpectedException(typeof(LegacyWrapperException))]
        public void TestLoadNonExistingLibrary()
        {
            IWrapperConfig configuration = WrapperConfigBuilder.Create()
                .TargetArchitecture(ArchitectureToLoad)
                .Build();

            using (var client = WrapperProxyFactory<ITestDll>.GetInstance(configuration))
            {
                client.TestNonExistingLibrary();
            }
        }

        [TestMethod, ExpectedException(typeof(LegacyWrapperException))]
        public void TestLoadNonExistingFunction()
        {
            IWrapperConfig configuration = WrapperConfigBuilder.Create()
                .TargetArchitecture(ArchitectureToLoad)
                .Build();

            using (var client = WrapperProxyFactory<ITestDll>.GetInstance(configuration))
            {
                client.TestNonExistingFunction();
            }
        }

        [TestMethod, ExpectedException(typeof(ObjectDisposedException))]
        public void TestMustThrowObjectDisposedException()
        {
            IWrapperConfig configuration = WrapperConfigBuilder.Create()
                .TargetArchitecture(ArchitectureToLoad)
                .Build();

            ITestDll client;
            using (client = WrapperProxyFactory<ITestDll>.GetInstance(configuration))
            {
                // Do nothing
            }

            client.TestStdCall(0);
        }

        [TestMethod, ExpectedException(typeof(LegacyWrapperException))]
        public void TestInterfaceContainsNoAttribute()
        {
            IWrapperConfig configuration = WrapperConfigBuilder.Create()
                .TargetArchitecture(ArchitectureToLoad)
                .Build();

            using (var client = WrapperProxyFactory<ITestDllWithoutAttribute>.GetInstance(configuration))
            {
                client.MethodWithAttribute();
            }
        }

        [TestMethod, ExpectedException(typeof(LegacyWrapperException))]
        public void TestMethodContainsNoAttribute()
        {
            IWrapperConfig configuration = WrapperConfigBuilder.Create()
                .TargetArchitecture(ArchitectureToLoad)
                .Build();

            using (var client = WrapperProxyFactory<ITestDllWithAttribute>.GetInstance(configuration))
            {
                client.MethodWithoutAttribute();
            }
        }
    }
}
