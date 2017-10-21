using System;
using System.Net;
using LegacyWrapper.ErrorHandling;
using LegacyWrapperClient.Client;
using LegacyWrapperTest.Interface;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LegacyWrapperTest.Client
{
    [TestClass]
    public class EdgeCaseTests : LegacyWrapperTestBase
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestTypeIsNotAnInterface()
        {
            // Test a random class that's derived from IDisposable but not an interface
            using (WrapperClientFactory<HttpListener>.CreateWrapperClient())
            {
                // Do nothing
            }
        }

        [TestMethod, ExpectedException(typeof(LegacyWrapperException))]
        public void TestLoadNonExistingLibrary()
        {
            using (var client = WrapperClientFactory<ITestDll>.CreateWrapperClient(ArchitectureToLoad))
            {
                client.TestNonExistingLibrary();
            }
        }

        [TestMethod, ExpectedException(typeof(LegacyWrapperException))]
        public void TestLoadNonExistingFunction()
        {
            using (var client = WrapperClientFactory<ITestDll>.CreateWrapperClient(ArchitectureToLoad))
            {
                client.TestNonExistingFunction();
            }
        }

        [TestMethod, ExpectedException(typeof(ObjectDisposedException))]
        public void TestMustThrowObjectDisposedException()
        {
            ITestDll client;
            using (client = WrapperClientFactory<ITestDll>.CreateWrapperClient(ArchitectureToLoad))
            {
                // Do nothing
            }

            client.TestStdCall(0);
        }

        [TestMethod, ExpectedException(typeof(LegacyWrapperException))]
        public void TestInterfaceContainsNoAttribute()
        {
            using (var client = WrapperClientFactory<ITestDllWithoutAttribute>.CreateWrapperClient())
            {
                client.MethodWithAttribute();
            }
        }

        [TestMethod, ExpectedException(typeof(LegacyWrapperException))]
        public void TestMethodContainsNoAttribute()
        {
            using (var client = WrapperClientFactory<ITestDllWithAttribute>.CreateWrapperClient())
            {
                client.MethodWithoutAttribute();
            }
        }
    }
}
