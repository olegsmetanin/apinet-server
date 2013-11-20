using System;
using System.Linq;
using AGO.Core.Filters;
using AGO.Tasks.Controllers.DTO;
using AGO.Tasks.Model.Dictionary;
using NUnit.Framework;

namespace AGO.Tasks.Test
{
	/// <summary>
	/// Тесты CRUD справочника тегов задач
	/// </summary>
	public class TaskTagCRUDTest: AbstractDictionaryTest
	{
		[TestFixtureSetUp]
		public new void Init()
		{
			base.Init();
		}

		[TestFixtureTearDown]
		public new void Cleanup()
		{
			base.Cleanup();
		}

		[TearDown]
		public new void TearDown()
		{
			base.TearDown();
		}

		[Test]
		public void LookupTagWithoutTermReturnAll()
		{
			var t1 = M.Tag("t1");
			var t2 = M.Tag("t2");
			var t3 = M.Tag("t3", t2);
			_SessionProvider.FlushCurrentSession();

			var result = Controller.LookupTags(TestProject, null, 0).ToArray();

			Assert.AreEqual(3, result.Length);
			Assert.AreEqual(t1.FullName, result[0].Text);
			Assert.AreEqual(t2.FullName, result[1].Text);
			Assert.AreEqual(t3.FullName, result[2].Text);
		}

		[Test]
		public void LookupTagWithTermReturnMatched()
		{
			M.Tag("abc");
			var t2 = M.Tag("cde");
			var t3 = M.Tag("z", t2);
			_SessionProvider.FlushCurrentSession();

			var result = Controller.LookupTags(TestProject, "cd", 0).ToArray();

			Assert.AreEqual(2, result.Length);
			Assert.AreEqual(t2.FullName, result[0].Text);
			Assert.AreEqual(t3.FullName, result[1].Text);
		}

		[Test]
		public void LookupTagWithHierarchyTermReturnMatched()
		{
			var t1 = M.Tag("abc");
			var t2 = M.Tag("dfg", t1);
			var t3 = M.Tag("ehx", t2);
			_SessionProvider.FlushCurrentSession();

			var result = Controller.LookupTags(TestProject, "ab df", 0).ToArray();

			Assert.AreEqual(2, result.Length);
			Assert.AreEqual(t2.FullName, result[0].Text);
			Assert.AreEqual(t3.FullName, result[1].Text);

			result = Controller.LookupTags(TestProject, "dfg,hx", 0).ToArray();

			Assert.AreEqual(1, result.Length);
			Assert.AreEqual(t3.FullName, result[0].Text);

			result = Controller.LookupTags(TestProject, "abc\\ehx", 0).ToArray();

			Assert.AreEqual(1, result.Length);
			Assert.AreEqual(t3.FullName, result[0].Text);
		}

		[Test]
		public void GetTagsFromEmptyReturnEmpty()
		{
			var result = Controller.GetTags(TestProject,
			                                Enumerable.Empty<IModelFilterNode>().ToArray(),
			                                Enumerable.Empty<SortInfo>().ToArray(),
			                                0);
			Assert.IsNotNull(result);
			Assert.IsFalse(result.Any());
		}

		[Test]
		public void ReadTags()
		{
			var t1 = M.Tag("t1");
			var t2 = M.Tag("t2");
			_SessionProvider.FlushCurrentSession();

			var result = Controller.GetTags(TestProject,
			                                Enumerable.Empty<IModelFilterNode>().ToArray(),
			                                new[] {new SortInfo {Property = "FullName"}},
			                                0).ToArray();

			Assert.AreEqual(2, result.Length);
			Assert.AreEqual(t1.FullName, result[0].Name);
			Assert.AreEqual(t2.FullName, result[1].Name);
		}

		[Test]
		public void CreateTag()
		{
			var param = new TaskTagDTO {Name = "abc"};
			var vr = Controller.CreateTag(TestProject, param).Validation;
			_SessionProvider.FlushCurrentSession(!vr.Success);

			var abc = _SessionProvider.CurrentSession.QueryOver<TaskTagModel>()
				.Where(m => m.ProjectCode == TestProject && m.FullName == "abc")
				.SingleOrDefault();
			Assert.IsNotNull(abc);
			Assert.AreNotEqual(default(Guid), abc.Id);
			Assert.AreEqual("abc", abc.FullName);
			Assert.IsTrue(vr.Success);

			param.Name = "grp";
			vr = Controller.CreateTag(TestProject, param, false).Validation;
			_SessionProvider.FlushCurrentSession(!vr.Success);

			var grp = _SessionProvider.CurrentSession.QueryOver<TaskTagModel>()
				.Where(m => m.ProjectCode == TestProject && m.FullName == "grp")
				.SingleOrDefault();
			Assert.IsNotNull(grp);
			Assert.AreNotEqual(default(Guid), grp.Id);
			Assert.AreEqual("grp", grp.FullName);
			Assert.IsTrue(vr.Success);

			param.Name = "parent\\child\\subchild";
			vr = Controller.CreateTag(TestProject, param, false).Validation;
			_SessionProvider.FlushCurrentSession(!vr.Success);

			var parent = _SessionProvider.CurrentSession.QueryOver<TaskTagModel>()
				.Where(m => m.ProjectCode == TestProject && m.FullName == "parent")
				.SingleOrDefault();
			var child = _SessionProvider.CurrentSession.QueryOver<TaskTagModel>()
				.Where(m => m.ProjectCode == TestProject && m.FullName == "parent\\child")
				.SingleOrDefault();
			var subchild = _SessionProvider.CurrentSession.QueryOver<TaskTagModel>()
				.Where(m => m.ProjectCode == TestProject && m.FullName == "parent\\child\\subchild")
				.SingleOrDefault();
			Assert.IsNotNull(parent);
			Assert.AreNotEqual(default(Guid), parent.Id);
			Assert.AreEqual("parent", parent.FullName);
			Assert.IsNotNull(child);
			Assert.AreNotEqual(default(Guid), child.Id);
			Assert.AreEqual(parent.Id, child.Parent.Id);
			Assert.AreEqual("parent\\child", child.FullName);
			Assert.AreEqual("child", child.Name);
			Assert.IsNotNull(subchild);
			Assert.AreNotEqual(default(Guid), subchild.Id);
			Assert.AreEqual(child.Id, subchild.Parent.Id);
			Assert.AreEqual("parent\\child\\subchild", subchild.FullName);
			Assert.AreEqual("subchild", subchild.Name);
			Assert.IsTrue(vr.Success);

			param.Name = "parent\\child2";
			vr = Controller.CreateTag(TestProject, param, false).Validation;
			_SessionProvider.FlushCurrentSession(!vr.Success);

			var child2 = _SessionProvider.CurrentSession.QueryOver<TaskTagModel>()
				.Where(m => m.ProjectCode == TestProject && m.FullName == "parent\\child2")
				.SingleOrDefault();
			Assert.IsNotNull(child2);
			Assert.AreNotEqual(default(Guid), child2.Id);
			Assert.AreEqual(parent.Id, child2.Parent.Id);
			Assert.AreEqual("parent\\child2", child2.FullName);
			Assert.AreEqual("child2", child2.Name);
			Assert.IsTrue(vr.Success);
		}

		[Test]
		public void CreateTagWithoutNameError()
		{
			var param = new TaskTagDTO { Name = " "};

			var vr = Controller.CreateTag(TestProject, param);
			
			Assert.IsFalse(vr.Validation.Success);
			Assert.IsTrue(vr.Validation.FieldErrors.Any(fe => fe.Key == "Name"));
		}

		[Test]
		public void CreateDuplicateTagError()
		{
			var t1 = M.Tag("t1");
			_SessionProvider.FlushCurrentSession();

			var param = new TaskTagDTO { Name = "t1" };
			var vr = Controller.CreateTag(TestProject, param, false);

			Assert.IsFalse(vr.Validation.Success);
			Assert.IsTrue(vr.Validation.FieldErrors.Any(fe => fe.Key == "FullName"));

// ReSharper disable RedundantArgumentDefaultValue
			//group and personal tags not duplicated
			vr = Controller.CreateTag(TestProject, param, true);
// ReSharper restore RedundantArgumentDefaultValue
			
			Assert.IsTrue(vr.Validation.Success);
			Assert.AreEqual(vr.Model[0].Name, "t1");
			Assert.AreNotEqual(t1.Id, vr.Model[0].Id);
		}

		[Test]
		public void EditTag()
		{
			var t1 = M.Tag("t1");
			_SessionProvider.FlushCurrentSession();

			var param = new TaskTagDTO { Id = t1.Id, ModelVersion = t1.ModelVersion, Name = "t22" };
			var res = Controller.EditTag(TestProject, param);
			_SessionProvider.FlushCurrentSession(!res.Validation.Success);

			Assert.IsTrue(res.Validation.Success);
			t1 = Session.Get<TaskTagModel>(t1.Id);
			Assert.AreEqual("t22", t1.FullName);
		}

		[Test]
		public void EditTagWithParentChange()
		{
			var parent = M.Tag("parent");
			var newparent = M.Tag("new parent");
			var sub = M.Tag("sub", parent);
			_SessionProvider.FlushCurrentSession();

			var param = new TaskTagDTO { Id = sub.Id, ModelVersion = sub.ModelVersion, Name = "new parent\\sub2" };
			var res = Controller.EditTag(TestProject, param);
			_SessionProvider.FlushCurrentSession(!res.Validation.Success);

			Assert.IsTrue(res.Validation.Success);
			Assert.AreEqual(1, res.Model.Length);
			Assert.AreEqual("new parent\\sub2", res.Model[0].Name);
			parent = Session.Get<TaskTagModel>(parent.Id);
			newparent = Session.Get<TaskTagModel>(newparent.Id);
			sub = Session.Get<TaskTagModel>(sub.Id);
			Assert.AreEqual(0, parent.Children.Count);
			Assert.AreEqual("new parent\\sub2", sub.FullName);
			Assert.AreEqual(newparent.Id, sub.Parent.Id);
			Assert.AreEqual(1, newparent.Children.Count);
		}

		[Test]
		public void EditTagWithNonExistingParentChange()
		{
			var parent = M.Tag("parent");
			var child = M.Tag("child", parent);
			var sub = M.Tag("sub", child);
			_SessionProvider.FlushCurrentSession();

			var param = new TaskTagDTO { Id = sub.Id, ModelVersion = sub.ModelVersion, Name = "aaa\\bbb\\sub" };
			var res = Controller.EditTag(TestProject, param);
			_SessionProvider.FlushCurrentSession(!res.Validation.Success);

			Assert.IsTrue(res.Validation.Success);
			Assert.AreEqual(3, res.Model.Length);
			Assert.IsTrue(res.Model.Any(m => m.Name == "aaa"));
			Assert.IsTrue(res.Model.Any(m => m.Name == "aaa\\bbb"));
			Assert.IsTrue(res.Model.Any(m => m.Name == "aaa\\bbb\\sub"));
			Assert.AreEqual("aaa\\bbb\\sub", res.Model[0].Name);
			child = Session.Get<TaskTagModel>(child.Id);
			Assert.AreEqual(0, child.Children.Count);
			sub = Session.Get<TaskTagModel>(sub.Id);
			Assert.AreEqual("aaa\\bbb\\sub", sub.FullName);
			var aaa = Session.QueryOver<TaskTagModel>()
				.Where(m => m.ProjectCode == TestProject && m.Owner == null && m.FullName == "aaa")
				.SingleOrDefault();
			var bbb = Session.QueryOver<TaskTagModel>()
				.Where(m => m.ProjectCode == TestProject && m.Owner == null && m.FullName == "aaa\\bbb")
				.SingleOrDefault();
			
			Assert.AreEqual(bbb.Id, sub.Parent.Id);
			Assert.AreEqual(aaa.Id, bbb.Parent.Id);
		}
	}
}