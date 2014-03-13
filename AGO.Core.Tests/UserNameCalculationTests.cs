using AGO.Core.Model.Security;
using NUnit.Framework;

namespace AGO.Core.Tests
{
	[TestFixture]
	public class UserNameCalculationTests
	{
		[Test]
		public void FullNameCalculatesForAnyParts()
		{
			var u = new UserModel { LastName = "Ivanov", FirstName = "Petr" };
			var fu = new UserModel { FirstName = "Petr" };
			var lu = new UserModel { LastName = "Ivanov" };
			var eu = new UserModel { FirstName = null, LastName = "  "};
			var nu = new UserModel();

			Assert.AreEqual("Petr Ivanov", u.FullName);
			Assert.AreEqual("Petr", fu.FullName);
			Assert.AreEqual("Ivanov", lu.FullName);
			Assert.AreEqual(eu.FullName, string.Empty);
			Assert.IsNull(nu.FullName);
		}
	}
}