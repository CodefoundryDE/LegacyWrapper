using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LegacyWrapperClient.Architecture;
using LegacyWrapperClient.DynamicProxy;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LegacyWrapperTest.Client
{
    public abstract class LegacyWrapperTestBase
    {
        protected TargetArchitecture ArchitectureToLoad;

        /// <summary>
        /// Here we're going to determine if we're running as 32 bit or 64 bit process - and use the opposing wrapper.
        /// However, there's no way to tell Visual Studio or ReSharper that they should run unit tests on both architectures - 
        /// we have to do that manually.
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            if (Environment.Is64BitProcess)
            {
                ArchitectureToLoad = TargetArchitecture.X86;
                WrapperClientInterceptor.OverrideLibraryName = @"TestLibrary\LegacyWrapperTestDll32.dll";
            }
            else
            {
                ArchitectureToLoad = TargetArchitecture.Amd64;
                WrapperClientInterceptor.OverrideLibraryName = @"TestLibrary\LegacyWrapperTestDll64.dll";
            }
        }
    }
}
