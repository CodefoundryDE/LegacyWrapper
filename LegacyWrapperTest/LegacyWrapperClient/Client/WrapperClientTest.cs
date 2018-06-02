using System;
using LegacyWrapper.Common.Serialization;
using LegacyWrapperClient.Client;
using LegacyWrapperClient.Transport;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace LegacyWrapperTest.LegacyWrapperClient.Client
{
    [TestClass]
    public class WrapperClientTest
    {
        [TestMethod]
        public void TestWrapperClientMakesOneSendReceiveCycle()
        {
            Mock<IPipeConnector> pipeConnectorMock = new Mock<IPipeConnector>();
            pipeConnectorMock.Setup(m => m.ReceiveCallResponse())
                .Returns(new CallResult() { Parameters = new object[0] });
            WrapperClient wrapperClient = new WrapperClient(pipeConnectorMock.Object);

            wrapperClient.InvokeInternal(new CallData() { Parameters = new object[0] });

            pipeConnectorMock.Verify(m => m.SendCallRequest(It.IsAny<CallData>()), Times.Once);
            pipeConnectorMock.Verify(m => m.ReceiveCallResponse(), Times.Once);
        }

        [TestMethod]
        public void TestCallParametersAreCopied()
        {
            object[] passedParameters = { 1, 2, 3 };
            object[] returnedParameters = { 3, 2, 1 };
            CallData callData = new CallData() { Parameters = passedParameters };

            Mock<IPipeConnector> pipeConnectorMock = new Mock<IPipeConnector>();
            pipeConnectorMock.Setup(m => m.ReceiveCallResponse())
                .Returns(new CallResult() { Parameters = returnedParameters });
            WrapperClient wrapperClient = new WrapperClient(pipeConnectorMock.Object);

            wrapperClient.InvokeInternal(callData);

            CollectionAssert.AreEqual(returnedParameters, passedParameters);
            Assert.AreNotSame(passedParameters, returnedParameters);
        }
    }
}
