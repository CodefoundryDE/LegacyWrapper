using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using Castle.DynamicProxy;
using LegacyWrapper.Common.Attributes;
using LegacyWrapper.Common.Serialization;
using LegacyWrapper.ErrorHandling;
using LegacyWrapperClient.Client;
using LegacyWrapperClient.Configuration;
using LegacyWrapperClient.DynamicProxy;
using LegacyWrapperClient.Transport;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;

namespace LegacyWrapperTest.LegacyWrapperClient.DynamicProxy
{
    [LegacyDllImport("TESTLIBRARY")]
    internal interface IMockInterface
    {
        [LegacyDllMethod]
        void TestMethod();
    }

    internal interface IMockInterfaceWithoutAttribute
    {
        [LegacyDllMethod]
        void TestMethod();
    }

    [LegacyDllImport("TESTLIBRARY")]
    internal interface IMockInterfaceWithoutMethodAttribute
    {
        void TestMethod();
    }

    [TestClass]
    public class WrapperClientInterceptorTest
    {
        private const string MockLibraryName = "TESTLIBRARY";

        private static readonly Type MockInterfaceType = typeof(IMockInterface);
        private static readonly Type MockInterfaceTypeWithoutAttribute = typeof(IMockInterfaceWithoutAttribute);
        private static readonly Type MockInterfaceTypeWithoutMethodAttribute = typeof(IMockInterfaceWithoutMethodAttribute);
        private static readonly CallResult ReturnedCallResult = new CallResult();

        private Mock<MethodInfo> _methodInfoMock;
        private Mock<IInvocation> _invocationMock;
        private Mock<IPipeConnector> _pipeConnectorMock;
        private Mock<WrapperClient> _wrapperClientMock;
        private Mock<ILibraryNameProvider> _libraryNameProviderMock;

        [TestInitialize]
        public void TestInitialize()
        {
            _methodInfoMock = new Mock<MethodInfo>();
            _methodInfoMock
                .SetupGet(mock => mock.Name)
                .Returns("TestMethod");
        
            _invocationMock = new Mock<IInvocation>();
            _invocationMock
                .SetupGet(mock => mock.Method)
                .Returns(_methodInfoMock.Object);

            _pipeConnectorMock = new Mock<IPipeConnector>();

            _wrapperClientMock = new Mock<WrapperClient>(_pipeConnectorMock.Object);
            _wrapperClientMock
                .Setup(mock => mock.InvokeInternal(It.IsAny<CallData>()))
                .Returns(ReturnedCallResult); ;
            _wrapperClientMock
                .As<IDisposable>()
                .Setup(mock => mock.Dispose())
                .Verifiable();

            _libraryNameProviderMock = new Mock<ILibraryNameProvider>();
            _libraryNameProviderMock
                .Setup(mock => mock.GetLibraryName(It.IsAny<LegacyDllImportAttribute>()))
                .Returns(MockLibraryName);
        }

        [TestMethod]
        public void TestInterceptorRetrievesReturnValue()
        {
            _methodInfoMock
                .Setup(mock => mock.GetCustomAttributes(It.IsAny<Type>(), It.IsAny<bool>()))
                .Returns(new object[] { new LegacyDllMethodAttribute() });
            WrapperClientInterceptor interceptor = new WrapperClientInterceptor(MockInterfaceType, _wrapperClientMock.Object, _libraryNameProviderMock.Object);

            interceptor.Intercept(_invocationMock.Object);

            _invocationMock.VerifySet(mock => mock.ReturnValue = ReturnedCallResult, Times.Once);
        }

        [TestMethod, ExpectedException(typeof(LegacyWrapperException))]
        public void TestThrowsOnMissingInterfaceAttribute()
        {
            WrapperClientInterceptor interceptor = new WrapperClientInterceptor(MockInterfaceTypeWithoutAttribute, _wrapperClientMock.Object, _libraryNameProviderMock.Object);

            interceptor.Intercept(_invocationMock.Object);
        }

        [TestMethod, ExpectedException(typeof(LegacyWrapperException))]
        public void TestThrowsOnMissingMethodAttribute()
        {
            WrapperClientInterceptor interceptor = new WrapperClientInterceptor(MockInterfaceTypeWithoutMethodAttribute, _wrapperClientMock.Object, _libraryNameProviderMock.Object);

            interceptor.Intercept(_invocationMock.Object);
        }

        [TestMethod]
        public void TestInterceptorRetrievesLibraryName()
        {
            _methodInfoMock
                .Setup(mock => mock.GetCustomAttributes(It.IsAny<Type>(), It.IsAny<bool>()))
                .Returns(new object[] { new LegacyDllMethodAttribute() });
            WrapperClientInterceptor interceptor = new WrapperClientInterceptor(MockInterfaceType, _wrapperClientMock.Object, _libraryNameProviderMock.Object);

            interceptor.Intercept(_invocationMock.Object);

            _libraryNameProviderMock.Verify(mock => mock.GetLibraryName(It.IsAny<LegacyDllImportAttribute>()), Times.Once);
        }

        [TestMethod, ExpectedException(typeof(ObjectDisposedException))]
        public void TestInterceptorCallsDispose()
        {
            Mock<MethodInfo> methodInfoMock = new Mock<MethodInfo>();
            methodInfoMock
                .SetupGet(mock => mock.Name)
                .Returns(nameof(WrapperClient.Dispose));
            _invocationMock
                .SetupGet(mock => mock.Method)
                .Returns(methodInfoMock.Object);
            WrapperClientInterceptor interceptor = new WrapperClientInterceptor(MockInterfaceType, _wrapperClientMock.Object, _libraryNameProviderMock.Object);


            interceptor.Intercept(_invocationMock.Object);

            // Should throw ObjectDisposedException now
            interceptor.Intercept(_invocationMock.Object);
        }
    }
}
