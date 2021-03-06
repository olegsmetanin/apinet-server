﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Web;
using AGO.Core;
using AGO.Core.Config;
using AGO.Core.Controllers;
using AGO.Core.Filters;
using AGO.Core.Model.Security;
using AGO.Core.Security;
using AGO.Tasks.Controllers;
using AGO.Tasks.Controllers.DTO;
using AGO.Tasks.Model.Dictionary;
using AGO.Tasks.Model.Task;
using NSubstitute;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace AGO.Tasks.Test.Security
{
	public class TasksControllerSecurityTests: AbstractSecurityTest
	{
		private string testFileStoreRoot;
		private TasksController controller;

		public override void FixtureSetUp()
		{
			base.FixtureSetUp();

			controller = IocContainer.GetInstance<TasksController>();
			testFileStoreRoot = Path.Combine(Path.GetTempPath(), "apinet_nunit");
			var uploadPath = Path.Combine(testFileStoreRoot, "upload");

			var config = new Dictionary<string, string>
			         	{
							{"UploadPath", uploadPath},
							{"FileStoreRoot", testFileStoreRoot}
						};
			var provider = new DictionaryKeyValueProvider(config);
			new KeyValueConfigProvider(provider).ApplyTo(controller);
			controller.Initialize();
		}

		[Test]
		public void OnlyMembersCanLookupTasks()
		{
			M.Task(1, executor: projManager);
			M.Task(2, executor: projExecutor);

			Func<UserModel, LookupEntry[]> action = u =>
			{
				Login(u.Email);
				return controller.LookupTasks(TestProject, null, 0).ToArray();
			};
			ReusableConstraint granted = Has.Length.EqualTo(2)
				.And.Exactly(1).Matches<LookupEntry>(e => e.Text == "t0-1")
				.And.Exactly(1).Matches<LookupEntry>(e => e.Text == "t0-2");
			ReusableConstraint grantedWithRestriction = Has.Length.EqualTo(1)
				.And.Exactly(1).Matches<LookupEntry>(e => e.Text == "t0-2");
			ReusableConstraint denied = Is.Empty;

			Assert.That(action(admin), denied);
			Assert.That(action(projAdmin), granted);
			Assert.That(action(projManager), granted);
			Assert.That(action(projExecutor), grantedWithRestriction);
			Assert.That(action(notMember), denied);
		}

		[Test]
		public void MembersCanLookupTasksWithGroupRestrictions()
		{
			M.Task(1, executor: projAdmin);
			M.Task(2, executor: projManager);
			M.Task(3, executor: projExecutor);

			Func<UserModel, LookupEntry[]> action = u =>
			{
				Login(u.Email);
				return controller.LookupTasks(TestProject, null, 0).ToArray();
			};

			ReusableConstraint allowed = Has.Length.EqualTo(3);
			ReusableConstraint restricted = Has.Length.EqualTo(1)
				.And.Exactly(1).Matches<LookupEntry>(e => e.Text == "t0-3");

			Assert.That(action(projAdmin), allowed);
			Assert.That(action(projManager), allowed);
			Assert.That(action(projExecutor), restricted);
		}

		[Test]
		public void OnlyMembersCanGetTasks()
		{
			M.Task(1, executor: projManager);
			M.Task(2, executor: projExecutor);

			Func<UserModel, TaskListItemDTO[]> action = u =>
			{
				Login(u.Email);
				return controller.GetTasks(TestProject, 
					Enumerable.Empty<IModelFilterNode>().ToList(),
					Enumerable.Empty<SortInfo>().ToList(),
					0, TaskPredefinedFilter.All).ToArray();
			};
			ReusableConstraint granted = Has.Length.EqualTo(2)
				.And.Exactly(1).Matches<TaskListItemDTO>(e => e.SeqNumber == "t0-1")
				.And.Exactly(1).Matches<TaskListItemDTO>(e => e.SeqNumber == "t0-2");
			ReusableConstraint grantedWithRestriction = Has.Length.EqualTo(1)
				.And.Exactly(1).Matches<TaskListItemDTO>(e => e.SeqNumber == "t0-2");
			ReusableConstraint denied = Throws.Exception.TypeOf<NoSuchProjectMemberException>();

			Assert.That(() => action(admin), denied);
			Assert.That(action(projAdmin), granted);
			Assert.That(action(projManager), granted);
			Assert.That(action(projExecutor), grantedWithRestriction);
			Assert.That(() => action(notMember), denied);
		}

		[Test]
		public void OnlyMembersCanGetTasksCount()
		{
			M.Task(1, executor: projManager);
			M.Task(2, executor: projExecutor);

			Func<UserModel, int> action = u =>
			{
				Login(u.Email);
				return controller.GetTasksCount(TestProject,
					Enumerable.Empty<IModelFilterNode>().ToList(),
					TaskPredefinedFilter.All);
			};
			ReusableConstraint granted = Is.EqualTo(2);
			ReusableConstraint grantedWithRestriction = Is.EqualTo(1);
			ReusableConstraint denied = Throws.Exception.TypeOf<NoSuchProjectMemberException>();

			Assert.That(() => action(admin), denied);
			Assert.That(action(projAdmin), granted);
			Assert.That(action(projManager), granted);
			Assert.That(action(projExecutor), grantedWithRestriction);
			Assert.That(() => action(notMember), denied);
		}

		[Test]
		public void OnlyMembersCanGetTaskDetails()
		{
			var task = M.Task(1, executor: projExecutor);

			Func<UserModel, TaskListItemDetailsDTO> action = u =>
			{
				Login(u.Email);
				return controller.GetTaskDetails(TestProject, task.SeqNumber);
			};
			ReusableConstraint granted = Is.Not.Null;
			ReusableConstraint denied = Throws.Exception.TypeOf<NoSuchProjectMemberException>();

			Assert.That(() => action(admin), denied);
			Assert.That(action(projAdmin), granted);
			Assert.That(action(projManager), granted);
			Assert.That(action(projExecutor), granted);
			Assert.That(() => action(notMember), denied);
		}

		[Test]
		public void OnlyMembersCanGetTask()
		{
			var task = M.Task(1, creator: projManager, executor: projExecutor);

			Func<UserModel, TaskViewDTO> action = u =>
			{
				Login(u.Email);
				return controller.GetTask(TestProject, task.SeqNumber);
			};
			ReusableConstraint granted = Is.Not.Null;
			ReusableConstraint denied = Throws.Exception.TypeOf<NoSuchProjectMemberException>();

			Assert.That(() => action(admin), denied);
			Assert.That(action(projAdmin), granted);
			Assert.That(action(projManager), granted);
			Assert.That(action(projExecutor), granted);
			Assert.That(() => action(notMember), denied);
		}

		[Test]
		public void OnlyAdminOrManagerCanCreateTask()
		{
			var tt = M.TaskType();
			var executorId = M.MemberFromUser(TestProject, projExecutor).Id;
			Func<UserModel, bool> action = u =>
			{
				Login(u.Email);
				var ur = controller.CreateTask(TestProject,
					new CreateTaskDTO {TaskType = tt.Id, Content = "nunit", Executors = new[] {executorId}});
				Session.Flush();
				if (ur.Validation.Success)
				{
					M.Track(Session.QueryOver<TaskModel>()
						.Where(m => m.ProjectCode == TestProject && m.SeqNumber == ur.Model)
						.SingleOrDefault());
				}
				return ur.Validation.Success;
			};
			ReusableConstraint granted = Is.True;
			ReusableConstraint denied = Is.False;

			Assert.That(action(admin), denied);
			Assert.That(action(projAdmin), granted);
			Assert.That(action(projManager), granted);
			Assert.That(action(projExecutor), denied);
			Assert.That(action(notMember), denied);
		}

		//TODO property-level security, when implemented, will require more fine grained tests
		[Test]
		public void OnlyMembersCanEditTask()
		{
			var task = M.Task(1, creator: projManager, executor:projExecutor);
			Func<UserModel, bool> action = u =>
			{
				Login(u.Email);
				var ur = controller.UpdateTask(TestProject,
					new PropChangeDTO
					{
						Id = task.Id, 
						Prop = "Content", 
						ModelVersion = task.ModelVersion, 
						Value = "nunit"
					});
				return ur.Validation.Success;
			};
			ReusableConstraint granted = Is.True;
			ReusableConstraint denied = Is.False;

			Assert.That(action(admin), denied);
			Assert.That(action(projAdmin), granted);
			Assert.That(action(projManager), granted);
			Assert.That(action(projExecutor), granted);
			Assert.That(action(notMember), denied);
		}

		[Test]
		public void OnlyMembersCanAddAgreemer()
		{
			var agreemerId = M.MemberFromUser(TestProject, projManager).Id;
			Func<UserModel, Agreement> action = u =>
			{
				Login(u.Email);
				var task = M.Task(1, executor: projExecutor);
				return controller.AddAgreemer(TestProject, task.Id, agreemerId);
			};
			ReusableConstraint granted = Is.Not.Null;
			ReusableConstraint denied = Throws.Exception.TypeOf<NoSuchProjectMemberException>();

			Assert.That(() => action(admin), denied);
			Assert.That(action(projAdmin), granted);
			Assert.That(action(projManager), granted);
			Assert.That(action(projExecutor), granted);
			Assert.That(() => action(notMember), denied);
		}

		[Test]
		public void OnlyAdminOrManagerAgreemerCanRemoveAgreement()
		{
			Func<UserModel, UserModel, bool> action = (u, a) =>
			{
				var task = M.Task(1, executor: projExecutor);
				var agreement = M.Agreement(task, a, u);

				Login(u.Email);

				return controller.RemoveAgreement(task.ProjectCode, task.Id, agreement.Id);
			};
			ReusableConstraint granted = Is.True;
			ReusableConstraint restricted = Throws.Exception.TypeOf<DeleteDeniedException>();
			ReusableConstraint denied = Throws.Exception.TypeOf<NoSuchProjectMemberException>();

			Assert.That(() => action(admin, projManager), denied);
			Assert.That(action(projAdmin, projAdmin), granted);
			Assert.That(action(projAdmin, projManager), granted);
			Assert.That(() => action(projManager, projAdmin), restricted);
			Assert.That(action(projManager, projManager), granted);
			Assert.That(() => action(projExecutor, projManager), restricted);
			Assert.That(() => action(projExecutor, projExecutor), restricted);
			Assert.That(() => action(notMember, projAdmin), denied);
		}

		[Test]
		public void OnlyAdminAgreemerOrManagerAgreemerCanAgree()
		{
			Func<UserModel, UserModel, bool> action = (u, a) =>
			{
				var task = M.Task(1, executor: projExecutor);
				M.Agreement(task, a, u);

				Login(u.Email);

				return controller.AgreeTask(task.ProjectCode, task.Id, "nunit").Done;
			};
			ReusableConstraint granted = Is.True;
			ReusableConstraint incorrect = Throws.Exception.TypeOf<CurrentUserIsNotAgreemerInTaskException>();
			ReusableConstraint restricted = Throws.Exception.TypeOf<ChangeDeniedException>();
			ReusableConstraint denied = Throws.Exception.TypeOf<NoSuchProjectMemberException>();

			Assert.That(() => action(admin, projManager), denied);
			Assert.That(action(projAdmin, projAdmin), granted);
			Assert.That(() => action(projAdmin, projManager), incorrect);
			Assert.That(() => action(projManager, projAdmin), incorrect);
			Assert.That(action(projManager, projManager), granted);
			Assert.That(() => action(projExecutor, projManager), incorrect);
			Assert.That(() => action(projExecutor, projExecutor), restricted);
			Assert.That(() => action(notMember, projAdmin), denied);
		}

		[Test]
		public void OnlyAdminAgreemerOrManagerAgreemerCanRevoke()
		{
			Func<UserModel, UserModel, bool> action = (u, a) =>
			{
				var task = M.Task(1, executor: projExecutor);
				M.Agreement(task, a, u, true);

				Login(u.Email);

				return controller.RevokeAgreement(task.ProjectCode, task.Id).Done;
			};
			ReusableConstraint granted = Is.False;
			ReusableConstraint incorrect = Throws.Exception.TypeOf<CurrentUserIsNotAgreemerInTaskException>();
			ReusableConstraint restricted = Throws.Exception.TypeOf<ChangeDeniedException>();
			ReusableConstraint denied = Throws.Exception.TypeOf<NoSuchProjectMemberException>();

			Assert.That(() => action(admin, projManager), denied);
			Assert.That(action(projAdmin, projAdmin), granted);
			Assert.That(() => action(projAdmin, projManager), incorrect);
			Assert.That(() => action(projManager, projAdmin), incorrect);
			Assert.That(action(projManager, projManager), granted);
			Assert.That(() => action(projExecutor, projManager), incorrect);
			Assert.That(() => action(projExecutor, projExecutor), restricted);
			Assert.That(() => action(notMember, projAdmin), denied);
		}

		[Test]
		public void OnlyAdminOrManagerCanDeleteTask()
		{
			Func<UserModel, bool> action = u =>
			{
				var task = M.Task(1, executor: projExecutor);
				Login(u.Email);
				return controller.DeleteTask(TestProject, task.Id);
			};
			ReusableConstraint granted = Is.True;
			ReusableConstraint restricted = Throws.Exception.TypeOf<DeleteDeniedException>();
			ReusableConstraint denied = Throws.Exception.TypeOf<NoSuchProjectMemberException>();

			Assert.That(() => action(admin), denied);
			Assert.That(action(projAdmin), granted);
			Assert.That(action(projManager), granted);
			Assert.That(() => action(projExecutor), restricted);
			Assert.That(() => action(notMember), denied);
		}

		[Test]
		public void OnlyMembersCanLookupParamTypes()
		{
			M.ParamType("p1");
			M.ParamType("p2");

			Func<UserModel, CustomParameterTypeDTO[]> action = u =>
			{
				Login(u.Email);
				return controller.LookupParamTypes(TestProject, null, 0).ToArray();
			};
			ReusableConstraint granted = Has.Length.EqualTo(2)
				.And.Exactly(1).Matches<CustomParameterTypeDTO>(e => e.Text == "p1")
				.And.Exactly(1).Matches<CustomParameterTypeDTO>(e => e.Text == "p2");
			ReusableConstraint denied = Is.Empty;

			Assert.That(action(admin), denied);
			Assert.That(action(projAdmin), granted);
			Assert.That(action(projManager), granted);
			Assert.That(action(projExecutor), granted);
			Assert.That(action(notMember), denied);
		}

		[Test]
		public void OnlyMembersCanManageParams()
		{
			var task = M.Task(1, executor: projExecutor);
			var pt = M.ParamType();
			Func<UserModel, bool> action = u =>
			{
				Login(u.Email);
				var ur1 = controller.EditParam(TestProject, task.Id,
					new CustomParameterDTO
					{
						Type = new CustomParameterTypeDTO {Id = pt.Id},
						Value = "nunit"
					});
				Session.Flush();
				var dto = ur1.Model;
				dto.Value = "nunit changed";
				var ur2 = controller.EditParam(TestProject, task.Id, dto);
				Session.Flush();
				var r3 = controller.DeleteParam(TestProject, ur2.Model.Id);
				Session.Flush();
				return ur1.Validation.Success && ur2.Validation.Success && r3;
			};
			ReusableConstraint granted = Is.True;
			ReusableConstraint denied = Throws.Exception.TypeOf<NoSuchProjectMemberException>();

			Assert.That(() => action(admin), denied);
			Assert.That(action(projAdmin), granted);
			Assert.That(action(projManager), granted);
			Assert.That(action(projExecutor), granted);
			Assert.That(() => action(notMember), denied);
		}

		[Test]
		public void OnlyMembersCanTagTask()
		{
			var adminTag = M.Tag("adminTag", owner: projAdmin);
			var mgrTag = M.Tag("mgrTag", owner: projManager);
			var execTag = M.Tag("execTag", owner: projExecutor);
			Func<UserModel, TaskTagModel, bool> action = (u, t) =>
			{
				var task = M.Task(1, executor: projExecutor);
				
				Login(u.Email);
				var result = controller.TagTask(TestProject, task.Id, t.Id);
				Session.Flush();
				return result;
			};
			
			ReusableConstraint granted = Is.True;
			ReusableConstraint restricted = Throws.Exception.TypeOf<CreationDeniedException>();
			ReusableConstraint denied = Throws.Exception.TypeOf<NoSuchProjectMemberException>();

			Assert.That(() => action(admin, mgrTag), denied);

			Assert.That(action(projAdmin, adminTag), granted);
			Assert.That(() => action(projAdmin, mgrTag), restricted);

			Assert.That(action(projManager, mgrTag), granted);
			Assert.That(() => action(projManager, adminTag), restricted);

			Assert.That(action(projExecutor, execTag), granted);
			Assert.That(() => action(projExecutor, adminTag), restricted);
			
			Assert.That(() => action(notMember, adminTag), denied);
		}

		[Test]
		public void OnlyMembersCanDetagTask()
		{

			var adminTag = M.Tag("adminTag", owner: projAdmin);
			var mgrTag = M.Tag("mgrTag", owner: projManager);
			var execTag = M.Tag("execTag", owner: projExecutor);
			Func<UserModel, TaskTagModel, bool> action = (u, t) =>
			{
				var task = M.Task(1, executor: projExecutor);
				var link = new TaskToTagModel
				{
					Task = task,
					Tag = t
				};
				task.Tags.Add(link);
				Session.Save(link);
				Session.Flush();
				Session.Clear();//needs for cascade operation

				Login(u.Email);
				var result = controller.DetagTask(TestProject, task.Id, t.Id);
				Session.Flush();
				return result;
			};

			ReusableConstraint granted = Is.True;
			ReusableConstraint restricted = Throws.Exception.TypeOf<DeleteDeniedException>();
			ReusableConstraint denied = Throws.Exception.TypeOf<NoSuchProjectMemberException>();

			Assert.That(() => action(admin, execTag), denied);

			Assert.That(action(projAdmin, adminTag), granted);
			Assert.That(() => action(projAdmin, execTag), restricted);

			Assert.That(action(projManager, mgrTag), granted);
			Assert.That(() => action(projManager, adminTag), restricted);

			Assert.That(action(projExecutor, execTag), granted);
			Assert.That(() => action(projExecutor, mgrTag), restricted);

			Assert.That(() => action(notMember, execTag), denied);
		}

		[Test]
		public void OnlyMembersCanGetFiles()
		{
			var task = M.Task(1, executor: projExecutor);
			var f1 = M.File(task, "f1.doc");
			var f2 = M.File(task, "f2.doc");

			Func<UserModel, FileDTO[]> action = u =>
			{
				Login(u.Email);
				return controller.GetFiles(TestProject, task.SeqNumber,
					Enumerable.Empty<IModelFilterNode>().ToList(),
					Enumerable.Empty<SortInfo>().ToList(),
					0).ToArray();
			};
			ReusableConstraint granted = Has.Length.EqualTo(2)
				.And.Exactly(1).Matches<FileDTO>(e => e.Name == f1.Name)
				.And.Exactly(1).Matches<FileDTO>(e => e.Name == f2.Name);
			ReusableConstraint denied = Throws.Exception.TypeOf<NoSuchProjectMemberException>();

			Assert.That(() => action(admin), denied);
			Assert.That(action(projAdmin), granted);
			Assert.That(action(projManager), granted);
			Assert.That(action(projExecutor), granted);
			Assert.That(() => action(notMember), denied);
		}

		[Test]
		public void OnlyMembersCanGetFilesCount()
		{
			var task = M.Task(1, executor: projExecutor);
			M.File(task, "f1.doc");
			M.File(task, "f2.doc");

			Func<UserModel, int> action = u =>
			{
				Login(u.Email);
				return controller.GetFilesCount(TestProject, task.SeqNumber,
					Enumerable.Empty<IModelFilterNode>().ToList());
			};
			ReusableConstraint granted = Is.EqualTo(2);
			ReusableConstraint denied = Throws.Exception.TypeOf<NoSuchProjectMemberException>();

			Assert.That(() => action(admin), denied);
			Assert.That(action(projAdmin), granted);
			Assert.That(action(projManager), granted);
			Assert.That(action(projExecutor), granted);
			Assert.That(() => action(notMember), denied);
		}

		[Test]
		public void OnlyMembersCanUploadFiles()
		{
			var task = M.Task(1, executor: projExecutor);
			var data = new byte[] { 0x01, 0x02, 0x03 };
			var reqMock = Substitute.For<HttpRequestBase>();
			reqMock.Form.Returns(new NameValueCollection
			{
				{"project", TestProject},
				{"ownerId", task.SeqNumber},
				{"uploadId", Guid.NewGuid().ToString()}
			});
			reqMock.Headers.Returns(new NameValueCollection());
			var fileMock = Substitute.For<HttpPostedFileBase>();
			fileMock.ContentType.Returns("application/pdf");
			fileMock.InputStream.Returns(new MemoryStream(data));
			var filesMock = Substitute.For<HttpFileCollectionBase>();
			filesMock.Count.Returns(1);
			filesMock[0].Returns(fileMock);
			var counter = 0;
			Func<UserModel, FileDTO[]> action = u =>
			{
				counter++;
				fileMock.FileName.Returns(counter + "test.pdf");
				Login(u.Email);
				return controller.UploadFiles(reqMock, filesMock)
					.Files
					.Select(dto => dto.Model)
					.ToArray();
			};
			ReusableConstraint granted = Has.Length.EqualTo(1)
				.And.Exactly(1).Matches<FileDTO>(f => f.Name.EndsWith("test.pdf") && f.Uploaded);
			ReusableConstraint denied = Throws.Exception.TypeOf<NoSuchProjectMemberException>();

			Assert.That(() => action(admin), denied);
			Assert.That(action(projAdmin), granted);
			Assert.That(action(projManager), granted);
			Assert.That(action(projExecutor), granted);
			Assert.That(() => action(notMember), denied);
		}

		[Test]
		public void OnlyMembersCanDeleteFile()
		{
			var task = M.Task(1, executor: projExecutor);

			Action<UserModel> action = u =>
			{
				var file = M.File(task);
				Session.Clear();

				Login(u.Email);
				controller.DeleteFile(TestProject, file.Id);
				Session.Flush();
			};
			ReusableConstraint granted = Throws.Nothing;
			ReusableConstraint denied = Throws.Exception.TypeOf<NoSuchProjectMemberException>();

			Assert.That(() => action(admin), denied);
			Assert.That(() => action(projAdmin), granted);
			Assert.That(() => action(projManager), granted);
			Assert.That(() => action(projExecutor), granted);
			Assert.That(() => action(notMember), denied);
		}

		[Test]
		public void OnlyMembersCanDeleteFiles()
		{
			var task = M.Task(1, executor: projExecutor);

			Action<UserModel> action = u =>
			{
				var f1 = M.File(task, "f1");
				var f2 = M.File(task, "f2");
				Session.Clear();

				Login(u.Email);
				controller.DeleteFiles(TestProject, new [] {f1.Id, f2.Id});
				Session.Flush();
			};
			ReusableConstraint granted = Throws.Nothing;
			ReusableConstraint denied = Throws.Exception.TypeOf<NoSuchProjectMemberException>();

			Assert.That(() => action(admin), denied);
			Assert.That(() => action(projAdmin), granted);
			Assert.That(() => action(projManager), granted);
			Assert.That(() => action(projExecutor), granted);
			Assert.That(() => action(notMember), denied);
		}

		[Test]
		public void OnlyExecutorsCanTrackTime()
		{
			var num = 0;
			Func<UserModel, UserModel, TimelogDTO> action = (u, e) =>
			{
				num++;
				var task = M.Task(num, executor: e);
				Login(u.Email);
				return controller.TrackTime(TestProject, task.Id, 2, null);
			};

			ReusableConstraint granted = Is.Not.Null;
			ReusableConstraint notExecutorRestriction = Throws.Exception.TypeOf<CurrentUserIsNotTaskExecutorException>();
			ReusableConstraint noAccessRestriction = Throws.Exception.TypeOf<NoSuchEntityException>();
			ReusableConstraint denied = Throws.Exception.TypeOf<NoSuchProjectMemberException>();
			
			Assert.That(() => action(admin, projExecutor), denied);

			Assert.That(action(projAdmin, projAdmin), granted);
			Assert.That(() => action(projAdmin, projManager), notExecutorRestriction);
			Assert.That(() => action(projAdmin, projExecutor), notExecutorRestriction);

			Assert.That(action(projManager, projManager), granted);
			Assert.That(() => action(projManager, projAdmin), notExecutorRestriction);
			Assert.That(() => action(projManager, projExecutor), notExecutorRestriction);

			Assert.That(action(projExecutor, projExecutor), granted);
			Assert.That(() => action(projExecutor, projAdmin), noAccessRestriction);
			Assert.That(() => action(projExecutor, projManager), noAccessRestriction);

			Assert.That(() => action(notMember, projExecutor), denied);
		}

		[Test]
		public void OnlyMyselfOrProjAdminCanChangeTime()
		{
			var num = 0;
			Func<UserModel, UserModel, TimelogDTO> action = (u, e) =>
			{
				num++;
				var task = M.Task(num, executor: e);
				var time = M.Time(task, e);
				Login(u.Email);
				return controller.UpdateTime(TestProject, time.Id, 2, null);
			};

			ReusableConstraint granted = Is.Not.Null;
			ReusableConstraint restricted = Throws.Exception.TypeOf<ChangeDeniedException>();
			ReusableConstraint denied = Throws.Exception.TypeOf<NoSuchProjectMemberException>();

			Assert.That(() => action(admin, projExecutor), denied);

			Assert.That(action(projAdmin, projAdmin), granted);
			Assert.That(action(projAdmin, projManager), granted);
			Assert.That(action(projAdmin, projExecutor), granted);

			Assert.That(action(projManager, projManager), granted);
			Assert.That(() => action(projManager, projAdmin), restricted);
			Assert.That(() => action(projManager, projExecutor), restricted);

			Assert.That(action(projExecutor, projExecutor), granted);
			Assert.That(() => action(projExecutor, projAdmin), restricted);
			Assert.That(() => action(projExecutor, projManager), restricted);

			Assert.That(() => action(notMember, projExecutor), denied);
		}

		[Test]
		public void OnlyMyselfOrProjAdminCanDeleteTime()
		{
			var num = 0;
			Action<UserModel, UserModel> action = (u, e) =>
			{
				num++;
				var task = M.Task(num, executor: e);
				var time = M.Time(task, e);
				Session.Clear();
				Login(u.Email);
				controller.DeleteTime(TestProject, time.Id);
				Session.Flush();
			};

			ReusableConstraint granted = Throws.Nothing;
			ReusableConstraint restricted = Throws.Exception.TypeOf<DeleteDeniedException>();
			ReusableConstraint noAccess = Throws.Exception.TypeOf<ChangeDeniedException>();
			ReusableConstraint denied = Throws.Exception.TypeOf<NoSuchProjectMemberException>();

			Assert.That(() => action(admin, projExecutor), denied);

			Assert.That(() => action(projAdmin, projAdmin), granted);
			Assert.That(() => action(projAdmin, projManager), granted);
			Assert.That(() => action(projAdmin, projExecutor), granted);

			Assert.That(() => action(projManager, projManager), granted);
			Assert.That(() => action(projManager, projAdmin), restricted);
			Assert.That(() => action(projManager, projExecutor), restricted);

			Assert.That(() => action(projExecutor, projExecutor), granted);
			Assert.That(() => action(projExecutor, projAdmin), noAccess);
			Assert.That(() => action(projExecutor, projManager), noAccess);

			Assert.That(() => action(notMember, projExecutor), denied);
		}

        [Test]
	    public void OnlyMembersCanGetComments()
	    {
            var task = M.Task(1, executor: projExecutor);
            M.Comment(task);

            Func<UserModel, CommentDTO[]> action = u =>
            {
                Login(u.Email);
                return controller.GetComments(task.ProjectCode, task.SeqNumber, 0).ToArray();
            };

            ReusableConstraint denied = Throws.Exception.TypeOf<NoSuchProjectMemberException>();
            ReusableConstraint granted = Has.Length.EqualTo(1);

            Assert.That(() => action(admin), denied);
            Assert.That(action(projAdmin), granted);
            Assert.That(action(projManager), granted);
            Assert.That(action(projExecutor), granted);
            Assert.That(() => action(notMember), denied);
	    }

        [Test]
        public void OnlyMembersCanGetCommentsCount()
        {
            var task = M.Task(1, executor: projExecutor);
            M.Comment(task);

            Func<UserModel, int> action = u =>
            {
                Login(u.Email);
                return controller.GetCommentsCount(task.ProjectCode, task.SeqNumber);
            };

            ReusableConstraint denied = Throws.Exception.TypeOf<NoSuchProjectMemberException>();
            ReusableConstraint granted = Is.EqualTo(1);

            Assert.That(() => action(admin), denied);
            Assert.That(action(projAdmin), granted);
            Assert.That(action(projManager), granted);
            Assert.That(action(projExecutor), granted);
            Assert.That(() => action(notMember), denied);
        }

        [Test]
	    public void OnlyMembersCanAddCommentToTask()
	    {
	        var task = M.Task(1, executor: projExecutor);

	        Func<UserModel, CommentDTO> action = u =>
	        {
	            Login(u.Email);
	            var dto = controller.CreateComment(task.ProjectCode, task.SeqNumber, "comment");
	            Session.Flush();
	            return dto;
	        };

	        ReusableConstraint denied = Throws.Exception.TypeOf<NoSuchProjectMemberException>();
	        ReusableConstraint granted = Is.Not.Null;

            Assert.That(()=> action(admin), denied);
            Assert.That(action(projAdmin), granted);
            Assert.That(action(projManager), granted);
            Assert.That(action(projExecutor), granted);
            Assert.That(() => action(notMember), denied);
	    }
	}
}
