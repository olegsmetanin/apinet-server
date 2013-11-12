using AGO.Core.Model.Security;
using NUnit.Framework;

namespace AGO.Core.Tests
{
	[TestFixture]
	public class UserNameCalculationTests
	{
		[Test]
		public void FullNameCalculatesForAllThreePart()
		{
			var u = new UserModel
			        	{
							LastName = "Ivanov",
			        		Name = "Petr",
							MiddleName = "Andreevich"
			        	};

			Assert.AreEqual(u.FullName, "Ivanov Petr Andreevich");
			Assert.AreEqual(u.FIO, "Ivanov P.A.");
		}

		[Test]
		public void FullNameCalculatesForNameAndLastNameOnly()
		{
			var u = new UserModel
			{
				LastName = "Connor",
				Name = "Jonh"
			};

			Assert.AreEqual(u.FullName, "Jonh Connor");
			Assert.AreEqual(u.FIO, "Connor J.");
		}

		[Test]
		public void BugFIONotCalculatedOnTestDataGeneration()
		{
			//extracted from Core TestDataService
			var user1 = new UserModel
			{
				Login = "user1@apinet-test.com",
				Active = true,
				LastName = "Bryan",
				Name = "Thomas",
				MiddleName = "",
				SystemRole = SystemRole.Member
			};

			Assert.AreEqual(user1.FullName, "Thomas Bryan");
			Assert.AreEqual(user1.FIO, "Bryan T.");
		}
	}
}