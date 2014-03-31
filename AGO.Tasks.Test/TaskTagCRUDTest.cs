using System;
using System.Collections.Generic;
using System.Linq;
using AGO.Core;
using AGO.Core.Model.Dictionary;
using AGO.Core.Model.Dictionary.Projects;
using AGO.Tasks.Model.Dictionary;
using NUnit.Framework;

namespace AGO.Tasks.Test
{
	public class TaskTagCRUDTest: AbstractTest
	{
		private Core.Controllers.DictionaryController controller;

		public override void FixtureSetUp()
		{
			base.FixtureSetUp();

			controller = IocContainer.GetInstance<Core.Controllers.DictionaryController>();
		}

		[Test]
		public void LookupProjectTagsWithProjectReturnAdminTags()
		{
			var result = controller.LookupTags(TestProject, ProjectTagModel.TypeCode, null, 0).ToList();

			Assert.That(result, Has.Count.EqualTo(3));
		}

		[Test]
		public void LookupTagsReturnAllAdminTagsForProject()
		{
			M.Tag("t1");
			M.Tag("t2");

			var result = controller.LookupTags(TestProject, TaskTagModel.TypeCode, null, 0).ToList();

			Assert.That(result, Has.Count.EqualTo(2));
		}

		[Test]
		public void LookupWithTermReturnMatched()
		{
			M.Tag("abc");
			M.Tag("zxc");

			var result = controller.LookupTags(TestProject, TaskTagModel.TypeCode, "AB", 0).ToList();

			Assert.That(result, Has.Count.EqualTo(1));
			Assert.That(result[0].Text, Is.EqualTo("abc"));
		}

		[Test]
		public void GetTagsReturnAllAdminTagsInOrderOfFullName()
		{
			M.Tag("a");
			M.Tag("b");
			M.Tag("c");

			var result = controller.GetTags(TestProject, TaskTagModel.TypeCode, 0).ToList();

			Assert.That(result, Has.Count.EqualTo(3));
			Assert.That(result[0].FullName, Is.EqualTo("a"));
			Assert.That(result[1].FullName, Is.EqualTo("b"));
			Assert.That(result[2].FullName, Is.EqualTo("c"));
		}

		[Test]
		public void GetTagsNextPageReturnEmpty()
		{
			M.Tag("t1");
			M.Tag("t2");

			var result = controller.GetTags(TestProject, TaskTagModel.TypeCode, 1).ToList();

			Assert.That(result, Is.Empty);
		}

		[Test]
		public void GetTagsCountReturnAdminTagsCount()
		{
			M.Tag("t1");
			M.Tag("t2");

			var result = controller.GetTagsCount(TestProject, TaskTagModel.TypeCode);

			Assert.That(result, Is.EqualTo(2));
		}

		[Test]
		public void CreateTagReturnCreated()
		{
			var newTag = controller.CreateTag(TestProject, TaskTagModel.TypeCode, Guid.Empty, "zxc") as TaskTagModel;
			Session.Flush();
			M.Track(newTag);

			Assert.That(newTag, Is.Not.Null);
			Assert.That(newTag.FullName, Is.EqualTo("zxc"));
			Assert.That(newTag.Name, Is.EqualTo("zxc"));
			Assert.That(newTag.ProjectCode, Is.EqualTo(TestProject));
			Assert.That(newTag.Parent, Is.Null);
		}

		[Test]
		public void CannotCreateDuplicateTag()
		{
			M.Tag("ttt");
			Session.Clear();

			var vr = controller.CreateTag(TestProject, TaskTagModel.TypeCode, Guid.Empty, "ttt") as ValidationResult;

			Assert.That(vr, Is.Not.Null);
			Assert.That(vr.Success, Is.False);
		}

		[Test]
		public void UpdateTagReturnUpdated()
		{
			var abc = M.Tag("abc");
			Session.Clear();

			var tags = controller.UpdateTag(TestProject, TaskTagModel.TypeCode, abc.Id, "newabc") as IEnumerable<TagModel>;
			Session.Flush();

			Assert.That(tags, Is.Not.Null);
			Assert.That(tags, Has.Count.EqualTo(1));
			Assert.That(tags.First().Name, Is.EqualTo("newabc"));
		}

		[Test]
		public void UpdateTagReturnAffected()
		{
			var parent = M.Tag("parent");
			M.Tag("child", parent);
			Session.Clear();

			var tags = controller.UpdateTag(TestProject, TaskTagModel.TypeCode, parent.Id, "newName") as IEnumerable<TagModel>;
			MainSession.Flush();

			Assert.That(tags, Is.Not.Null);
			Assert.That(tags, Has.Count.EqualTo(2));
			Assert.That(tags, Has.Exactly(1).Matches<TagModel>(t => t.Id == parent.Id));
		}

		[Test]
		public void CannotUpdateToDuplicate()
		{
			var a1 = M.Tag("a1");
			var b1 = M.Tag("b1");
			Session.Clear();

			var vr = controller.UpdateTag(TestProject, TaskTagModel.TypeCode, b1.Id, "a1") as ValidationResult;

			Assert.That(vr, Is.Not.Null);
			Assert.That(vr.Success, Is.False);
		}

		[Test]
		public void DeleteTagReturnId()
		{
			var t = M.Tag("t");

			var ids = controller.DeleteTag(TestProject, TaskTagModel.TypeCode, t.Id) as IEnumerable<Guid>;
			Session.Flush();

			Assert.That(ids, Contains.Item(t.Id));
		}

		[Test]
		public void DeleteCascaseReturnAffectedIds()
		{
			var p = M.Tag("p");
			var c = M.Tag("p", p);
			Session.Clear();

			var ids = controller.DeleteTag(TestProject, TaskTagModel.TypeCode, p.Id) as IEnumerable<Guid>;
			MainSession.Flush();

			Assert.That(ids, Contains.Item(p.Id));
			Assert.That(ids, Contains.Item(c.Id));
		}
	}
}
