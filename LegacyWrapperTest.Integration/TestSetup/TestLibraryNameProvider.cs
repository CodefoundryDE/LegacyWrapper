using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LegacyWrapper.Common.Attributes;
using LegacyWrapperClient.Architecture;
using LegacyWrapperClient.Configuration;
using LegacyWrapperClient.DynamicProxy;

namespace LegacyWrapperTest.Integration.TestSetup
{
    class TestLibraryNameProvider : ILibraryNameProvider
    {
        public string GetLibraryName(LegacyDllImportAttribute attribute)
        {
            if (Environment.Is64BitProcess)
            {
                return @"TestLibrary\LegacyWrapperTestDll32.dll";
            }
            else
            {
                return @"TestLibrary\LegacyWrapperTestDll64.dll";
            }
        }
    }
}
