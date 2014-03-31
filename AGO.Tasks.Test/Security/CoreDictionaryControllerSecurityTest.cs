using System;
using System.Collections.Generic;
using System.Linq;
using AGO.Core;
using AGO.Core.Controllers;
using AGO.Core.Filters;
using AGO.Core.Model.Dictionary;
using AGO.Core.Model.Security;
using AGO.Core.Security;
using AGO.Tasks.Model.Dictionary;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace AGO.Tasks.Test.Security
{
	public class CoreDictionaryControllerSecurityTest: AbstractSecurityTest
	{
		private DictionaryController controller;

		public override void FixtureSetUp()
		{
			base.FixtureSetUp();

			controller = IocContainer.GetInstance<DictionaryController>();
		}

		#region Param types

		[Test]
		public void OnlyMembersCanLookupParamTypes()
		{
			M.ParamType("p1");
			M.ParamType("p2");

			Func<UserModel, LookupEntry[]> action = u =>
			{
				Login(u.Email);
				return controller.LookupCustomPropertyTypes(TestProject, 0, null).ToArray();
			};

			ReusableConstraint denied = Throws.Exception.TypeOf<NoSuchProjectMemberException>();
			ReusableConstraint granted = Has.Length.EqualTo(2);

			Assert.That(() => action(admin), denied);
			Assert.That(action(projAdmin), granted);
			Assert.That(action(projManager), granted);
			Assert.That(action(projExecutor), granted);
			Assert.That(() => action(notMember), denied);
		}

		[Test]
		public void OnlyMembersCanGetParamTypes()
		{
			M.ParamType("p1");
			M.ParamType("p2");

			Func<UserModel, CustomPropertyTypeModel[]> action = u =>
			{
				Login(u.Email);
				return controller.GetCustomPropertyTypes(TestProject, 
					Enumerable.Empty<IModelFilterNode>().ToList(),
					new [] {new SortInfo { Property = "FullName" }}.ToList(),
					0).ToArray();
			};

			ReusableConstraint denied = Throws.Exception.TypeOf<NoSuchProjectMemberException>();
			ReusableConstraint granted = Has.Length.EqualTo(2);

			Assert.That(() => action(admin), denied);
			Assert.That(action(projAdmin), granted);
			Assert.That(action(projManager), granted);
			Assert.That(action(projExecutor), granted);
			Assert.That(() => action(notMember), denied);
		}

		[Test]
		public void OnlyMembersCanGetParamTypesCount()
		{
			M.ParamType("p1");
			M.ParamType("p2");

			Func<UserModel, int> action = u =>
			{
				Login(u.Email);
				return controller.GetCustomPropertyTypesCount(TestProject, Enumerable.Empty<IModelFilterNode>().ToList());
			};

			ReusableConstraint denied = Throws.Exception.TypeOf<NoSuchProjectMemberException>();
			ReusableConstraint granted = Is.EqualTo(2);

			Assert.That(() => action(admin), denied);
			Assert.That(action(projAdmin), granted);
			Assert.That(action(projManager), granted);
			Assert.That(action(projExecutor), granted);
			Assert.That(() => action(notMember), denied);
		}

		[Test]
		public void OnlyMembersCanGetParamType()
		{
			var p1 = M.ParamType("p1");

			Func<UserModel, CustomPropertyTypeModel> action = u =>
			{
				Login(u.Email);
				return controller.GetCustomPropertyType(TestProject, p1.Id);
			};

			ReusableConstraint denied = Throws.Exception.TypeOf<NoSuchProjectMemberException>();
			ReusableConstraint granted = Has.Property("Id").EqualTo(p1.Id);

			Assert.That(() => action(admin), denied);
			Assert.That(action(projAdmin), granted);
			Assert.That(action(projManager), granted);
			Assert.That(action(projExecutor), granted);
			Assert.That(() => action(notMember), denied);
		}

		[Test]
		public void OnlyProjAdminOrMgrCanCreateParamType()
		{
			Func<UserModel, bool> action = u =>
			{
				var data = new CustomPropertyTypeModel
				{
					Name = u.Id.ToString(),
					ValueType = CustomPropertyValueType.String
				};
				Login(u.Email);
				var result = controller.CreateCustomPropertyType(TestProject, data);
				if (result is CustomPropertyTypeModel)
				{
					M.Track(result as CustomPropertyTypeModel);
					return true;
				}
				return ((ValidationResult) result).Success;
			};

			ReusableConstraint denied = Throws.Exception.TypeOf<NoSuchProjectMemberException>();
			ReusableConstraint restricted = Throws.Exception.TypeOf<CreationDeniedException>();
			ReusableConstraint granted = Is.True;

			Assert.That(() => action(admin), denied);
			Assert.That(action(projAdmin), granted);
			Assert.That(action(projManager), granted);
			Assert.That(() => action(projExecutor), restricted);
			Assert.That(() => action(notMember), denied);
		}

		[Test]
		public void OnlyProjAdminOrMgrCanUpdateParamType()
		{
			var counter = 1;
			Func<UserModel, bool> action = u =>
			{
				Login(u.Email);
				var p = M.ParamType("p" + counter);
				
				var data = new PropChangeDTO
				{
					Id = p.Id,
					ModelVersion = p.ModelVersion,
					Prop = "Name",
					Value = "newName" + counter
				};
				counter++;
				var result = controller.UpdateCustomPropertyType(TestProject, data);
				var vr = result as ValidationResult;
				if (vr != null)
				{
					return vr.Success;
				}
				return true;
			};

			ReusableConstraint denied = Throws.Exception.TypeOf<NoSuchProjectMemberException>();
			ReusableConstraint restricted = Throws.Exception.TypeOf<ChangeDeniedException>();
			ReusableConstraint granted = Is.True;

			Assert.That(() => action(admin), denied);
			Assert.That(action(projAdmin), granted);
			Assert.That(action(projManager), granted);
			Assert.That(() => action(projExecutor), restricted);
			Assert.That(() => action(notMember), denied);
		}

		[Test]
		public void OnlyProjAdminOrMgrCanDeleteParamType()
		{
			var counter = 1;
			Func<UserModel, bool> action = u =>
			{
				Login(u.Email);
				var p = M.ParamType("p" + counter);
				counter++;
				var result = controller.DeleteCustomPropertyType(TestProject, p.Id);
				var vr = result as ValidationResult;
				if (vr != null)
				{
					return vr.Success;
				}
				return true;
			};

			ReusableConstraint denied = Throws.Exception.TypeOf<NoSuchProjectMemberException>();
			ReusableConstraint restricted = Throws.Exception.TypeOf<DeleteDeniedException>();
			ReusableConstraint granted = Is.True;

			Assert.That(() => action(admin), denied);
			Assert.That(action(projAdmin), granted);
			Assert.That(action(projManager), granted);
			Assert.That(() => action(projExecutor), restricted);
			Assert.That(() => action(notMember), denied);
		}

		#endregion

		#region Task tag
		
		[Test]
		public void OnlyMembersCanLookupOwnTags()
		{
			var admintTag = M.Tag(projAdmin.Email, owner: projAdmin);
			var mgrTag = M.Tag(projManager.Email, owner: projManager);
			var execTag = M.Tag(projExecutor.Email, owner: projExecutor);
		
			Func<UserModel, LookupEntry[]> action = u =>
			{
				Login(u.Email);
				return controller.LookupTags(TestProject, TaskTagModel.TypeCode, null, 0).ToArray();
			};
			Func<TaskTagModel, ReusableConstraint> granted = tag => Has.Length.EqualTo(1)
				.And.Exactly(1).Matches<LookupEntry>(e => e.Text == tag.FullName);
			ReusableConstraint denied = Throws.Exception.TypeOf<NoSuchProjectMemberException>();
		
			Assert.That(() => action(admin), denied);
			Assert.That(action(projAdmin), granted(admintTag));
			Assert.That(action(projManager), granted(mgrTag));
			Assert.That(action(projExecutor), granted(execTag));
			Assert.That(() => action(notMember), denied);
		}
		
		[Test]
		public void OnlyMembersCanGetOwnTags()
		{
			var admintTag = M.Tag(projAdmin.Email, owner: projAdmin);
			var mgrTag = M.Tag(projManager.Email, owner: projManager);
			var execTag = M.Tag(projExecutor.Email, owner: projExecutor);
		
			Func<UserModel, TagModel[]> action = u =>
			{
				Login(u.Email);
				return controller.GetTags(TestProject, TaskTagModel.TypeCode, 0).ToArray();
			};
			Func<TaskTagModel, ReusableConstraint> granted = tag => Has.Length.EqualTo(1)
				.And.Exactly(1).Matches<TaskTagModel>(e => e.Name == tag.FullName);
			ReusableConstraint denied = Throws.Exception.TypeOf<NoSuchProjectMemberException>();
		
			Assert.That(() => action(admin), denied);
			Assert.That(action(projAdmin), granted(admintTag));
			Assert.That(action(projManager), granted(mgrTag));
			Assert.That(action(projExecutor), granted(execTag));
			Assert.That(() => action(notMember), denied);
		}
		
		[Test]
		public void OnlyMembersCanCreateOwnTags()
		{
			Func<UserModel, bool> action = u =>
			{
				Login(u.Email);
				var result = controller.CreateTag(TestProject, TaskTagModel.TypeCode, Guid.Empty, u.Id.ToString());
				var tag = result as TagModel;
				if (tag != null)
				{
					M.Track(tag);
					return true;
				}
				return ((ValidationResult) result).Success;
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
		public void OnlyMembersCanEditOwnTags()
		{
			Func<UserModel, bool> action = u =>
			{
				var utag = M.Tag(u.Id.ToString(), owner: u);
				Login(u.Email);
				var result = controller.UpdateTag(TestProject, TaskTagModel.TypeCode, utag.Id, "aaa");
				var vr = result as ValidationResult;
				if (vr != null) return vr.Success;
				return true;
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
		public void OnlyMembersCanDeleteOwnTags()
		{
			var admintTag = M.Tag(projAdmin.Email, owner: projAdmin);
			var mgrTag = M.Tag(projManager.Email, owner: projManager);
			var execTag = M.Tag(projExecutor.Email, owner: projExecutor);
		
			Func<UserModel, Guid, IEnumerable<Guid>> action = (u, id) =>
			{				
				Login(u.Email);
				var result = controller.DeleteTag(TestProject, TaskTagModel.TypeCode, id);
				return result as IEnumerable<Guid>;
			};
			Func<Guid, ReusableConstraint> granted = id => Contains.Item(id);
			ReusableConstraint restricted = Throws.Exception.TypeOf<DeleteDeniedException>();
			ReusableConstraint denied = Throws.Exception.TypeOf<NoSuchProjectMemberException>();
		
			//not project members
			Assert.That(() => action(admin, mgrTag.Id), denied);
			Assert.That(() => action(notMember, execTag.Id), denied);
			//not own tags
			Assert.That(() => action(projAdmin, mgrTag.Id), restricted);
			Assert.That(() => action(projManager, execTag.Id), restricted);
			Assert.That(() => action(projExecutor, admintTag.Id), restricted);
			//own tags
			Assert.That(action(projAdmin, admintTag.Id), granted(admintTag.Id));
			Assert.That(action(projManager, mgrTag.Id), granted(mgrTag.Id));
			Assert.That(action(projExecutor, execTag.Id), granted(execTag.Id));
		}
		
		#endregion
	}
}
