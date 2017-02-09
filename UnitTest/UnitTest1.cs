using System;
using NUnit.Framework;

namespace UnitTest
{
    [TestFixture]
    public class UnitTest1
    {
        [Test]
        public void TestMethod1()
        {
            Assert.AreEqual(1, 1);
            ObjectAssert.Equal<ListMapper>(null, null);
        }
    }

    public static class ObjectAssert
    {
        public static bool Equal<T>(T expected, T actual)
        {
            return true;
        }
    }

}
