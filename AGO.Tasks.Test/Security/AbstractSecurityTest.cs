using AGO.Core.Model.Security;

namespace AGO.Tasks.Test.Security
{
	public abstract class AbstractSecurityTest: AbstractTest
	{
		protected UserModel admin;
		protected UserModel projAdmin;
		protected UserModel projManager;
		protected UserModel projExecutor;
		protected UserModel notMember;

		protected override void SetupTestProject()
		{
			admin = LoginToUser("admin@apinet-test.com");
			projAdmin = LoginToUser("user1@apinet-test.com");
			projManager = LoginToUser("user2@apinet-test.com");
			projExecutor = LoginToUser("user3@apinet-test.com");
			notMember = LoginToUser("artem1@facebook.com");
			FM.Project(TestProject, creator:admin);
			FM.Member(TestProject, projAdmin, BaseProjectRoles.Administrator);
			FM.Member(TestProject, projManager, TaskProjectRoles.Manager);
			FM.Member(TestProject, projExecutor, TaskProjectRoles.Executor);
		}
	}
}