using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using LegacyWrapperClient.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LegacyWrapperTest.Client
{
    [TestClass]
    public class WrapperClientTest
    {

        // Define delegate matching api function
        private delegate int GetSystemMetrics(int index);

        [TestMethod]
        public void TestCallMethod()
        {
            int x = (int)WrapperClient.Call<GetSystemMetrics>("User32.dll", "GetSystemMetrics", new object[] { 0 });
            int y = (int)WrapperClient.Call<GetSystemMetrics>("User32.dll", "GetSystemMetrics", new object[] { 1 });
        }
    }
}
