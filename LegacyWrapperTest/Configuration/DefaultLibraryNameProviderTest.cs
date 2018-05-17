using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LegacyWrapper.Common.Attributes;
using LegacyWrapperClient.Configuration;
using LegacyWrapperTest.TestSetup;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LegacyWrapperTest.Configuration
{
    [TestClass]
    public class DefaultLibraryNameProviderTest
    {
        private const string TestLibraryName = "TESTLIBRARYNAME";
        private static readonly LegacyDllImportAttribute TestLegacyDllImportAttribute = new LegacyDllImportAttribute(TestLibraryName);

        [TestMethod]
        public void TestReturnsPassedLibraryName()
        {
            ILibraryNameProvider provider = new DefaultLibraryNameProvider();

            string actualLibraryName = provider.GetLibraryName(TestLegacyDllImportAttribute);

            Assert.AreEqual(TestLibraryName, actualLibraryName);
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void TestThrowsExceptionOnNullArgument()
        {
            ILibraryNameProvider provider = new DefaultLibraryNameProvider();

            string actualLibraryName = provider.GetLibraryName(null);
        }
    }
}
