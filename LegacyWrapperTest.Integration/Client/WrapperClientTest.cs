using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using LegacyWrapper.ErrorHandling;
using LegacyWrapperClient.Architecture;
using LegacyWrapperClient.Client;
using LegacyWrapperClient.Configuration;
using LegacyWrapperClient.DynamicProxy;
using LegacyWrapperTest.Integration.Interface;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LegacyWrapperTest.Integration.Client
{
    [TestClass]
    public class WrapperClientTest : LegacyWrapperTestBase
    {
        [TestMethod]
        public void TestCallMethodWithoutException()
        {
            WrapperClientInterceptor.OverrideLibraryName = null;

            IWrapperConfig configuration = WrapperConfigBuilder.Create()
                .TargetArchitecture(ArchitectureToLoad)
                .Build();

            // Create new Wrapper client providing the proxy interface
            // Remember to ensure a call to the Dispose()-Method!
            using (var client = WrapperProxyFactory<IUser32Dll>.GetInstance(configuration))
            {
                // Make calls - it's that simple!
                int x = client.GetSystemMetrics(0);
                int y = client.GetSystemMetrics(1);
            }
        }

        [TestMethod]
        public void TestStdCall()
        {
            IWrapperConfig configuration = WrapperConfigBuilder.Create()
                .TargetArchitecture(ArchitectureToLoad)
                .Build();

            int input = 5;

            int result;

            using (var client = WrapperProxyFactory<ITestDll>.GetInstance(configuration))
            {
                result = client.TestStdCall(input);
            }

            Assert.AreEqual(input, result);
        }

        [TestMethod]
        public void TestNormalFunc()
        {
            IWrapperConfig configuration = WrapperConfigBuilder.Create()
                .TargetArchitecture(ArchitectureToLoad)
                .Build();

            int input = 5;

            int result;

            using (var client = WrapperProxyFactory<ITestDll>.GetInstance(configuration))
            {
                result = client.TestNormalFunc(input);
            }

            Assert.AreEqual(input, result);
        }

        [TestMethod]
        public void TestPCharHandling()
        {
            IWrapperConfig configuration = WrapperConfigBuilder.Create()
                .TargetArchitecture(ArchitectureToLoad)
                .Build();

            string input = "Hello World";

            string result;

            using (var client = WrapperProxyFactory<ITestDll>.GetInstance(configuration))
            {
                result = client.TestPCharHandling(input);
            }

            Assert.AreEqual(input, result);
        }

        [TestMethod]
        public void TestPWideCharHandling()
        {
            IWrapperConfig configuration = WrapperConfigBuilder.Create()
                .TargetArchitecture(ArchitectureToLoad)
                .Build();

            string input = "Hello World";

            string result;

            using (var client = WrapperProxyFactory<ITestDll>.GetInstance(configuration))
            {
                result = client.TestPWideCharHandling(input);
            }

            Assert.AreEqual(input, result);
        }

        [TestMethod]
        public void TestRefParameterHandling()
        {
            IWrapperConfig configuration = WrapperConfigBuilder.Create()
                .TargetArchitecture(ArchitectureToLoad)
                .Build();

            int parameter = 1337;
            using (var client = WrapperProxyFactory<ITestDll>.GetInstance(configuration))
            {
                client.TestVarParamHandling(ref parameter);
            }

            // Ref param should be incremented by 1
            Assert.AreEqual(1338, parameter);
        }

        [TestMethod]
        public void TestMultipleRefParamsHandling()
        {
            IWrapperConfig configuration = WrapperConfigBuilder.Create()
                .TargetArchitecture(ArchitectureToLoad)
                .Build();

            int param1 = 1337;
            int param2 = 7777;

            using (var client = WrapperProxyFactory<ITestDll>.GetInstance(configuration))
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
            IWrapperConfig configuration = WrapperConfigBuilder.Create()
                .TargetArchitecture(ArchitectureToLoad)
                .Build();

            int parameter = 1337;

            using (var client = WrapperProxyFactory<ITestDll>.GetInstance(configuration))
            {
                client.TestVarParamHandling(ref parameter);
                client.TestVarParamHandling(ref parameter);
            }

            // Ref param should be incremented by 1
            Assert.AreEqual(1339, parameter);
        }

        
    }
}
