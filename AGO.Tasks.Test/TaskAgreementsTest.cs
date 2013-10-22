﻿using System;
using System.Linq;
using AGO.Core.Model.Security;
using AGO.Home.Model.Projects;
using AGO.Tasks.Controllers;
using AGO.Tasks.Model.Task;
using NHibernate;
using NUnit.Framework;

namespace AGO.Tasks.Test
{
	/// <summary>
	/// Тесты управления согласованием задачи
	/// </summary>
	[TestFixture]
	public class TaskAgreementsTest: AbstractTest
	{
		private TasksController controller;
		private TaskModel task;
		private ProjectParticipantModel pIvanov;
		private ProjectParticipantModel pPetrov;

		[TestFixtureSetUp]
		public new void Init()
		{
			base.Init();
			
			controller = IocContainer.GetInstance<TasksController>();

			var project = Session.QueryOver<ProjectModel>().Where(m => m.ProjectCode == TestProject).SingleOrDefault();
			Assert.IsNotNull(project);
			var ivanov = Session.QueryOver<UserModel>().Where(m => m.Login == "user1@agosystems.com").SingleOrDefault();
			Assert.IsNotNull(ivanov);
			var petrov = Session.QueryOver<UserModel>().Where(m => m.Login == "user2@agosystems.com").SingleOrDefault();
			Assert.IsNotNull(petrov);

			pIvanov = new ProjectParticipantModel
			          	{
			          		Project = project,
			          		User = ivanov,
			          		GroupName = "Executors",
			          		IsDefaultGroup = true
			          	};
			_CrudDao.Store(pIvanov);
			pPetrov = new ProjectParticipantModel
			          	{
			          		Project = project,
			          		User = petrov,
			          		GroupName = "Executors",
			          		IsDefaultGroup = true
			          	};
			_CrudDao.Store(pPetrov);
			project.Participants.Add(pIvanov);
			project.Participants.Add(pPetrov);

			_SessionProvider.FlushCurrentSession();
		}

		[TestFixtureTearDown]
		public new void Cleanup()
		{
			base.Cleanup();
		}

		[SetUp]
		public void SetUp()
		{
			task = M.Task(1);
			_SessionProvider.FlushCurrentSession();
		}

		[TearDown]
		public new void TearDown()
		{
			base.TearDown();
		}

		//add
		[Test]
		public void AddAgreementReturnSuccess()
		{
			var dd = new DateTime(2013, 01, 01, 0, 0, 0, DateTimeKind.Utc);
			controller.AddAgreemer(task.Id, pIvanov.Id, dd);
			_SessionProvider.FlushCurrentSession();

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
			    Creator = CurrentUser,
			    Task = task,
			    Agreemer = pIvanov
			};
			_CrudDao.Store(agr);
			task.Agreements.Add(agr);
			_CrudDao.Store(task);
			_SessionProvider.FlushCurrentSession();

			controller.AddAgreemer(task.Id, pIvanov.Id);
		}
		//can't add to closed
		[Test, ExpectedException(typeof(CanNotAddAgreemerToClosedTaskException))]
		public void AddAgreementToClosedThrow()
		{
			task.ChangeStatus(TaskStatus.Closed, CurrentUser);
			_CrudDao.Store(task);
			_SessionProvider.FlushCurrentSession();

			controller.AddAgreemer(task.Id, pIvanov.Id);
			_SessionProvider.FlushCurrentSession();
		}

		//remove
		[Test]
		public void RemoveAgreementReturnTrue()
		{
			var agr = new TaskAgreementModel
			{
				Creator = CurrentUser,
				Task = task,
				Agreemer = pIvanov
			};
			_CrudDao.Store(agr);
			task.Agreements.Add(agr);
			_CrudDao.Store(task);
			_SessionProvider.FlushCurrentSession();

			var res = controller.RemoveAgreement(task.Id, agr.Id);
			_SessionProvider.FlushCurrentSession();

			Assert.IsTrue(res);
			agr = Session.Get<TaskAgreementModel>(agr.Id);
			Assert.IsNull(agr);
		}
		//can't remove not existing

		[Test]
		public void RemoveNonExistingReturnFalse()
		{
			var res = controller.RemoveAgreement(task.Id, Guid.NewGuid());
			_SessionProvider.FlushCurrentSession();

			Assert.IsFalse(res);
		}
		//can't remove from closed
		[Test, ExpectedException(typeof(CanNotRemoveAgreemerFromClosedTaskException))]
		public void RemoveAgreementFromClosedThrow()
		{
			var agr = new TaskAgreementModel
			{
				Creator = CurrentUser,
				Task = task,
				Agreemer = pIvanov
			};
			_CrudDao.Store(agr);
			task.Agreements.Add(agr);
			task.ChangeStatus(TaskStatus.Closed, CurrentUser);
			_CrudDao.Store(task);
			_SessionProvider.FlushCurrentSession();

			controller.RemoveAgreement(task.Id, agr.Id);
			_SessionProvider.FlushCurrentSession();
		}

		//agree (with comment)
		[Test]
		public void AgreeReturnSuccess()
		{
			var agr = new TaskAgreementModel
			{
				Creator = CurrentUser,
				Task = task,
				Agreemer = pIvanov
			};
			_CrudDao.Store(agr);
			task.Agreements.Add(agr);
			_CrudDao.Store(task);
			_SessionProvider.FlushCurrentSession();

			Logout();
			Login(pIvanov.User.Login, "1");
			controller.AgreeTask(task.Id, "good job, bro");
			_SessionProvider.FlushCurrentSession();

			Logout();
			LoginAdmin();
			agr = Session.Get<TaskAgreementModel>(agr.Id);
			Assert.IsTrue(agr.Done);
			Assert.IsNotNull(agr.AgreedAt);
			Assert.AreEqual(DateTime.Today, agr.AgreedAt.Value.Date);
			Assert.AreEqual("good job, bro", agr.Comment);
		}

		//can't agree for other person
		[Test, ExpectedException(typeof(CurrentUserIsNotAgreemerInTaskException))]
		public void AgreeFromOtherPersonThrow()
		{
			var agr = new TaskAgreementModel
			{
				Creator = CurrentUser,
				Task = task,
				Agreemer = pIvanov
			};
			_CrudDao.Store(agr);
			task.Agreements.Add(agr);
			_CrudDao.Store(task);
			_SessionProvider.FlushCurrentSession();

			Logout();
			Login(pPetrov.User.Login, "1");
			try
			{
				controller.AgreeTask(task.Id, "good job, bro");
				_SessionProvider.FlushCurrentSession();
			}
			finally
			{
				Logout();
				LoginAdmin();
			}
		}

		//can't agree closed
		[Test, ExpectedException(typeof(CanNotAgreeClosedTaskException))]
		public void AgreeClosedThrow()
		{
			var agr = new TaskAgreementModel
			{
				Creator = CurrentUser,
				Task = task,
				Agreemer = pIvanov
			};
			_CrudDao.Store(agr);
			task.Agreements.Add(agr);
			task.ChangeStatus(TaskStatus.Closed, CurrentUser);
			_CrudDao.Store(task);
			_SessionProvider.FlushCurrentSession();

			Logout();
			Login(pIvanov.User.Login, "1");
			try
			{
				controller.AgreeTask(task.Id, "good job, bro");
				_SessionProvider.FlushCurrentSession();
			}
			finally
			{
				Logout();
				LoginAdmin();
			}
		}

		//revoke agreement
		[Test]
		public void RevokeAgreementReturnSuccess()
		{
			var agr = new TaskAgreementModel
			{
				Creator = CurrentUser,
				Task = task,
				Agreemer = pIvanov,
				Done = true,
				AgreedAt = DateTime.Now,
				Comment = "good job, bro"
			};
			_CrudDao.Store(agr);
			task.Agreements.Add(agr);
			_CrudDao.Store(task);
			_SessionProvider.FlushCurrentSession();

			Logout();
			Login(pIvanov.User.Login, "1");
			controller.RevokeAgreement(task.Id);
			_SessionProvider.FlushCurrentSession();
			
			Logout();
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
				Creator = CurrentUser,
				Task = task,
				Agreemer = pIvanov,
				Done = true,
				AgreedAt = DateTime.Now,
				Comment = "good job, bro"
			};
			_CrudDao.Store(agr);
			task.Agreements.Add(agr);
			_CrudDao.Store(task);
			_SessionProvider.FlushCurrentSession();

			Logout();
			Login(pPetrov.User.Login, "1");
			try
			{
				controller.RevokeAgreement(task.Id);
				_SessionProvider.FlushCurrentSession();
			}
			finally
			{
				Logout();
				LoginAdmin();
			}
		}

		//can't revoke from closed
		[Test, ExpectedException(typeof(CanNotRevokeAgreementFromClosedTaskException))]
		public void RevokeFromClosedThrow()
		{
			var agr = new TaskAgreementModel
			{
				Creator = CurrentUser,
				Task = task,
				Agreemer = pIvanov,
				Done = true,
				AgreedAt = DateTime.Now,
				Comment = "good job, bro"
			};
			_CrudDao.Store(agr);
			task.Agreements.Add(agr);
			task.ChangeStatus(TaskStatus.Closed, CurrentUser);
			_CrudDao.Store(task);
			_SessionProvider.FlushCurrentSession();

			Logout();
			Login(pIvanov.User.Login, "1");
			try
			{
				controller.RevokeAgreement(task.Id);
				_SessionProvider.FlushCurrentSession();
			}
			finally
			{
				Logout();
				LoginAdmin();
			}
		}
	}
}