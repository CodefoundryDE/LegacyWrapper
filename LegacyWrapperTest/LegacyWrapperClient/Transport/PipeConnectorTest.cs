using System;
using System.IO;
using System.IO.Pipes;
using System.Runtime.Serialization;
using LegacyWrapper.Common.Serialization;
using LegacyWrapper.Common.Token;
using LegacyWrapper.ErrorHandling;
using LegacyWrapperClient.ProcessHandling;
using LegacyWrapperClient.Transport;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace LegacyWrapperTest.LegacyWrapperClient.Transport
{
    [TestClass]
    public class PipeConnectorTest
    {
        private Mock<PipeStreamFactory> GetPipeStreamFactoryMock()
        {
            Mock<PipeStream> pipeStreamMock = new Mock<PipeStream>(PipeDirection.InOut, PipeTransmissionMode.Message, 0);

            Mock<PipeStreamFactory> pipeClientStreamFactoryMock = new Mock<PipeStreamFactory>();

            pipeClientStreamFactoryMock
                .Setup(mock => mock.GetConnectedPipeStream(It.IsAny<PipeToken>()))
                .Returns(pipeStreamMock.Object);

            return pipeClientStreamFactoryMock;
        }

        [TestMethod]
        public void TestPipeConnectorOpensPipe()
        {
            Mock<IFormatter> formatterMock = new Mock<IFormatter>();
            Mock<IWrapperProcessStarter> wrapperProcessStarterMock = new Mock<IWrapperProcessStarter>();
            Mock<PipeStreamFactory> pipeStreamFactoryMock = GetPipeStreamFactoryMock();

            PipeToken pipeToken = new PipeToken(Guid.NewGuid().ToString());
            IPipeConnector pipeConnector = new PipeConnector(formatterMock.Object, wrapperProcessStarterMock.Object, pipeStreamFactoryMock.Object, pipeToken);
            CallData callDataToSend = new CallData();

            pipeConnector.SendCallRequest(callDataToSend);

            pipeStreamFactoryMock.Verify(mock => mock.GetConnectedPipeStream(It.Is<PipeToken>(actual => Equals(actual, pipeToken))), Times.Once);
        }

        [TestMethod]
        public void TestPipeConnectorSendsCallRequest()
        {
            Mock<IFormatter> formatterMock = new Mock<IFormatter>();
            Mock<IWrapperProcessStarter> wrapperProcessStarterMock = new Mock<IWrapperProcessStarter>();
            Mock<PipeStreamFactory> pipeStreamFactoryMock = GetPipeStreamFactoryMock();

            PipeToken pipeToken = new PipeToken(Guid.NewGuid().ToString());
            IPipeConnector pipeConnector = new PipeConnector(formatterMock.Object, wrapperProcessStarterMock.Object, pipeStreamFactoryMock.Object, pipeToken);
            CallData callDataToSend = new CallData();

            pipeConnector.SendCallRequest(callDataToSend);

            formatterMock.Verify(mock => mock.Serialize(It.IsAny<PipeStream>(), It.Is<CallData>(actual => ReferenceEquals(actual, callDataToSend))), Times.AtLeast(1));
        }

        [TestMethod]
        public void TestPipeConnectorFetchesCallResponse()
        {
            CallResult callResultToFetch = new CallResult();

            Mock<IFormatter> formatterMock = new Mock<IFormatter>();
            formatterMock
                .Setup(x => x.Deserialize(It.IsAny<Stream>()))
                .Returns(callResultToFetch);
            Mock<IWrapperProcessStarter> wrapperProcessStarterMock = new Mock<IWrapperProcessStarter>();
            Mock<PipeStreamFactory> pipeStreamFactoryMock = GetPipeStreamFactoryMock();

            PipeToken pipeToken = new PipeToken(Guid.NewGuid().ToString());
            IPipeConnector pipeConnector = new PipeConnector(formatterMock.Object, wrapperProcessStarterMock.Object, pipeStreamFactoryMock.Object, pipeToken);

            CallResult actualCallResult = pipeConnector.ReceiveCallResponse();

            formatterMock.Verify(mock => mock.Deserialize(It.IsAny<PipeStream>()), Times.AtLeast(1));
            Assert.AreEqual(callResultToFetch, actualCallResult);
        }

        [TestMethod, ExpectedException(typeof(LegacyWrapperException))]
        public void TestThrowsExceptionIfContainedInCallResult()
        {
            CallResult callResultToFetch = new CallResult();
            callResultToFetch.Exception = new LegacyWrapperException();

            Mock<IFormatter> formatterMock = new Mock<IFormatter>();
            formatterMock
                .Setup(x => x.Deserialize(It.IsAny<Stream>()))
                .Returns(callResultToFetch);
            Mock<IWrapperProcessStarter> wrapperProcessStarterMock = new Mock<IWrapperProcessStarter>();
            Mock<PipeStreamFactory> pipeStreamFactoryMock = GetPipeStreamFactoryMock();

            PipeToken pipeToken = new PipeToken(Guid.NewGuid().ToString());
            IPipeConnector pipeConnector = new PipeConnector(formatterMock.Object, wrapperProcessStarterMock.Object, pipeStreamFactoryMock.Object, pipeToken);

            pipeConnector.ReceiveCallResponse();

            Assert.Fail("Exception should have been thrown by PipeConnector");
        }

        [TestMethod]
        public void TestPipeGetsClosed()
        {
            CallResult callResultToFetch = new CallResult();

            Mock<IFormatter> formatterMock = new Mock<IFormatter>();
            formatterMock
                .Setup(x => x.Deserialize(It.IsAny<Stream>()))
                .Returns(callResultToFetch);
            Mock<IWrapperProcessStarter> wrapperProcessStarterMock = new Mock<IWrapperProcessStarter>();
            Mock<PipeStreamFactory> pipeStreamFactoryMock = GetPipeStreamFactoryMock();

            PipeToken pipeToken = new PipeToken(Guid.NewGuid().ToString());
            IPipeConnector pipeConnector = new PipeConnector(formatterMock.Object, wrapperProcessStarterMock.Object, pipeStreamFactoryMock.Object, pipeToken);

            pipeConnector.Dispose();

            formatterMock.Verify(mock => mock.Serialize(It.IsAny<PipeStream>(), It.Is<CallData>(actual => actual.Status.Equals(KeepAliveStatus.Close))), Times.AtLeast(1));
        }
    }
}
