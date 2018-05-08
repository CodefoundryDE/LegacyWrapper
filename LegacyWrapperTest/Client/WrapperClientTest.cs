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
    public class WrapperClientTest : LegacyWrapperTestBase
    {
        [TestMethod]
        public void TestCallMethodWithoutException()
        {
            WrapperClientInterceptor.OverrideLibraryName = null;

            // Create new Wrapper client providing the proxy interface
            // Remember to ensure a call to the Dispose()-Method!
            using (var client = WrapperClientFactory<IUser32Dll>.CreateWrapperClient(ArchitectureToLoad))
            {
                // Make calls - it's that simple!
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

        
    }
}
