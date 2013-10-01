using System;
using AGO.Core.Filters;
using AGO.Core.Model.Example;
using AGO.Core.Application;
using NUnit.Framework;

namespace AGO.Core.Tests
{
	[TestFixture]
	public class FilteringTests : AbstractTestFixture
	{
		[TestFixtureSetUp]
		public void SetUp()
		{
			Initialize();
		}

		[Test]
		public void SimpleFiltersTest()
		{
			var models = _FilteringService.Filter<PrimitiveModel>()
				.Where(m => m.StringProperty == "PrimitiveModel1" && m.BoolProperty)
				.List(_FilteringDao);			
			Assert.AreEqual(1, models.Count);
			Assert.AreEqual("PrimitiveModel1", models[0].StringProperty);

			models = _FilteringService.Filter<PrimitiveModel>()
				.Where(m => m.StringProperty == "PrimitiveModel1" || m.StringProperty == "PrimitiveModel2")
				.List(_FilteringDao);
			Assert.AreEqual(2, models.Count);

			models = _FilteringService.Filter<PrimitiveModel>()
				.WhereString(m => m.StringProperty).Like("Primitive%")
				.List(_FilteringDao);
			Assert.AreEqual(2, models.Count);

			var guid = Guid.Parse("11111111-1111-1111-1111-111111111111");
			models = _FilteringService.Filter<PrimitiveModel>()
				.Where(m => m.GuidProperty == guid)
				.List(_FilteringDao);
			Assert.AreEqual(1, models.Count);
			Assert.AreEqual("PrimitiveModel1", models[0].StringProperty);

			models = _FilteringService.Filter<PrimitiveModel>()
				.Where(m => m.EnumProperty == ExampleEnum.Value1)
				.List(_FilteringDao);
			Assert.AreEqual(1, models.Count);
			Assert.AreEqual("PrimitiveModel1", models[0].StringProperty);

			models = _FilteringService.Filter<PrimitiveModel>()
				.WhereProperty(m => m.NullableBoolProperty).Not().Exists()
				.List(_FilteringDao);
			Assert.AreEqual(1, models.Count);
			Assert.AreEqual("PrimitiveModel1", models[0].StringProperty);

			var startDate = new DateTime(2012, 12, 31);
			var endDate = new DateTime(2013, 1, 2);
			models = _FilteringService.Filter<PrimitiveModel>()
				.Where(m => m.DateTimeProperty > startDate && m.DateTimeProperty < endDate)
				.List(_FilteringDao);
			Assert.AreEqual(1, models.Count);
			Assert.AreEqual("PrimitiveModel1", models[0].StringProperty);

			var startDate2 = new DateTime(2013, 1, 31);
			var endDate2 = new DateTime(2013, 2, 2);
			models = _FilteringService.Filter<PrimitiveModel>()
				.Where(m => m.NullableDateTimeProperty > startDate2 && m.NullableDateTimeProperty < endDate2)
				.List(_FilteringDao);
			Assert.AreEqual(1, models.Count);
			Assert.AreEqual("PrimitiveModel2", models[0].StringProperty);
		}

		[Test]
		public void JoinedFiltersTest()
		{
			var manyEndModels = _FilteringService.Filter<ManyEndModel>()
				.WhereCollection(m => m.OneEndModels).Where(m => m.Name == "OneEndModel1").End()
				.List(_FilteringDao);
			Assert.AreEqual(1, manyEndModels.Count);
			Assert.AreEqual("ManyEndModel1", manyEndModels[0].Name);

			manyEndModels = _FilteringService.Filter<ManyEndModel>()
				.WhereCollection(m => m.OneEndModels).Where(m => m.Name == "OneEndModel2").End()
				.WhereCollection(m => m.PrimitiveModels).Where(m => m.StringProperty == "PrimitiveModel2").End()
				.WhereCollection(m => m.HierarchicalModels).Where(m => m.Name == "HierarchicalModel2").End()
				.List(_FilteringDao);
			Assert.AreEqual(1, manyEndModels.Count);
			Assert.AreEqual("ManyEndModel2", manyEndModels[0].Name);

			manyEndModels = _FilteringService.Filter<ManyEndModel>()
				.Or()
				.WhereCollection(m => m.OneEndModels).Where(m => m.Name == "BadName").End()
				.WhereCollection(m => m.PrimitiveModels).Where(m => m.StringProperty == "PrimitiveModel2").End()
				.List(_FilteringDao);
			Assert.AreEqual(1, manyEndModels.Count);
			Assert.AreEqual("ManyEndModel2", manyEndModels[0].Name);

			var oneEndModels = _FilteringService.Filter<OneEndModel>()
				.WhereModel(m => m.ManyEnd).Where(m => m.Name == "ManyEndModel1").End()
				.List(_FilteringDao);
			Assert.AreEqual(1, oneEndModels.Count);
			Assert.AreEqual("OneEndModel1", oneEndModels[0].Name);

			var manyToManyModels = _FilteringService.Filter<ManyToMany1Model>()
				.WhereCollection(m => m.AssociatedModels).Where(m => m.Name == "ManyToMany2Model").End()
				.List(_FilteringDao);
			Assert.AreEqual(1, manyToManyModels.Count);
			Assert.AreEqual("ManyToMany1Model", manyToManyModels[0].Name);
		}

		[Test]
		public void HierarchyFiltersTest()
		{
			var rootModels = _FilteringService.Filter<HierarchicalModel>()
				.WhereProperty(m => m.Parent).Not().Exists()
				.List(_FilteringDao);
			Assert.AreEqual(1, rootModels.Count);
			Assert.AreEqual("HierarchicalModel1", rootModels[0].Name);

			var rootModel = rootModels[0];
			var firstLevelModels = _FilteringService.Filter<HierarchicalModel>()
				.Where(m => m.Parent == rootModel)
				.List(_FilteringDao);
			Assert.AreEqual(1, firstLevelModels.Count);
			Assert.AreEqual("HierarchicalModel2", firstLevelModels[0].Name);

			firstLevelModels = _FilteringService.Filter<HierarchicalModel>()
				.WhereModel(m => m.Parent).Where(m => m.Name == "HierarchicalModel1").End()
				.List(_FilteringDao);
			Assert.AreEqual(1, firstLevelModels.Count);
			Assert.AreEqual("HierarchicalModel2", firstLevelModels[0].Name);
		}

		[Test]
		public void NestedWhereFiltersTest()
		{
			var models = _FilteringService.Filter<PrimitiveModel>()
				.Where(m => m.ManyEnd.Name == "ManyEndModel2" && m.StringProperty == "PrimitiveModel2")
				.List(_FilteringDao);
			Assert.AreEqual(1, models.Count);
			Assert.AreEqual("PrimitiveModel2", models[0].StringProperty);

			models = _FilteringService.Filter<PrimitiveModel>()
				.WhereString(m => m.ManyEnd.Name).Like("ManyEnd%")
				.WhereProperty(m => m.NullableBoolProperty).Exists()
				.List(_FilteringDao);
			Assert.AreEqual(1, models.Count);
			Assert.AreEqual("PrimitiveModel2", models[0].StringProperty);
		}

		[Test]
		public void FilterSerializationTest()
		{
			var filter = _FilteringService.Filter<ManyEndModel>()
				.WhereCollection(m => m.OneEndModels).Where(m => m.Name == "OneEndModel2").End()
				.WhereCollection(m => m.PrimitiveModels).Where(m => m.StringProperty == "PrimitiveModel2").End()
				.WhereCollection(m => m.HierarchicalModels).Where(m => m.Name == "HierarchicalModel2").End();

			JsonService.CreateSerializer().Serialize(Console.Out, filter);
		}

		[Test]
		public void FilterDeserializationAndQueryTest()
		{
			const string json = @"
			{
				'items': [
					{
						'op': 'exists',
						'path': 'OneEndModels'
					},
					{
						'op': 'exists',
						'path': 'PrimitiveModels'
					},
					{
						'op': 'exists',
						'path': 'HierarchicalModels'
					}
				]
			}";

			IModelFilterNode filter = null;
			Assert.DoesNotThrow(() => filter = _FilteringService.ParseFilterFromJson(json));
			Assert.IsNotNull(filter);

			var models = _FilteringDao.List<ManyEndModel>(new[] {filter});
			Assert.AreEqual(1, models.Count);
			Assert.AreEqual("ManyEndModel2", models[0].Name);
		}

		[Test]
		public void FilterDeserializationAndQueryTest2()
		{
			const string json = @"
			{
				'items': [
					{
						'op': '==',
						'path': 'StringProperty',
						'value': 'PrimitiveModel1'						
					},
					{
						'op': '==',
						'path': 'GuidProperty',
						'value': '11111111-1111-1111-1111-111111111111'						
					},
					{
						'op': '>=',
						'path': 'DateTimeProperty',
						'value': '2013-01-01T00:00:00Z'						
					},
					{
						'op': '<=',
						'path': 'DateTimeProperty',
						'value': '2013-01-01T01:30:00Z'						
					},
					{
						'op': '>',
						'path': 'IntProperty',
						'value': 10						
					},
					{
						'op': '<',
						'path': 'IntProperty',
						'value': 12						
					},
					{
						'op': '>',
						'path': 'DoubleProperty',
						'value': '0.1'						
					},
					{
						'op': '<',
						'path': 'DoubleProperty',
						'value': '0.2'						
					},
					{
						'op': '==',
						'path': 'FloatProperty',
						'value': '0.11'						
					}
				]
			}";

			IModelFilterNode filter = null;
			Assert.DoesNotThrow(() => filter = _FilteringService.ParseFilterFromJson(json));
			Assert.IsNotNull(filter);

			var models = _FilteringDao.List<PrimitiveModel>(new[] { filter });
			Assert.AreEqual(1, models.Count);
			Assert.AreEqual("PrimitiveModel1", models[0].StringProperty);
		}

		[Test]
		public void FilterDeserializationAndQueryTest3()
		{
			const string json = @"
			{
				op: '||',
				'items': [
					{
						'op': 'isnull',
						'path': 'StringProperty'					
					},
					{
						'op': 'like',
						'path': 'StringProperty',
						'value': '%Model1'						
					}
				]
			}";

			IModelFilterNode filter = null;
			Assert.DoesNotThrow(() => filter = _FilteringService.ParseFilterFromJson(json));
			Assert.IsNotNull(filter);

			var models = _FilteringDao.List<PrimitiveModel>(new[] { filter });
			Assert.AreEqual(1, models.Count);
			Assert.AreEqual("PrimitiveModel1", models[0].StringProperty);
		}

		[Test]
		public void FilterConcatenationTest()
		{
			const string json = @"
			{
				'items': [
					{
						'op': '==',
						'path': 'StringProperty',
						'value': 'PrimitiveModel2'				
					},
					{
						'op': '==',
						'path': 'NullableBoolProperty',
						'value': true
					}
				]
			}";

			IModelFilterNode jsonFilter = null;
			Assert.DoesNotThrow(() => jsonFilter = _FilteringService.ParseFilterFromJson(json));
			Assert.IsNotNull(jsonFilter);

			var fixedFilter = _FilteringService.Filter<PrimitiveModel>()
				.Where(m => m.ManyEnd.Name == "ManyEndModel2" && m.EnumProperty == ExampleEnum.Value2);

			var models = _FilteringDao.List<PrimitiveModel>(new[] { jsonFilter, fixedFilter });
			Assert.AreEqual(1, models.Count);
		}
	}
}
