using System;
using System.Collections.Generic;
using System.Linq;
using AGO.Core.Controllers;
using AGO.Core.Model.Dictionary;
using AGO.Core.Model.Dictionary.Projects;
using NUnit.Framework;

namespace AGO.Core.Tests
{
	/// <summary>
	/// Tests of CRUD methods for tags in dictionary controller.
	/// Because purpose of controller methods is universal tags crud implementation, test split in 
	/// two parts: project tags (tested here) and task tags (tested in tasks module)
	/// </summary>
	public class TagCRUDTests: AbstractPersistenceTest<ModelHelper>
	{
		private DictionaryController controller;

		public override void FixtureSetUp()
		{
			base.FixtureSetUp();

			controller = IocContainer.GetInstance<DictionaryController>();
		}

		protected override void CreateModelHelpers()
		{
			FM = new ModelHelper(() => MainSession, () => CurrentUser);
			M = new ModelHelper(() => MainSession, () => CurrentUser);
		}

		[Test]
		public void LookupTagsReturnAllAdminTags()
		{
			var result = controller.LookupTags(null, ProjectTagModel.TypeCode, null, 0).ToList();

			Assert.That(result, Has.Count.EqualTo(3));
		}

		[Test]
		public void LookupWithTermReturnMatched()
		{
			var result = controller.LookupTags(null, ProjectTagModel.TypeCode, "PAY", 0).ToList();

			Assert.That(result, Has.Count.EqualTo(1));
			Assert.That(result[0].Text, Is.EqualTo("Pay attention"));
		}

		[Test]
		public void GetTagsReturnAllAdminTagsInOrderOfFullName()
		{
			var result = controller.GetTags(null, ProjectTagModel.TypeCode, 0).ToList();

			Assert.That(result, Has.Count.EqualTo(3));
			Assert.That(result[0].FullName, Is.EqualTo("Important"));
			Assert.That(result[1].FullName, Is.EqualTo("Pay attention"));
			Assert.That(result[2].FullName, Is.EqualTo("Urgent"));
		}

		[Test]
		public void GetTagsNextPageReturnEmpty()
		{
			var result = controller.GetTags(null, ProjectTagModel.TypeCode, 1).ToList();

			Assert.That(result, Is.Empty);
		}

		[Test]
		public void GetTagsCountReturnAdminTagsCount()
		{
			var result = controller.GetTagsCount(null, ProjectTagModel.TypeCode);

			Assert.That(result, Is.EqualTo(3));
		}

		[Test]
		public void CreateTagReturnCreated()
		{
			var newTag = controller.CreateTag(null, ProjectTagModel.TypeCode, Guid.Empty, "zxc") as ProjectTagModel;
			MainSession.Flush();				
			M.Track(newTag);

			Assert.That(newTag, Is.Not.Null);
			Assert.That(newTag.FullName, Is.EqualTo("zxc"));
			Assert.That(newTag.Name, Is.EqualTo("zxc"));
			Assert.That(newTag.ProjectCode, Is.Null);
			Assert.That(newTag.Parent, Is.Null);
		}

		[Test]
		public void CannotCreateDuplicateTag()
		{
			var t = M.ProjectTag("ttt");
			MainSession.Clear();

			var vr = controller.CreateTag(null, ProjectTagModel.TypeCode, Guid.Empty, "ttt") as ValidationResult;

			Assert.That(vr, Is.Not.Null);
			Assert.That(vr.Success, Is.False);
		}

		[Test]
		public void UpdateTagReturnUpdated()
		{
			var abc = M.ProjectTag("abc");
			MainSession.Clear();

			var tags = controller.UpdateTag(null, ProjectTagModel.TypeCode, abc.Id, "newabc") as IEnumerable<TagModel>;
			MainSession.Flush();

			Assert.That(tags, Is.Not.Null);
			Assert.That(tags, Has.Count.EqualTo(1));
			Assert.That(tags.First().Name, Is.EqualTo("newabc"));
		}

		[Test]
		public void UpdateTagReturnAffected()
		{
			var parent = M.ProjectTag("parent");
			M.ProjectTag("child", parent: parent);
			MainSession.Clear();

			var tags = controller.UpdateTag(null, ProjectTagModel.TypeCode, parent.Id, "newName") as IEnumerable<TagModel>;
			MainSession.Flush();

			Assert.That(tags, Is.Not.Null);
			Assert.That(tags, Has.Count.EqualTo(2));
			Assert.That(tags, Has.Exactly(1).Matches<TagModel>(t => t.Id == parent.Id));
		}

		[Test]
		public void CannotUpdateToDuplicate()
		{
			var a1 = M.ProjectTag("a1");
			var b1 = M.ProjectTag("b1");
			MainSession.Clear();

			var vr = controller.UpdateTag(null, ProjectTagModel.TypeCode, b1.Id, "a1") as ValidationResult;

			Assert.That(vr, Is.Not.Null);
			Assert.That(vr.Success, Is.False);
		}

		[Test]
		public void DeleteTagReturnId()
		{
			var t = M.ProjectTag("t");

			var ids = controller.DeleteTag(null, ProjectTagModel.TypeCode, t.Id) as IEnumerable<Guid>;
			MainSession.Flush();

			Assert.That(ids, Contains.Item(t.Id));
		}

		[Test]
		public void DeleteCascaseReturnAffectedIds()
		{
			var p = M.ProjectTag("p");
			var c = M.ProjectTag("p", parent: p);
			MainSession.Clear();

			var ids = controller.DeleteTag(null, ProjectTagModel.TypeCode, p.Id) as IEnumerable<Guid>;
			MainSession.Flush();

			Assert.That(ids, Contains.Item(p.Id));
			Assert.That(ids, Contains.Item(c.Id));
		}
	}
}