using NUnit.Framework;

namespace Favalet.Tokens
{
    [TestFixture]
    public sealed class IdentityTokenTest
    {
        [Test]
        public void Identity()
        {
            var actual = Token.Identity("abc");

            Assert.AreEqual("abc", actual.ToString());
        }
    }
}
