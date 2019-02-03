using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LegacyWrapper.Common.Token;
using LegacyWrapperClient.Token;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LegacyWrapperTest.LegacyWrapperClient.Token
{
    [TestClass]
    public class GuidTokenGeneratorTest
    {
        [TestMethod]
        public void TestCreatesGuid()
        {
            ITokenGenerator generator = new GuidTokenGenerator();

            PipeToken token = generator.GenerateToken();

            Guid result = Guid.Parse(token.Token);
            Assert.AreEqual(token.Token, result.ToString());
        }

        [TestMethod]
        public void TestCreatesUniqueTokens()
        {
            ITokenGenerator generator = new GuidTokenGenerator();

            ICollection tokens = Enumerable.Range(1, 100)
                .Select(i => generator.GenerateToken())
                .ToList();

            CollectionAssert.AllItemsAreUnique(tokens);
        }
    }
}
