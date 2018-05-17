using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LegacyWrapperClient.Architecture;
using LegacyWrapperClient.Client;
using LegacyWrapperClient.Configuration;
using LegacyWrapperClient.DynamicProxy;
using LegacyWrapperTest.Interface;
using LegacyWrapperTest.TestSetup;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LegacyWrapperTest.Client
{
    /// <summary>
    /// Here we're going to determine if we're running as 32 bit or 64 bit process - and use the opposing wrapper.
    /// However, there's no way to tell Visual Studio or ReSharper that they should run unit tests on both architectures - 
    /// we have to do that manually.
    /// </summary>
    [TestClass]
    public abstract class LegacyWrapperTestBase
    {
        protected TargetArchitecture ArchitectureToLoad;

        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext context)
        {
            WrapperProxyFactory<ITestDll>.InjectionKernel.Rebind<ILibraryNameProvider>().To<TestLibraryNameProvider>();
        }

        [TestInitialize]
        public void TestInitialize()
        {
            if (Environment.Is64BitProcess)
            {
                ArchitectureToLoad = TargetArchitecture.X86;
            }
            else
            {
                ArchitectureToLoad = TargetArchitecture.Amd64;
            }
        }
    }
}
