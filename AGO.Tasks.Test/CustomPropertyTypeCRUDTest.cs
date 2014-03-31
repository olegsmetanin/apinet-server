using System;
using System.Collections.Generic;
using System.Linq;
using AGO.Core;
using AGO.Core.Controllers;
using AGO.Core.Filters;
using AGO.Core.Model.Dictionary;
using NUnit.Framework;

namespace AGO.Tasks.Test
{
	/// <summary>
	/// Tests for custom prop types crud placed here, because in core we can't test entity, that live in project db.
	/// Tasks module in this case act as a reference implementation module.
	/// </summary>
	public class CustomPropertyTypeCRUDTest: AbstractTest
	{

		private DictionaryController controller;

		public override void FixtureSetUp()
		{
			base.FixtureSetUp();

			controller = IocContainer.GetInstance<DictionaryController>();
		}

		[Test]
		public void LookupReturnMatched()
		{
			M.ParamType("abc");
			M.ParamType("asd");
			M.ParamType("zxc");

			var result = controller.LookupCustomPropertyTypes(TestProject, 0, "A").ToList();

			Assert.That(result, Has.Count.EqualTo(2));
			Assert.That(result[0].Text, Is.EqualTo("abc"));
			Assert.That(result[1].Text, Is.EqualTo("asd"));
		}

		[Test]
		public void GetTypesReturnAllInOrderOfFullName()
		{
			M.ParamType("a");
			M.ParamType("b");
			M.ParamType("c");

			var result = controller.GetCustomPropertyTypes(TestProject,
				Enumerable.Empty<IModelFilterNode>().ToList(),
				new [] { new SortInfo { Property = "FullName"} },
				0).ToList();

			Assert.That(result, Has.Count.EqualTo(3));
			Assert.That(result[0].FullName, Is.EqualTo("a"));
			Assert.That(result[1].FullName, Is.EqualTo("b"));
			Assert.That(result[2].FullName, Is.EqualTo("c"));
		}

		[Test]
		public void GetTypesCount()
		{
			M.ParamType("t1");
			M.ParamType("t2");

			var result = controller.GetCustomPropertyTypesCount(TestProject, Enumerable.Empty<IModelFilterNode>().ToList());

			Assert.That(result, Is.EqualTo(2));
		}

		[Test]
		public void CreateTypeReturnCreated()
		{
			var input = new CustomPropertyTypeModel
			{
				Name = "abc",
				ValueType = CustomPropertyValueType.Number,
				Format = "N2"
			};
			var result = controller.CreateCustomPropertyType(TestProject, input) as CustomPropertyTypeModel;
			if (result != null)
				M.Track(result);
			Session.Flush();

			Assert.That(result, Is.Not.Null);
			Assert.That(result.ProjectCode, Is.EqualTo(TestProject));
			Assert.That(result.Parent, Is.Null);
			Assert.That(result.Name, Is.EqualTo("abc"));
			Assert.That(result.FullName, Is.EqualTo("abc"));
			Assert.That(result.ValueType, Is.EqualTo(CustomPropertyValueType.Number));
			Assert.That(result.Format, Is.EqualTo("N2"));
		}

		[Test]
		public void CannotCreateDuplicateType()
		{
			M.ParamType("str1");

			var input = new CustomPropertyTypeModel
			{
				Name = "str1",
				ValueType = CustomPropertyValueType.String
			};
			var result = controller.CreateCustomPropertyType(TestProject, input) as ValidationResult;
			
			Assert.That(result, Is.Not.Null);
			Assert.That(result.Success, Is.False);
		}

		[Test]
		public void CreateTypeWithSameNameInOtherParent()
		{
			var p1 = M.ParamType("p1");
			var p2 = M.ParamType("p2");
			M.ParamType("sub", parent: p1);
			var input = new CustomPropertyTypeModel
			{
				Name = "sub",
				ParentId = p2.Id,
				ValueType = CustomPropertyValueType.Number,
			};
			var result = controller.CreateCustomPropertyType(TestProject, input) as CustomPropertyTypeModel;
			if (result != null)
				M.Track(result);
			Session.Flush();

			Assert.That(result, Is.Not.Null);
			Assert.That(result.ProjectCode, Is.EqualTo(TestProject));
			Assert.That(result.Parent, Is.Not.Null);
			Assert.That(result.Parent.Id, Is.EqualTo(p2.Id));
			Assert.That(result.Name, Is.EqualTo("sub"));
			Assert.That(result.FullName, Is.EqualTo("p2 / sub"));
		}

		[Test]
		public void UpdateTagReturnUpdated()
		{
			var abc = M.ParamType("abc");
			Session.Clear();
			var data = new PropChangeDTO
			{
				Id = abc.Id,
				ModelVersion = abc.ModelVersion,
				Prop = "Name",
				Value = "newabc"
			};

			var prms = controller.UpdateCustomPropertyType(TestProject, data) as IEnumerable<CustomPropertyTypeModel>;
			Session.Flush();

			Assert.That(prms, Is.Not.Null);
			Assert.That(prms, Has.Count.EqualTo(1));
			Assert.That(prms.First().Name, Is.EqualTo("newabc"));
		}

		[Test]
		public void UpdateTagReturnAffected()
		{
			var parent = M.ParamType("parent");
			M.ParamType("child", parent: parent);
			Session.Clear();

			var data = new PropChangeDTO
			{
				Id = parent.Id,
				ModelVersion = parent.ModelVersion,
				Prop = "Name",
				Value = "ppp"
			};
			var prms = controller.UpdateCustomPropertyType(TestProject, data) as IEnumerable<CustomPropertyTypeModel>;
			Session.Flush();

			Assert.That(prms, Is.Not.Null);
			Assert.That(prms, Has.Count.EqualTo(2));
			Assert.That(prms, Has.Exactly(1).Matches<CustomPropertyTypeModel>(t => t.Id == parent.Id));
		}

		[Test]
		public void CannotUpdateToDuplicate()
		{
			M.ParamType("a1");
			var b1 = M.ParamType("b1");
			Session.Clear();

			var data = new PropChangeDTO
			{
				Id = b1.Id,
				ModelVersion = b1.ModelVersion,
				Prop = "Name",
				Value = "a1"
			};
			var vr = controller.UpdateCustomPropertyType(TestProject, data) as ValidationResult;

			Assert.That(vr, Is.Not.Null);
			Assert.That(vr.Success, Is.False);
		}

		[Test]
		public void DeleteReturnId()
		{
			var p = M.ParamType("p");

			var ids = controller.DeleteCustomPropertyType(TestProject, p.Id) as IEnumerable<Guid>;
			Session.Flush();

			Assert.That(ids, Contains.Item(p.Id));
		}

		[Test]
		public void DeleteCascaseReturnAffectedIds()
		{
			var p = M.ParamType("p");
			var c = M.ParamType("p", parent: p);
			Session.Clear();

			var ids = controller.DeleteCustomPropertyType(TestProject, p.Id) as IEnumerable<Guid>;
			Session.Flush();

			Assert.That(ids, Contains.Item(p.Id));
			Assert.That(ids, Contains.Item(c.Id));
		}

		[Test]
		public void CannotDeleteReferenced()
		{
			var pt = M.ParamType("p");
			var t = M.Task(1);
			M.Param(t, pt, "adfasf");

			try
			{
				controller.DeleteCustomPropertyType(TestProject, pt.Id);
				Assert.Fail("Exception wasn't throws");
			}
			catch (Exception e)
			{
				Assert.That(e, Is.TypeOf<CannotDeleteReferencedItemException>());
			}
		}
	}
}
