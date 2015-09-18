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
        // Define delegates matching api function
        private delegate int GetSystemMetrics(int index);

        private delegate int TestStdCallDelegate(int param);

        private delegate int TestNormalFuncDelegate(int param);

        private delegate string TestPCharHandlingDelegate(string param);

        private delegate string TestPWideCharHandlingDelegate(string param);

        [TestMethod]
        public void TestCallMethodWithoutException()
        {
            // Create new WrapperClient
            // Remember to ensure a call to the Dispose()-Method!
            using (var client = new WrapperClient())
            {
                // Make calls providing library name, function name, and parameters
                int x = (int)client.Call<GetSystemMetrics>("User32.dll", "GetSystemMetrics", new object[] { 0 });
                int y = (int)client.Call<GetSystemMetrics>("User32.dll", "GetSystemMetrics", new object[] { 1 });
            }
        }

        [TestMethod]
        public void TestStdCall()
        {
            int input = 5;

            int result;
            using (var client = new WrapperClient())
            {
                result = (int)client.Call<TestStdCallDelegate>("TestDll.dll", "TestStdCall", new object[] { input });
            }

            Assert.AreEqual(input, result);
        }

        [TestMethod]
        public void TestNormalFunc()
        {
            int input = 5;

            int result;
            using (var client = new WrapperClient())
            {
                result = (int)client.Call<TestNormalFuncDelegate>("TestDll.dll", "TestNormalFunc", new object[] { input });
            }

            Assert.AreEqual(input, result);
        }

        [TestMethod]
        public void TestPCharHandling()
        {
            string input = "Hello World";

            string result;
            using (var client = new WrapperClient())
            {
                result = (string)client.Call<TestPCharHandlingDelegate>("TestDll.dll", "TestPCharHandling", new object[] { input });
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
                result = (string)client.Call<TestPWideCharHandlingDelegate>("TestDll.dll", "TestPWideCharHandling", new object[] { input });
            }

            Assert.AreEqual(input, result);
        }

        [TestMethod]
        [Ignore]
        public void TestPerformance()
        {
            Stopwatch stopwatch = new Stopwatch();

            long seed = Environment.TickCount;  // Prevents the JIT Compiler 
                                                // from optimizing Fkt calls away
            long result = 0;
            int count = 100000000;

            Debug.WriteLine("20 Tests without correct preparation");
            Debug.WriteLine("Warmup");
            for (int repeat = 0; repeat < 20; ++repeat)
            {
                stopwatch.Reset();
                stopwatch.Start();
                TestFunction(seed, count);
                stopwatch.Stop();
                Debug.WriteLine("Ticks: " + stopwatch.ElapsedTicks + " ms: " + stopwatch.ElapsedMilliseconds);
            }

            Process.GetCurrentProcess().ProcessorAffinity =
        new IntPtr(2); // Uses the second Core or Processor for the Test
            Process.GetCurrentProcess().PriorityClass =
        ProcessPriorityClass.High;      // Prevents "Normal" processes 
                                        // from interrupting Threads
            Thread.CurrentThread.Priority =
        ThreadPriority.Highest;     // Prevents "Normal" Threads 
                                    // from interrupting this thread

            Debug.WriteLine(string.Empty);
            Debug.WriteLine(string.Empty);
            Debug.WriteLine("20 Tests with correct preparation");
            Debug.WriteLine("Warmup");
            //stopwatch.Reset();
            //stopwatch.Start();
            //while (stopwatch.ElapsedMilliseconds < 1200)  // A Warmup of 1000-1500 ms 
            //{                                             // stabilizes the CPU cache and pipeline.
            //    TestFunction(seed, count); // Warmup
            //}
            //stopwatch.Stop();

            for (int repeat = 0; repeat < 20; ++repeat)
            {
                stopwatch.Reset();
                stopwatch.Start();
                TestFunction(seed, count);
                stopwatch.Stop();
                Debug.WriteLine("Ticks: " + stopwatch.ElapsedTicks + " ms: " + stopwatch.ElapsedMilliseconds);
            }
            Debug.WriteLine(result); // prevents optimizations (current compilers are 
                                     // too silly to analyze the dataflow that deep, but we never know )
        }

        public static void TestFunction(long seed, int count)
        {
            using (var client = new WrapperClient())
            {
                client.Call<TestStdCallDelegate>("TestDll.dll", "TestStdCall", new object[] { 5 });
            }
        }

        [TestMethod, ExpectedException(typeof(ObjectDisposedException))]
        public void TestMustThrowObjectDisposedException()
        {
            WrapperClient client;
            using (client = new WrapperClient())
            {
                // Do Nothing
            }

            client.Call<TestStdCallDelegate>("TestDll.dll", "TestStdCall", new object[] { });
        }
    }
}
