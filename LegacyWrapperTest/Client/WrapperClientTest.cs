using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using LegacyWrapperClient.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LegacyWrapperTest.Client
{
    [TestClass]
    public class WrapperClientTest
    {
        private const string TestDllPath = @"TestLibrary\TestDll.dll";

        // We assume the following dll functions in our Test.dll here: 
        //
        // function TestStdCall(const AParam: Integer): Integer; stdcall;
        // function TestNormalFunc(const AParam: Integer): Integer;
        // function TestPCharHandling(const AParam: PChar): PChar; stdcall;
        // function TestPWideCharHandling(const AParam: PWideChar): PWideChar; stdcall;
        //
        // Every function is supposed to return the input parameter unchanged.

        // Define delegates matching api function
        private delegate int GetSystemMetrics(int index);

        private delegate int TestStdCallDelegate(int param);
        private delegate int TestNormalFuncDelegate(int param);
        private delegate string TestPCharHandlingDelegate(string param);
        private delegate string TestPWideCharHandlingDelegate(string param);

        // These functions are expected to increment their parameters by 1
        private delegate void TestVarParamHandling(ref int param);
        private delegate void TestMultipleVarParamsHandling(ref int param1, ref int param2);

        [TestMethod]
        public void TestCallMethodWithoutException()
        {
            // Create new WrapperClient
            // Remember to ensure a call to the Dispose()-Method!
            using (var client = new WrapperClient())
            {
                // Make calls providing library name, function name, and parameters
                int x = (int)client.Invoke<GetSystemMetrics>("User32.dll", "GetSystemMetrics", new object[] { 0 });
                int y = (int)client.Invoke<GetSystemMetrics>("User32.dll", "GetSystemMetrics", new object[] { 1 });
            }
        }

        [TestMethod]
        public void TestStdCall()
        {
            int input = 5;

            int result;
            using (var client = new WrapperClient())
            {
                result = (int)client.Invoke<TestStdCallDelegate>(TestDllPath, "TestStdCall", new object[] { input });
            }

            Assert.AreEqual(input, result);
        }

        /// <summary>
        /// This will NOT work, since the dll function is not a stdcall function
        /// </summary>
        [TestMethod]
        public void TestNormalFunc()
        {
            int input = 5;

            int result;
            using (var client = new WrapperClient())
            {
                result = (int)client.Invoke<TestNormalFuncDelegate>(TestDllPath, "TestNormalFunc", new object[] { input });
            }

            Assert.AreNotEqual(input, result);
        }

        [TestMethod]
        public void TestPCharHandling()
        {
            string input = "Hello World";

            string result;
            using (var client = new WrapperClient())
            {
                result = (string)client.Invoke<TestPCharHandlingDelegate>(TestDllPath, "TestPCharHandling", new object[] { input });
            }

            Assert.AreEqual(input, result);
        }

        [TestMethod]
        public void TestPWideCharHandling()
        {
            string input = "Hello World";

            string result;
            using (var client = new WrapperClient())
            {
                result = (string)client.Invoke<TestPWideCharHandlingDelegate>(TestDllPath, "TestPWideCharHandling", new object[] { input });
            }

            Assert.AreEqual(input, result);
        }

        [TestMethod, ExpectedException(typeof(ObjectDisposedException))]
        public void TestMustThrowObjectDisposedException()
        {
            WrapperClient client;
            using (client = new WrapperClient())
            {
                // Do Nothing
            }

            client.Invoke<TestStdCallDelegate>(TestDllPath, "TestStdCall", new object[0]);
        }

        /// <summary>
        /// The wrapper client should throw an ArgumentException when the generic type param is not a delegate
        /// </summary>
        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void TestMustThrowArgumentException()
        {
            using (var client = new WrapperClient())
            {
                client.Invoke<object>(string.Empty, string.Empty, new object[0]);
            }
        }

        [TestMethod]
        public void TestRefParameterHandling()
        {
            object[] parameter = { 1337 };
            using (var client = new WrapperClient())
            {
                client.Invoke<TestVarParamHandling>(TestDllPath, "TestVarParamHandling", parameter);

                // Ref param should be incremented by 1
                Assert.AreEqual(1338, parameter[0]);
            }
        }

        [TestMethod]
        public void TestMultipleRefParamsHandling()
        {
            object[] parameters = { 1337, 7777 };
            using (var client = new WrapperClient())
            {
                client.Invoke<TestMultipleVarParamsHandling>(TestDllPath, "TestMultipleVarParamsHandling", parameters);

                // Ref param should be incremented by 1
                Assert.AreEqual(1338, parameters[0]);
                Assert.AreEqual(7778, parameters[1]);
            }
        }
    }
}
