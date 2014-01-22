using System;
using System.Linq;
using AGO.Core;
using AGO.Core.Filters;
using AGO.Tasks.Controllers.DTO;
using AGO.Tasks.Model.Dictionary;
using AGO.Tasks.Model.Task;
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
			vr = Controller.CreateTag(TestProject, param).Validation;
			_SessionProvider.FlushCurrentSession(!vr.Success);

			var grp = _SessionProvider.CurrentSession.QueryOver<TaskTagModel>()
				.Where(m => m.ProjectCode == TestProject && m.FullName == "grp")
				.SingleOrDefault();
			Assert.IsNotNull(grp);
			Assert.AreNotEqual(default(Guid), grp.Id);
			Assert.AreEqual("grp", grp.FullName);
			Assert.IsTrue(vr.Success);

			param.Name = "parent\\child\\subchild";
			vr = Controller.CreateTag(TestProject, param).Validation;
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
			vr = Controller.CreateTag(TestProject, param).Validation;
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
			M.Tag("t1");
			_SessionProvider.FlushCurrentSession();

			var param = new TaskTagDTO { Name = "t1" };
			var vr = Controller.CreateTag(TestProject, param);
			_SessionProvider.FlushCurrentSession(!vr.Validation.Success);

			Assert.IsFalse(vr.Validation.Success);
			Assert.IsTrue(vr.Validation.FieldErrors.Any(fe => fe.Key == "FullName"));
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
				.Where(m => m.ProjectCode == TestProject && m.Creator == CurrentUser && m.FullName == "aaa")
				.SingleOrDefault();
			var bbb = Session.QueryOver<TaskTagModel>()
				.Where(m => m.ProjectCode == TestProject && m.Creator == CurrentUser && m.FullName == "aaa\\bbb")
				.SingleOrDefault();
			
			Assert.AreEqual(bbb.Id, sub.Parent.Id);
			Assert.AreEqual(aaa.Id, bbb.Parent.Id);
		}

		[Test]
		public void EditTagWithRestrictedReturn()
		{
			var parent = M.Tag("parent");
			var child = M.Tag("child", parent);
			var sub1 = M.Tag("sub1", child);
			M.Tag("sub2", child);
			_SessionProvider.FlushCurrentSession();

			var param = new TaskTagDTO { Id = child.Id, ModelVersion = child.ModelVersion, Name = "new parent\\child2" };
			var res = Controller.EditTag(TestProject, param, new [] { sub1.Id });
			_SessionProvider.FlushCurrentSession(!res.Validation.Success);

			Assert.IsTrue(res.Validation.Success);
			Assert.AreEqual(3, res.Model.Length);
			Assert.AreEqual("new parent\\child2", res.Model[0].Name);
			Assert.AreEqual("new parent", res.Model[1].Name);
			Assert.AreEqual("new parent\\child2\\sub1", res.Model[2].Name);
		}

		[Test]
		public void DeleteTag()
		{
			var t1 = M.Tag("t1");
			_SessionProvider.FlushCurrentSession();

			var res = Controller.DeleteTags(TestProject, new[] {t1.Id}).ToArray();
			_SessionProvider.FlushCurrentSession();

			Assert.IsNotNull(res);
			Assert.AreEqual(1, res.Length);
			Assert.AreEqual(t1.Id, res[0]);
			t1 = Session.Get<TaskTagModel>(t1.Id);
			Assert.IsNull(t1);
		}

		[Test]
		public void DeleteTagWithChilds()
		{
			var parent = M.Tag("parent");
			var child = M.Tag("child", parent);
			var sub = M.Tag("sub", child);
			_SessionProvider.FlushCurrentSession();

			var res = Controller.DeleteTags(TestProject, new[] {parent.Id}, null, new [] { child.Id, sub.Id }).ToArray();
			_SessionProvider.FlushCurrentSession();

			Assert.AreEqual(3, res.Length);
			Assert.IsTrue(res.Contains(parent.Id));
			Assert.IsTrue(res.Contains(child.Id));
			Assert.IsTrue(res.Contains(sub.Id));
			Assert.IsNull(Session.Get<TaskTagModel>(parent.Id));
			Assert.IsNull(Session.Get<TaskTagModel>(child.Id));
			Assert.IsNull(Session.Get<TaskTagModel>(sub.Id));
		}

		[Test]
		public void DeleteParentAndChildTagInOneTime()
		{
			var parent = M.Tag("parent");
			var child = M.Tag("child", parent);
			var sub = M.Tag("sub", child);
			_SessionProvider.FlushCurrentSession();

			var res = Controller.DeleteTags(TestProject, new[] { child.Id, parent.Id }, null, new [] { sub.Id }).ToArray();
			_SessionProvider.FlushCurrentSession();

			Assert.AreEqual(3, res.Length);
			Assert.IsTrue(res.Contains(parent.Id));
			Assert.IsTrue(res.Contains(child.Id));
			Assert.IsTrue(res.Contains(sub.Id));
			Assert.IsNull(Session.Get<TaskTagModel>(parent.Id));
			Assert.IsNull(Session.Get<TaskTagModel>(child.Id));
			Assert.IsNull(Session.Get<TaskTagModel>(sub.Id));
		}

		[Test, ExpectedException(typeof(CanNotReplaceWithItemThatWillBeDeletedTo))]
		public void CanNotDeleteIfReplacementInChilds()
		{
			var parent = M.Tag("parent");
			var child = M.Tag("child", parent);
			var sub = M.Tag("sub", child);
			_SessionProvider.FlushCurrentSession();

			Controller.DeleteTags(TestProject, new[] { parent.Id }, sub.Id);
		}

		[Test, ExpectedException(typeof(CannotDeleteReferencedItemException))]
		public void CanNotDeleteIfReferencedTag()
		{
			var task = M.Task(1);
			var tag = M.Tag("t1");
			var link = new TaskToTagModel
			           	{
			           		Creator = CurrentUser,
			           		Task = task,
			           		Tag = tag
			           	};
			Session.Save(link);
			_SessionProvider.FlushCurrentSession();

			Controller.DeleteTags(TestProject, new[] { tag.Id });
		}

		[Test]
		public void DeleteTagWithReplacement()
		{
			var task1 = M.Task(1);
			var task2 = M.Task(2);
			var tag1 = M.Tag("t1");
			var tag2 = M.Tag("t2");
			var link1 = new TaskToTagModel
			{
				Creator = CurrentUser,
				Task = task1,
				Tag = tag1
			};
			var link2 = new TaskToTagModel
			{
				Creator = CurrentUser,
				Task = task2,
				Tag = tag1
			};
			Session.Save(link1);
			Session.Save(link2);
			_SessionProvider.FlushCurrentSession();

			var res = Controller.DeleteTags(TestProject, new[] { tag1.Id }, tag2.Id).ToArray();
			_SessionProvider.FlushCurrentSession();

			Assert.AreEqual(1, res.Length);
			Assert.AreEqual(tag1.Id, res[0]);
			Session.Refresh(task1);
			Session.Refresh(task2);
			Assert.AreEqual(tag2.Id, task1.Tags.First().Tag.Id);
			Assert.AreEqual(tag2.Id, task2.Tags.First().Tag.Id);
			Assert.IsNull(Session.Get<TaskTagModel>(tag1.Id));
		}

		[Test]
		public void DeleteTagWithRestrictedReturn()
		{
			var parent = M.Tag("parent");
			var child = M.Tag("child", parent);
			var sub1 = M.Tag("sub1", child);
			M.Tag("sub2", child);
			_SessionProvider.FlushCurrentSession();

			var res = Controller.DeleteTags(TestProject, new[] { child.Id }, null, new [] { sub1.Id }).ToArray();
			_SessionProvider.FlushCurrentSession();

			Assert.AreEqual(2, res.Length);
			Assert.IsTrue(res.Contains(child.Id));
			Assert.IsTrue(res.Contains(sub1.Id));
		}
	}
}