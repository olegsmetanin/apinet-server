using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using AGO.Core.Model.Dictionary;
using AGO.Tasks.Controllers;
using AGO.Tasks.Controllers.DTO;
using AGO.Tasks.Model.Task;
using NUnit.Framework;
using AGO.Core;

namespace AGO.Tasks.Test
{
	/// <summary>
	/// Тесты работы с пользовательскими свойствами задачи
	/// </summary>
	public class TaskUserPropsTest: AbstractTest
	{
		private TasksController controller;
		private TaskModel task;

		public override void FixtureSetUp()
		{
			base.FixtureSetUp();
			controller = IocContainer.GetInstance<TasksController>();
		}
	
		public override void SetUp()
		{
			base.SetUp();

			Thread.CurrentThread.CurrentUICulture = new CultureInfo("en");
			task = M.Task(1);
		}

		[Test]
		public void LookupParamTypesReturnAll()
		{
			var sp = M.ParamType("1sp");
			var np = M.ParamType("2np", CustomPropertyValueType.Number);
			var dp = M.ParamType("3dp", CustomPropertyValueType.Date);

			var result = controller.LookupParamTypes(TestProject, null, 0).ToArray();

			Assert.IsNotNull(result);
			Assert.AreEqual(3, result.Length);

			Assert.AreEqual(sp.Id, result[0].Id);
			StringAssert.AreEqualIgnoringCase(sp.FullName, result[0].Text);
			Assert.AreEqual(CustomPropertyValueType.String, result[0].ValueType);

			Assert.AreEqual(np.Id, result[1].Id);
			StringAssert.AreEqualIgnoringCase(np.FullName, result[1].Text);
			Assert.AreEqual(CustomPropertyValueType.Number, result[1].ValueType);

			Assert.AreEqual(dp.Id, result[2].Id);
			StringAssert.AreEqualIgnoringCase(dp.FullName, result[2].Text);
			Assert.AreEqual(CustomPropertyValueType.Date, result[2].ValueType);
		}

		[Test]
		public void CreateValidParamReturnSuccess()
		{
			var sp = M.ParamType();
			var model = new CustomParameterDTO
			            	{
			            		Type = new CustomParameterTypeDTO {Id = sp.Id},
			            		Value = "123"
			            	};

			var ur = controller.EditParam(task.Id, model);
			_SessionProvider.FlushCurrentSession(!ur.Validation.Success);
			
			Assert.IsTrue(ur.Validation.Success);
			Assert.IsNotNull(ur.Model);
			Assert.AreNotEqual(Guid.Empty, ur.Model.Id);
			Assert.AreEqual(sp.Id, ur.Model.Type.Id);
			Assert.AreEqual("123", ur.Model.Value);
			Session.Refresh(task);
			Assert.AreEqual(1, task.CustomProperties.Count);
			Assert.AreEqual(ur.Model.Id, task.CustomProperties.First().Id);
			M.Track(task.CustomProperties.First());
		}

		[Test]
		public void UpdateStrParamReturnSuccess()
		{
			var p = M.Param(task, "s1", "123");
			var model = new CustomParameterDTO
			{
				Id = p.Id,
				Type = new CustomParameterTypeDTO { Id = p.PropertyType.Id },
				Value = "456"
			};

			var ur = controller.EditParam(task.Id, model);
			_SessionProvider.FlushCurrentSession(!ur.Validation.Success);

			Assert.IsTrue(ur.Validation.Success);
			Assert.IsNotNull(ur.Model);
			Assert.AreEqual("456", ur.Model.Value);
			Session.Refresh(task);
			Assert.AreEqual(1, task.CustomProperties.Count);
			Assert.AreEqual(ur.Model.Id, task.CustomProperties.First().Id);
			Assert.AreEqual("456", task.CustomProperties.First().StringValue);
		}

		[Test]
		public void UpdateNumParamReturnSuccess()
		{
			var p = M.Param(task, "n1", 1m);
			var model = new CustomParameterDTO
			{
				Id = p.Id,
				Type = new CustomParameterTypeDTO { Id = p.PropertyType.Id },
				Value = "12.2"
			};

			var ur = controller.EditParam(task.Id, model);
			_SessionProvider.FlushCurrentSession(!ur.Validation.Success);

			Assert.IsTrue(ur.Validation.Success);
			Assert.IsNotNull(ur.Model);
			Assert.AreEqual(12.2m, ur.Model.Value);
			Session.Refresh(task);
			Assert.AreEqual(1, task.CustomProperties.Count);
			Assert.AreEqual(ur.Model.Id, task.CustomProperties.First().Id);
			Assert.AreEqual(12.2m, task.CustomProperties.First().NumberValue);
		}

		[Test]
		public void UpdateDateParamReturnSuccess()
		{
			var d = new DateTime(2013, 10, 10, 12, 12, 00, DateTimeKind.Utc);
			var p = M.Param(task, "d1", d);
			var model = new CustomParameterDTO
			{
				Id = p.Id,
				Type = new CustomParameterTypeDTO { Id = p.PropertyType.Id },
				Value = "11/09/2013 14:16"
			};

			var ur = controller.EditParam(task.Id, model);
			_SessionProvider.FlushCurrentSession(!ur.Validation.Success);

			Assert.IsTrue(ur.Validation.Success);
			Assert.IsNotNull(ur.Model);
			Assert.AreEqual(new DateTime(2013, 11, 09, 14, 16, 00, DateTimeKind.Utc), ur.Model.Value);
			Session.Refresh(task);
			Assert.AreEqual(1, task.CustomProperties.Count);
			Assert.AreEqual(ur.Model.Id, task.CustomProperties.First().Id);
			Assert.AreEqual(new DateTime(2013, 11, 09, 14, 16, 00), task.CustomProperties.First().DateValue.ToLocalTime());
		}

		[Test]
		public void DeleteParamReturnSuccess()
		{
			var p = M.Param(task, "s1", "spv");

			var res = controller.DeleteParam(p.Id);
			_SessionProvider.FlushCurrentSession(!res);

			Assert.IsTrue(res);
			p = Session.Get<TaskCustomPropertyModel>(p.Id);
			Assert.IsNull(p);
			Session.Refresh(task);
			Assert.AreEqual(0, task.CustomProperties.Count);
		}
	}
}