using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using LegacyWrapper.ErrorHandling;
using LegacyWrapperClient.Architecture;
using LegacyWrapperClient.Client;
using LegacyWrapperTest.Interface;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LegacyWrapperTest.Client
{
    [TestClass]
    public class WrapperClientTest
    {
        private string TestDllPath;
        private TargetArchitecture ArchitectureToLoad;

        // We assume the following dll functions in our Test.dll here: 
        // Every function is supposed to return the input parameter unchanged.
        //
        // function TestStdCall(const AParam: Integer): Integer; stdcall;
        // function TestNormalFunc(const AParam: Integer): Integer;
        // function TestPCharHandling(const AParam: PChar): PChar; stdcall;
        // function TestPWideCharHandling(const AParam: PWideChar): PWideChar; stdcall;
        //
        // These procedures are expected to increment their parameters by 1:
        //
        // procedure TestVarParamHandling(var AParam: Integer); stdcall;      
        // procedure TestMultipleVarParamsHandling(var AParam1: Integer; var AParam2: Integer); stdcall;

        // Define delegates matching api function
        private delegate int GetSystemMetrics(int index);

        private delegate int TestStdCallDelegate(int param);
        private delegate int TestNormalFuncDelegate(int param);
        private delegate string TestPCharHandlingDelegate(string param);
        private delegate string TestPWideCharHandlingDelegate(string param);


        private delegate void TestVarParamHandling(ref int param);
        private delegate void TestMultipleVarParamsHandling(ref int param1, ref int param2);

        // Expected to throw an exception
        private delegate void TestWrongVarParamHandling(int param);

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
                TestDllPath = @"TestLibrary\LegacyWrapperTestDll32.dll";
            }
            else
            {
                ArchitectureToLoad = TargetArchitecture.Amd64;
                TestDllPath = @"TestLibrary\LegacyWrapperTestDll64.dll";
            }
        }

        [TestMethod]
        public void TestCallMethodWithoutException()
        {
            // Create new WrapperClient
            // Remember to ensure a call to the Dispose()-Method!
            var client = WrapperClientFactory<IUser32Dll>.CreateWrapperClient("User32.dll", ArchitectureToLoad);

            // Make calls providing library name, function name, and parameters
            int x = client.GetSystemMetrics(0);
            int y = client.GetSystemMetrics(1);

        }

        [TestMethod]
        public void TestStdCall()
        {
            int input = 5;

            int result;

            var client = WrapperClientFactory<ITestDll>.CreateWrapperClient(TestDllPath, ArchitectureToLoad);

            result = client.TestStdCall(input);

            Assert.AreEqual(input, result);
        }

        [TestMethod]
        public void TestNormalFunc()
        {
            int input = 5;

            int result;

            var client = WrapperClientFactory<ITestDll>.CreateWrapperClient(TestDllPath, ArchitectureToLoad);

            result = client.TestNormalFunc(input);

            Assert.AreEqual(input, result);
        }

        [TestMethod]
        public void TestPCharHandling()
        {
            string input = "Hello World";

            string result;

            var client = WrapperClientFactory<ITestDll>.CreateWrapperClient(TestDllPath, ArchitectureToLoad);

            result = client.TestPCharHandling(input);

            Assert.AreEqual(input, result);
        }

        [TestMethod]
        public void TestPWideCharHandling()
        {
            string input = "Hello World";

            string result;

            var client = WrapperClientFactory<ITestDll>.CreateWrapperClient(TestDllPath, ArchitectureToLoad);

            result = client.TestPWideCharHandling(input);

            Assert.AreEqual(input, result);
        }

        //[TestMethod, ExpectedException(typeof(ObjectDisposedException))]
        //public void TestMustThrowObjectDisposedException()
        //{
        //    WrapperClient client;
        //    using (client = new WrapperClient(TestDllPath, ArchitectureToLoad))
        //    {
        //        // Do Nothing
        //    }

        //    client.Invoke<TestStdCallDelegate>("TestStdCall", new object[0]);
        //}

        [TestMethod]
        public void TestRefParameterHandling()
        {
            int parameter = 1337;
            var client = WrapperClientFactory<ITestDll>.CreateWrapperClient(TestDllPath, ArchitectureToLoad);

            client.TestVarParamHandling(ref parameter);

            // Ref param should be incremented by 1
            Assert.AreEqual(1338, parameter);

        }

        [TestMethod]
        public void TestMultipleRefParamsHandling()
        {
            int param1 = 1337;
            int param2 = 7777;

            var client = WrapperClientFactory<ITestDll>.CreateWrapperClient(TestDllPath, ArchitectureToLoad);

            client.TestMultipleVarParamsHandling(ref param1, ref param2);

            // Ref param should be incremented by 1
            Assert.AreEqual(1338, param1);
            Assert.AreEqual(7778, param2);
        }

        [TestMethod]
        public void TestCallLibraryMultipleTimes()
        {
            int parameter = 1337;

            var client = WrapperClientFactory<ITestDll>.CreateWrapperClient(TestDllPath, ArchitectureToLoad);

            client.TestVarParamHandling(ref parameter);
            client.TestVarParamHandling(ref parameter);

            // Ref param should be incremented by 1
            Assert.AreEqual(1339, parameter);

        }

        [TestMethod, ExpectedException(typeof(LegacyWrapperException))]
        public void TestLoadNonExistingLibrary()
        {
            var client = WrapperClientFactory<ITestDll>.CreateWrapperClient("DummyLibraryName.dll", ArchitectureToLoad);

            client.TestNonExistingLibrary();
        }

        [TestMethod, ExpectedException(typeof(LegacyWrapperException))]
        public void TestLoadNonExistingFunction()
        {
            var client = WrapperClientFactory<ITestDll>.CreateWrapperClient(TestDllPath, ArchitectureToLoad);

            client.TestNonExistingFunction();
        }
    }
}
