using AGO.Core.Model.Projects;
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
			base.SetupTestProject();

			admin = LoginToUser("admin@apinet-test.com");
			projAdmin = LoginToUser("user1@apinet-test.com");
			projManager = LoginToUser("user2@apinet-test.com");
			projExecutor = LoginToUser("user3@apinet-test.com");
			notMember = LoginToUser("artem1@facebook.com");

			var project = MainSession.QueryOver<ProjectModel>().Where(m => m.ProjectCode == TestProject).SingleOrDefault();
			//remove member, added in base class
			FPM.DropCreated();
			//and add needed members
			FPM.Member(project, projAdmin, BaseProjectRoles.Administrator);
			FPM.Member(project, projManager, TaskProjectRoles.Manager);
			FPM.Member(project, projExecutor, TaskProjectRoles.Executor);
			MainSession.Update(project);
			MainSession.Flush();
			MainSession.Clear();
		}
	}
}