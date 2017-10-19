using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using LegacyWrapper.ErrorHandling;
using LegacyWrapperClient.Architecture;
using LegacyWrapperClient.Client;
using LegacyWrapperClient.DynamicProxy;
using LegacyWrapperTest.Interface;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LegacyWrapperTest.Client
{
    [TestClass]
    public class WrapperClientTest
    {
        private TargetArchitecture ArchitectureToLoad;

        /// <summary>
        /// Here we're going to determine if we're running as 32 bit or 64 bit process - and use the opposing wrapper.
        /// However, there's no way to tell Visual Studio or ReSharper that they should run unit tests on both architectures - 
        /// we have to do that manually.
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            if (Environment.Is64BitProcess)
            {
                ArchitectureToLoad = TargetArchitecture.X86;
                WrapperClientInterceptor.OverrideLibraryName = @"TestLibrary\LegacyWrapperTestDll32.dll";
            }
            else
            {
                ArchitectureToLoad = TargetArchitecture.Amd64;
                WrapperClientInterceptor.OverrideLibraryName = @"TestLibrary\LegacyWrapperTestDll64.dll";
            }
        }

        [TestMethod]
        public void TestCallMethodWithoutException()
        {
            WrapperClientInterceptor.OverrideLibraryName = null;
            // Create new WrapperClient
            // Remember to ensure a call to the Dispose()-Method!
            using (var client = WrapperClientFactory<IUser32Dll>.CreateWrapperClient(ArchitectureToLoad))
            {
                // Make calls providing library name, function name, and parameters
                int x = client.GetSystemMetrics(0);
                int y = client.GetSystemMetrics(1);
            }
        }

        [TestMethod]
        public void TestStdCall()
        {
            int input = 5;

            int result;

            using (var client = WrapperClientFactory<ITestDll>.CreateWrapperClient(ArchitectureToLoad))
            {
                result = client.TestStdCall(input);
            }

            Assert.AreEqual(input, result);
        }

        [TestMethod]
        public void TestNormalFunc()
        {
            int input = 5;

            int result;

            using (var client = WrapperClientFactory<ITestDll>.CreateWrapperClient(ArchitectureToLoad))
            {
                result = client.TestNormalFunc(input);
            }

            Assert.AreEqual(input, result);
        }

        [TestMethod]
        public void TestPCharHandling()
        {
            string input = "Hello World";

            string result;

            using (var client = WrapperClientFactory<ITestDll>.CreateWrapperClient(ArchitectureToLoad))
            {
                result = client.TestPCharHandling(input);
            }

            Assert.AreEqual(input, result);
        }

        [TestMethod]
        public void TestPWideCharHandling()
        {
            string input = "Hello World";

            string result;

            using (var client = WrapperClientFactory<ITestDll>.CreateWrapperClient(ArchitectureToLoad))
            {
                result = client.TestPWideCharHandling(input);
            }

            Assert.AreEqual(input, result);
        }

        [TestMethod, ExpectedException(typeof(ObjectDisposedException))]
        public void TestMustThrowObjectDisposedException()
        {
            ITestDll client;
            using (client = WrapperClientFactory<ITestDll>.CreateWrapperClient(ArchitectureToLoad))
            {
                // Do Nothing
            }

            client.TestStdCall(0);
        }

        [TestMethod]
        public void TestRefParameterHandling()
        {
            int parameter = 1337;
            using (var client = WrapperClientFactory<ITestDll>.CreateWrapperClient(ArchitectureToLoad))
            {
                client.TestVarParamHandling(ref parameter);
            }

            // Ref param should be incremented by 1
            Assert.AreEqual(1338, parameter);
        }

        [TestMethod]
        public void TestMultipleRefParamsHandling()
        {
            int param1 = 1337;
            int param2 = 7777;

            using (var client = WrapperClientFactory<ITestDll>.CreateWrapperClient(ArchitectureToLoad))
            {
                client.TestMultipleVarParamsHandling(ref param1, ref param2);
            }

            // Ref param should be incremented by 1
            Assert.AreEqual(1338, param1);
            Assert.AreEqual(7778, param2);
        }

        [TestMethod]
        public void TestCallLibraryMultipleTimes()
        {
            int parameter = 1337;

            using (var client = WrapperClientFactory<ITestDll>.CreateWrapperClient(ArchitectureToLoad))
            {
                client.TestVarParamHandling(ref parameter);
                client.TestVarParamHandling(ref parameter);
            }

            // Ref param should be incremented by 1
            Assert.AreEqual(1339, parameter);
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
    }
}
