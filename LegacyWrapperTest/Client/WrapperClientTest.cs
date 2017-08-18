using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using LegacyWrapper.ErrorHandling;
using LegacyWrapperClient.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LegacyWrapperTest.Client
{
    [TestClass]
    public class WrapperClientTest
    {
        private const string TestDllPath = @"TestLibrary\LegacyWrapperTestDll32.dll";

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

        [TestMethod]
        public void TestCallMethodWithoutException()
        {
            // Create new WrapperClient
            // Remember to ensure a call to the Dispose()-Method!
            using (var client = new WrapperClient("User32.dll"))
            {
                // Make calls providing library name, function name, and parameters
                int x = (int)client.Invoke<GetSystemMetrics>("GetSystemMetrics", new object[] { 0 });
                int y = (int)client.Invoke<GetSystemMetrics>("GetSystemMetrics", new object[] { 1 });
            }
        }

        [TestMethod]
        public void TestStdCall()
        {
            int input = 5;

            int result;
            using (var client = new WrapperClient(TestDllPath))
            {
                result = (int)client.Invoke<TestStdCallDelegate>("TestStdCall", new object[] { input });
            }

            Assert.AreEqual(input, result);
        }

        [TestMethod]
        public void TestNormalFunc()
        {
            int input = 5;

            int result;
            using (var client = new WrapperClient(TestDllPath))
            {
                result = (int)client.Invoke<TestNormalFuncDelegate>("TestNormalFunc", new object[] { input });
            }

            Assert.AreEqual(input, result);
        }

        [TestMethod]
        public void TestPCharHandling()
        {
            string input = "Hello World";

            string result;
            using (var client = new WrapperClient(TestDllPath))
            {
                result = (string)client.Invoke<TestPCharHandlingDelegate>("TestPCharHandling", new object[] { input });
            }

            Assert.AreEqual(input, result);
        }

        [TestMethod]
        public void TestPWideCharHandling()
        {
            string input = "Hello World";

            string result;
            using (var client = new WrapperClient(TestDllPath))
            {
                result = (string)client.Invoke<TestPWideCharHandlingDelegate>("TestPWideCharHandling", new object[] { input });
            }

            Assert.AreEqual(input, result);
        }

        [TestMethod, ExpectedException(typeof(ObjectDisposedException))]
        public void TestMustThrowObjectDisposedException()
        {
            WrapperClient client;
            using (client = new WrapperClient(TestDllPath))
            {
                // Do Nothing
            }

            client.Invoke<TestStdCallDelegate>("TestStdCall", new object[0]);
        }

        /// <summary>
        /// The wrapper client should throw an ArgumentException when the generic type param is not a delegate
        /// </summary>
        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void TestMustThrowArgumentException()
        {
            using (var client = new WrapperClient(TestDllPath))
            {
                client.Invoke<object>(string.Empty, new object[0]);
            }
        }

        [TestMethod]
        public void TestRefParameterHandling()
        {
            object[] parameter = { 1337 };
            using (var client = new WrapperClient(TestDllPath))
            {
                client.Invoke<TestVarParamHandling>("TestVarParamHandling", parameter);

                // Ref param should be incremented by 1
                Assert.AreEqual(1338, parameter[0]);
            }
        }

        [TestMethod]
        public void TestMultipleRefParamsHandling()
        {
            object[] parameters = { 1337, 7777 };
            using (var client = new WrapperClient(TestDllPath))
            {
                client.Invoke<TestMultipleVarParamsHandling>("TestMultipleVarParamsHandling", parameters);

                // Ref param should be incremented by 1
                Assert.AreEqual(1338, parameters[0]);
                Assert.AreEqual(7778, parameters[1]);
            }
        }

        [TestMethod]
        public void TestCallLibraryMultipleTimes()
        {
            object[] parameter = { 1337 };
            using (var client = new WrapperClient(TestDllPath))
            {
                client.Invoke<TestVarParamHandling>("TestVarParamHandling", parameter);
                client.Invoke<TestVarParamHandling>("TestVarParamHandling", parameter);

                // Ref param should be incremented by 1
                Assert.AreEqual(1339, parameter[0]);
            }
        }

        [TestMethod, ExpectedException(typeof(LegacyWrapperException))]
        public void TestWrongRefParameterHandling()
        {
            using (var client = new WrapperClient(TestDllPath))
            {
                client.Invoke<TestWrongVarParamHandling>("TestVarParamHandling", new object[0]);
            }
        }

        [TestMethod, ExpectedException(typeof(LegacyWrapperException))]
        public void TestLoadNonExistingLibrary()
        {
            using (var client = new WrapperClient(Guid.NewGuid().ToString()))
            {
                client.Invoke<TestWrongVarParamHandling>("DummyMethod", new object[0]);
            }
        }

        [TestMethod, ExpectedException(typeof(LegacyWrapperException))]
        public void TestLoadNonExistingFunction()
        {
            using (var client = new WrapperClient(TestDllPath))
            {
                client.Invoke<TestWrongVarParamHandling>("DummyMethod", new object[0]);
            }
        }
    }
}
