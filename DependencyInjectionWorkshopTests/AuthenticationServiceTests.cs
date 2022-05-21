using DependencyInjectionWorkshop.Models;

namespace DependencyInjectionWorkshopTests
{
    [TestFixture]
    public class AuthenticationServiceTests
    {
        [Test]
        public void is_valid()
        {
            var authenticationService = new AuthenticationService();
            var verify = authenticationService.Verify("andy", "1234", "0000");
            Assert.AreEqual(verify, true);
        }
    }
}