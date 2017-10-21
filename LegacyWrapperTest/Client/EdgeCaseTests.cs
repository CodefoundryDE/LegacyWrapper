using System;
using System.Net;
using LegacyWrapperClient.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LegacyWrapperTest.Client
{
    [TestClass]
    public class EdgeCaseTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestTypeIsNotAnInterface()
        {
            // Test a random class that's derrived from IDisposable but not an interface
            using (WrapperClientFactory<HttpListener>.CreateWrapperClient())
            {
                // Do nothing
            }
        }
    }
}
