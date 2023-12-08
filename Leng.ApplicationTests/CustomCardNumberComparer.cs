using Leng.BlazorServer.Pages;

namespace Leng.BlazorServer.Tests.Pages
{
    [TestFixture]
    public class CustomCardNumberComparerTests
    {
        private CardSheet.CustomCardNumberComparer _comparer;

        [SetUp]
        public void SetUp()
        {
            _comparer = new CardSheet.CustomCardNumberComparer();
        }

        [Test]
        public void Compare_RegularNumbers_ReturnsExpectedOrder()
        {
            string number1 = "10";
            string number2 = "2";

            var result = _comparer.Compare(number1, number2);

            // Assert that 10 is considered greater than 2
            Assert.That(result, Is.GreaterThan(0));
        }

        [Test]
        public void Compare_WithSpecialCharacters_ReturnsExpectedOrder()
        {
            string number1 = "2a";
            string number2 = "2";

            var result = _comparer.Compare(number1, number2);

            // Assert that "2a" is considered greater than "2"
            Assert.That(result, Is.GreaterThan(0));
        }

        [Test]
        public void Compare_IdenticalNumbers_ReturnsZero()
        {
            string number1 = "10a";
            string number2 = "10a";

            var result = _comparer.Compare(number1, number2);

            // Identical numbers should return 0
            Assert.That(result, Is.EqualTo(0));
        }
    }
}
