using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using AGO.Core;
using AGO.Core.Config;
using AGO.Core.Filters;
using AGO.Tasks.Controllers;
using AGO.Tasks.Model.Task;
using NHibernate;
using NSubstitute;
using NUnit.Framework;

namespace AGO.Tasks.Test
{
	/// <summary>
	/// Test for task files crud
	/// </summary>
	public class TaskFilesTest: AbstractTest
	{
		private string testFileStoreRoot;
		private TasksController controller;
		private TaskModel task;
		

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

		public override void SetUp()
		{
			base.SetUp();

			Directory.CreateDirectory(testFileStoreRoot);

			Thread.CurrentThread.CurrentUICulture = new CultureInfo("en");
			task = M.Task(1);
		}

		public override void TearDown()
		{
			Directory.Delete(testFileStoreRoot, true);

			base.TearDown();
		}

		[Test]
		public void CreateFileReturnSuccess()
		{
			var data = new byte[] {0x01, 0x02, 0x03};
			var reqMock = Substitute.For<HttpRequestBase>();
			reqMock.Form.Returns(new NameValueCollection
			{
				{"project", TestProject},
				{"ownerId", task.SeqNumber},
				{"uploadId", Guid.NewGuid().ToString()}
			});
			reqMock.Headers.Returns(new NameValueCollection());
			var fileMock = Substitute.For<HttpPostedFileBase>();
			fileMock.FileName.Returns("test.pdf");
			fileMock.ContentType.Returns("application/pdf");
			fileMock.InputStream.Returns(new MemoryStream(data));
			var filesMock = Substitute.For<HttpFileCollectionBase>();
			filesMock.Count.Returns(1);
			filesMock[0].Returns(fileMock);

			var ur = controller.UploadFiles(reqMock, filesMock);
			Session.Flush();//as in apiexecutor

			Assert.That(ur, Is.Not.Null);
			Assert.That(ur.Files, Is.Not.Null);
			Assert.That(ur.Files.Count(), Is.EqualTo(1));
			var id = ur.Files.First().Model.Id;
			var file = Session.Get<TaskFileModel>(id);
			Assert.That(file, Is.Not.Null);
			Assert.That(file.Name, Is.EqualTo("test.pdf"));
			var storedFileName = Path.Combine(testFileStoreRoot, file.Path);
			Assert.That(File.Exists(storedFileName), Is.True);
			Assert.That(File.ReadAllBytes(storedFileName), Is.EqualTo(data));
		}

		[Test]
		public void CreateDuplicateFileThrow()
		{
			var alreadyExists = M.File(task, "test.pdf", mime: "application/pdf");

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
			fileMock.FileName.Returns(alreadyExists.Name);
			fileMock.ContentType.Returns(alreadyExists.ContentType);
			fileMock.InputStream.Returns(new MemoryStream(data));
			var filesMock = Substitute.For<HttpFileCollectionBase>();
			filesMock.Count.Returns(1);
			filesMock[0].Returns(fileMock);

			Assert.That(() => controller.UploadFiles(reqMock, filesMock),
				Throws.Exception.TypeOf<MustBeUniqueException>());
		}

		[Test]
		public void UpdateFileReturnSuccess()
		{
			var f = M.File(task, "test.txt");

			var data = new byte[] { 0x01, 0x02, 0x03 };
			var reqMock = Substitute.For<HttpRequestBase>();
			reqMock.Form.Returns(new NameValueCollection
			{
				{"project", TestProject},
				{"ownerId", task.SeqNumber},
				{"uploadId", Guid.NewGuid().ToString()},
				{"fileId", f.Id.ToString()}
			});
			reqMock.Headers.Returns(new NameValueCollection());
			var fileMock = Substitute.For<HttpPostedFileBase>();
			fileMock.FileName.Returns("test.pdf");
			fileMock.InputStream.Returns(new MemoryStream(data));
			var filesMock = Substitute.For<HttpFileCollectionBase>();
			filesMock.Count.Returns(1);
			filesMock[0].Returns(fileMock);

			var ur = controller.UploadFiles(reqMock, filesMock);
			Session.Flush();//as in apiexecutor
			Session.Clear();

			Assert.That(ur, Is.Not.Null);
			Assert.That(ur.Files, Is.Not.Null);
			Assert.That(ur.Files.Count(), Is.EqualTo(1));
			f = Session.Get<TaskFileModel>(f.Id);
			Assert.That(f.Name, Is.EqualTo("test.pdf"));
			var storedFileName = Path.Combine(testFileStoreRoot, f.Path);
			Assert.That(File.Exists(storedFileName), Is.True);
			Assert.That(File.ReadAllBytes(storedFileName), Is.EqualTo(data));
		}

		[Test]
		public void UpdateDuplicateFileThrow()
		{
			var f1 = M.File(task, "1.txt");
			var f2 = M.File(task, "2.txt");

			var data = new byte[] { 0x01, 0x02, 0x03 };
			var reqMock = Substitute.For<HttpRequestBase>();
			reqMock.Form.Returns(new NameValueCollection
			{
				{"project", TestProject},
				{"ownerId", task.SeqNumber},
				{"uploadId", Guid.NewGuid().ToString()},
				{"fileId", f1.Id.ToString()}
			});
			reqMock.Headers.Returns(new NameValueCollection());
			var fileMock = Substitute.For<HttpPostedFileBase>();
			fileMock.FileName.Returns(f2.Name);
			fileMock.InputStream.Returns(new MemoryStream(data));
			var filesMock = Substitute.For<HttpFileCollectionBase>();
			filesMock.Count.Returns(1);
			filesMock[0].Returns(fileMock);

			Assert.That(() => controller.UploadFiles(reqMock, filesMock),
				Throws.Exception.TypeOf<MustBeUniqueException>());
		}

		[Test]
		public void ReadFilesReturnData()
		{
			var f1 = M.File(task, "1.txt");
			var f2 = M.File(task, "2.txt");

			var files = controller.GetFiles(TestProject,
				task.SeqNumber,
				Enumerable.Empty<IModelFilterNode>().ToList(),
				new[] {new SortInfo {Property = "Name"}},
				0).ToArray();

			Assert.That(files, Is.Not.Null);
			Assert.That(files, Has.Length.EqualTo(2));
			Assert.That(files[0].Id, Is.EqualTo(f1.Id));
			Assert.That(files[0].Name, Is.EqualTo(f1.Name));
			Assert.That(files[1].Id, Is.EqualTo(f2.Id));
			Assert.That(files[1].Name, Is.EqualTo(f2.Name));
		}

		[Test]
		public void GetFilesCountReturnCount()
		{
			M.File(task, "1.txt");
			M.File(task, "2.txt");

			var count = controller.GetFilesCount(TestProject,
				task.SeqNumber,
				Enumerable.Empty<IModelFilterNode>().ToList());

			Assert.That(count, Is.EqualTo(2));
		}

		[Test]
		public void DeleteFileSuccess()
		{
			var f1 = M.File(task, "1.txt");
			Session.Clear();

			controller.DeleteFile(TestProject, f1.Id);
			Session.Flush();

			f1 = Session.Get<TaskFileModel>(f1.Id);
			Assert.That(f1, Is.Null);
		}

		[Test]
		public void DeleteFileWithInvalidProjectThrow()
		{
			Assert.That(() => controller.DeleteFile("bla bla proj", Guid.Empty),
				Throws.Exception.TypeOf<NoSuchProjectException>());
		}

		[Test]
		public void DeleteFileWithInvalidFileIdThrow()
		{
			Assert.That(() => controller.DeleteFile(TestProject, Guid.Empty),
				Throws.Exception.TypeOf<ObjectNotFoundException>());
		}

		[Test]
		public void DeleteSomeFilesSuccess()
		{
			var f1 = M.File(task, "1.txt");
			var f2 = M.File(task, "2.txt");
			var f3 = M.File(task, "3.txt");
			Session.Clear();

			controller.DeleteFiles(TestProject, new [] {f1.Id, f3.Id});
			Session.Flush();

			f1 = Session.Get<TaskFileModel>(f1.Id);
			f2 = Session.Get<TaskFileModel>(f2.Id);
			f3 = Session.Get<TaskFileModel>(f3.Id);
			Assert.That(f1, Is.Null);
			Assert.That(f2, Is.Not.Null);
			Assert.That(f3, Is.Null);
		}

		[Test]
		public void DeleteEmptyFilesSuccess()
		{
			Assert.That(() => controller.DeleteFiles(TestProject, new Guid[0]),
				Throws.Nothing);
		}

		[Test]
		public void DeleteSomeFilesWithInvalidFileIdThrow()
		{
			var f1 = M.File(task, "1.txt");
			var f2 = M.File(task, "2.txt");
			var f3 = M.File(task, "3.txt");
			Session.Clear();

// ReSharper disable once AccessToModifiedClosure
			Assert.That(() => controller.DeleteFiles(TestProject, new[] {f1.Id, Guid.NewGuid()}),
				Throws.Exception.TypeOf<ObjectNotFoundException>());
			Session.Clear();

			f1 = Session.Get<TaskFileModel>(f1.Id);
			f2 = Session.Get<TaskFileModel>(f2.Id);
			f3 = Session.Get<TaskFileModel>(f3.Id);
			Assert.That(f1, Is.Not.Null);
			Assert.That(f2, Is.Not.Null);
			Assert.That(f3, Is.Not.Null);
		}
	}
}