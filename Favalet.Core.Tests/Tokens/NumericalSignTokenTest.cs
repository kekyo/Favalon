using NUnit.Framework;

namespace Favalet.Tokens
{
    [TestFixture]
    public sealed class NumericalSignTokenTest
    {
        [Test]
        public void PlusSign()
        {
            var actual = Token.PlusSign();

            Assert.AreEqual("+", actual.ToString());
        }

        [Test]
        public void MinusSign()
        {
            var actual = Token.MinusSign();

            Assert.AreEqual("-", actual.ToString());
        }
    }
}
