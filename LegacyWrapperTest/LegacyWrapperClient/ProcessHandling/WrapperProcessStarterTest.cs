using System;
using System.Diagnostics;
using LegacyWrapper.Common.Token;
using LegacyWrapperClient.Configuration;
using LegacyWrapperClient.ProcessHandling;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace LegacyWrapperTest.LegacyWrapperClient.ProcessHandling
{
    [TestClass]
    public class WrapperProcessStarterTest
    {
        private readonly string MockProcessName = "PROCESSNAME";
        private readonly PipeToken MockToken = new PipeToken(Guid.NewGuid().ToString());

        private Mock<MockableProcess> _processMock;
        private Mock<IProcessFactory> _processFactoryMock;
        private Mock<IWrapperExecutableNameProvider> _nameProviderMock;

        [TestInitialize]
        public void TestInitialize()
        {
            _processMock = new Mock<MockableProcess>();
            _processMock.Setup(mock => mock
                        .Start())
                        .Returns(true);

            _processFactoryMock = new Mock<IProcessFactory>();
            _processFactoryMock.Setup(mock => mock
                               .GetProcess(It.IsAny<string>(), It.IsAny<string>()))
                               .Returns(_processMock.Object);

            _nameProviderMock = new Mock<IWrapperExecutableNameProvider>();
            _nameProviderMock.Setup(mock => mock
                             .GetWrapperExecutableName())
                             .Returns(MockProcessName);
        }

        [TestMethod]
        public void TestStartsProcess()
        {
            WrapperProcessStarter processStarter = new WrapperProcessStarter(_nameProviderMock.Object, MockToken, _processFactoryMock.Object);

            processStarter.StartWrapperProcess();

            _processFactoryMock.Verify(mock => mock.GetProcess(It.Is<string>(actual => actual == MockProcessName), It.Is<string>(actual => actual == MockToken.Token)), Times.Once);
            _processMock.Verify(mock => mock.Start(), Times.Once);
        }
    }
}
