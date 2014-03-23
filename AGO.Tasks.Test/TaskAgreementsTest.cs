using System;
using System.Linq;
using AGO.Core.Model.Security;
using AGO.Core.Model.Projects;
using AGO.Tasks.Controllers;
using AGO.Tasks.Model.Task;
using NUnit.Framework;

namespace AGO.Tasks.Test
{
	/// <summary>
	/// Тесты управления согласованием задачи
	/// </summary>
	public class TaskAgreementsTest: AbstractTest
	{
		private TasksController controller;
		private TaskModel task;
		private UserModel ivanov;
		private UserModel petrov;
		private ProjectMemberModel pIvanov;
//		private ProjectMemberModel pPetrov;

		public override void FixtureSetUp()
		{
			base.FixtureSetUp();
			
			controller = IocContainer.GetInstance<TasksController>();

			ivanov = LoginToUser("user1@apinet-test.com");
			petrov = LoginToUser("user2@apinet-test.com");
			var project = FM.ProjectFromCode(TestProject);
			pIvanov = FPM.Member(project, ivanov, TaskProjectRoles.Manager);
			/*pPetrov = */FPM.Member(project, petrov, TaskProjectRoles.Manager);//need member, but implicitly (and manager, not executor, see AgreeFromOtherPersonThrow test)
		}

		public override void SetUp()
		{
			base.SetUp();

			task = M.Task(1);
		}

		//add
		[Test]
		public void AddAgreementReturnSuccess()
		{
			var dd = new DateTime(2013, 01, 01, 0, 0, 0, DateTimeKind.Utc);
			controller.AddAgreemer(TestProject, task.Id, pIvanov.Id, dd);
			Session.Flush();

			Session.Refresh(task);
			Assert.AreEqual(1, task.Agreements.Count);
			Assert.AreEqual(pIvanov, task.Agreements.First().Agreemer);
			Assert.AreEqual(dd, task.Agreements.First().DueDate);
		}

		//can't add duplicate
		[Test, ExpectedException(typeof(AgreemerAlreadyAssignedToTaskException))]
		public void AddDuplicateAgreementThrow()
		{
			var agr = new TaskAgreementModel
			{
				Creator = M.MemberFromUser(task.ProjectCode, CurrentUser),
			    Task = task,
			    Agreemer = pIvanov
			};
			ProjDao.Store(agr);
			task.Agreements.Add(agr);
			ProjDao.Store(task);
			Session.Flush();

			controller.AddAgreemer(TestProject, task.Id, pIvanov.Id);
		}
		//can't add to closed
		[Test, ExpectedException(typeof(CanNotAddAgreemerToClosedTaskException))]
		public void AddAgreementToClosedThrow()
		{
			task.ChangeStatus(TaskStatus.Closed, M.MemberFromUser(task.ProjectCode, CurrentUser));
			ProjDao.Store(task);
			Session.Flush();

			controller.AddAgreemer(TestProject, task.Id, pIvanov.Id);
		}

		//remove
		[Test]
		public void RemoveAgreementReturnTrue()
		{
			var agr = new TaskAgreementModel
			{
				Creator = M.MemberFromUser(task.ProjectCode, CurrentUser),
				Task = task,
				Agreemer = pIvanov
			};
			ProjDao.Store(agr);
			task.Agreements.Add(agr);
			ProjDao.Store(task);
			Session.Flush();

			Login(ivanov.Email);
			var res = controller.RemoveAgreement(task.ProjectCode, task.Id, agr.Id);
			Session.Flush();

			Assert.IsTrue(res);
			agr = Session.Get<TaskAgreementModel>(agr.Id);
			Assert.IsNull(agr);
		}
		//can't remove not existing

		[Test]
		public void RemoveNonExistingReturnFalse()
		{
			var res = controller.RemoveAgreement(task.ProjectCode, task.Id, Guid.NewGuid());
			Session.Flush();

			Assert.IsFalse(res);
		}
		//can't remove from closed
		[Test, ExpectedException(typeof(CanNotRemoveAgreemerFromClosedTaskException))]
		public void RemoveAgreementFromClosedThrow()
		{
			var agr = new TaskAgreementModel
			{
				Creator = M.MemberFromUser(task.ProjectCode, CurrentUser),
				Task = task,
				Agreemer = pIvanov
			};
			ProjDao.Store(agr);
			task.Agreements.Add(agr);
			task.ChangeStatus(TaskStatus.Closed, M.MemberFromUser(task.ProjectCode, CurrentUser));
			ProjDao.Store(task);
			Session.Flush();

			Login(ivanov.Email);
			controller.RemoveAgreement(task.ProjectCode, task.Id, agr.Id);
		}

		//agree (with comment)
		[Test]
		public void AgreeReturnSuccess()
		{
			var agr = new TaskAgreementModel
			{
				Creator = M.MemberFromUser(task.ProjectCode, CurrentUser),
				Task = task,
				Agreemer = pIvanov
			};
			ProjDao.Store(agr);
			task.Agreements.Add(agr);
			ProjDao.Store(task);
			Session.Flush();

			Login(ivanov.Email);
			controller.AgreeTask(task.ProjectCode, task.Id, "good job, bro");
			Session.Flush();

			LoginAdmin();
			agr = Session.Get<TaskAgreementModel>(agr.Id);
			Assert.IsTrue(agr.Done);
			Assert.IsNotNull(agr.AgreedAt);
// ReSharper disable once PossibleInvalidOperationException
			Assert.AreEqual(DateTime.Today, agr.AgreedAt.Value.ToLocalTime().Date);
			Assert.AreEqual("good job, bro", agr.Comment);
		}

		//can't agree for other person
		[Test, ExpectedException(typeof(CurrentUserIsNotAgreemerInTaskException))]
		public void AgreeFromOtherPersonThrow()
		{
			var agr = new TaskAgreementModel
			{
				Creator = M.MemberFromUser(task.ProjectCode, CurrentUser),
				Task = task,
				Agreemer = pIvanov
			};
			ProjDao.Store(agr);
			task.Agreements.Add(agr);
			ProjDao.Store(task);
			Session.Flush();

			Login(petrov.Email);
			try
			{
				controller.AgreeTask(task.ProjectCode, task.Id, "good job, bro");
			}
			finally
			{
				LoginAdmin();
			}
		}

		//can't agree closed
		[Test, ExpectedException(typeof(CanNotAgreeClosedTaskException))]
		public void AgreeClosedThrow()
		{
			var agr = new TaskAgreementModel
			{
				Creator = M.MemberFromUser(task.ProjectCode, CurrentUser),
				Task = task,
				Agreemer = pIvanov
			};
			ProjDao.Store(agr);
			task.Agreements.Add(agr);
			task.ChangeStatus(TaskStatus.Closed, M.MemberFromUser(task.ProjectCode, CurrentUser));
			ProjDao.Store(task);
			Session.Flush();

			Login(ivanov.Email);
			try
			{
				controller.AgreeTask(task.ProjectCode, task.Id, "good job, bro");
			}
			finally
			{
				LoginAdmin();
			}
		}

		//revoke agreement
		[Test]
		public void RevokeAgreementReturnSuccess()
		{
			var agr = new TaskAgreementModel
			{
				Creator = M.MemberFromUser(task.ProjectCode, CurrentUser),
				Task = task,
				Agreemer = pIvanov,
				Done = true,
				AgreedAt = DateTime.Now,
				Comment = "good job, bro"
			};
			ProjDao.Store(agr);
			task.Agreements.Add(agr);
			ProjDao.Store(task);
			Session.Flush();

			Login(ivanov.Email);
			controller.RevokeAgreement(task.ProjectCode, task.Id);
			Session.Flush();
			
			LoginAdmin();
			agr = Session.Get<TaskAgreementModel>(agr.Id);
			Assert.IsFalse(agr.Done);
			Assert.IsNull(agr.AgreedAt);
			Assert.IsNull(agr.Comment);
		}

		//can't revoke from other user
		[Test, ExpectedException(typeof(CurrentUserIsNotAgreemerInTaskException))]
		public void RevokeAgreementFromOtherPersonThrow()
		{
			var agr = new TaskAgreementModel
			{
				Creator = M.MemberFromUser(task.ProjectCode, CurrentUser),
				Task = task,
				Agreemer = pIvanov,
				Done = true,
				AgreedAt = DateTime.Now,
				Comment = "good job, bro"
			};
			ProjDao.Store(agr);
			task.Agreements.Add(agr);
			ProjDao.Store(task);
			Session.Flush();

			Login(petrov.Email);
			try
			{
				controller.RevokeAgreement(task.ProjectCode, task.Id);
			}
			finally
			{
				LoginAdmin();
			}
		}

		//can't revoke from closed
		[Test, ExpectedException(typeof(CanNotRevokeAgreementFromClosedTaskException))]
		public void RevokeFromClosedThrow()
		{
			var agr = new TaskAgreementModel
			{
				Creator = M.MemberFromUser(task.ProjectCode, CurrentUser),
				Task = task,
				Agreemer = pIvanov,
				Done = true,
				AgreedAt = DateTime.Now,
				Comment = "good job, bro"
			};
			ProjDao.Store(agr);
			task.Agreements.Add(agr);
			task.ChangeStatus(TaskStatus.Closed, M.MemberFromUser(task.ProjectCode, CurrentUser));
			ProjDao.Store(task);
			Session.Flush();

			Login(ivanov.Email);
			try
			{
				controller.RevokeAgreement(task.ProjectCode, task.Id);
			}
			finally
			{
				LoginAdmin();
			}
		}
	}
}