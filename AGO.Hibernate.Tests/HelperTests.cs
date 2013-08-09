using System;
using System.Collections.Generic;
using System.IO;
using AGO.Hibernate.Application;
using AGO.Hibernate.Model.Example;
using NUnit.Framework;

namespace AGO.Hibernate.Tests
{
	[TestFixture]
	public class HelperTests : AbstractApplication
	{
		[Test]
		public void CreateAndPopulateDatabase()
		{
			_AlternateHibernateConfigRegex = "^AlternateHibernate_(.*)";
			InitContainer();

			var createDatabaseStream = typeof(FilteringTests).Assembly.GetManifestResourceStream(
				typeof(FilteringTests).Assembly.GetName().Name + ".CreateDatabase.sql");
			if (createDatabaseStream == null)
				throw new InvalidOperationException();

			string script;
			using (var reader = new StreamReader(createDatabaseStream))
				script = reader.ReadToEnd();

			ExecuteNonQuery(script);
			DoPopulateDatabase();
		}

		[Test]
		public void PopulateDatabase()
		{
			InitContainer();
			DoPopulateDatabase();
		}

		protected void DoPopulateDatabase()
		{
			var manyEndModel1 = new ManyEndModel
			{
				Name = "ManyEndModel1"
			};
			_CrudDao.Store(manyEndModel1);

			var manyEndModel2 = new ManyEndModel
			{
				Name = "ManyEndModel2"
			};
			_CrudDao.Store(manyEndModel2);

			var oneEndModel1 = new OneEndModel
			{
				Name = "OneEndModel1",
				ManyEnd = manyEndModel1
			};
			_CrudDao.Store(oneEndModel1);

			var oneEndModel2 = new OneEndModel
			{
				Name = "OneEndModel2",
				ManyEnd = manyEndModel2
			};
			_CrudDao.Store(oneEndModel2);

			var primitiveModel1 = new PrimitiveModel
			{
				StringProperty = "PrimitiveModel1",
				GuidProperty = Guid.Parse("11111111-1111-1111-1111-111111111111"),
				DateTimeProperty = new DateTime(2013, 1, 1),
				EnumProperty = ExampleEnum.Value1,
				BoolProperty = true,
				ByteProperty = 11,
				CharProperty = '1',
				DecimalProperty = 0.11m,
				DoubleProperty = 0.11,
				FloatProperty = 0.11f,
				IntProperty = 11,
				LongProperty = 11,
			};
			_CrudDao.Store(primitiveModel1);

			var primitiveModel2 = new PrimitiveModel
			{
				StringProperty = "PrimitiveModel2",
				DateTimeProperty = DateTime.Now,
				EnumProperty = ExampleEnum.Value2,

				NullableGuidProperty = Guid.Parse("22222222-2222-2222-2222-222222222222"),
				NullableDateTimeProperty = new DateTime(2013, 2, 1),
				NullableEnumProperty = ExampleEnum.Value2,
				NullableBoolProperty = true,
				NullableByteProperty = 22,
				NullableCharProperty = '2',
				NullableDecimalProperty = 0.22m,
				NullableDoubleProperty = 0.22,
				NullableFloatProperty = 0.22f,
				NullableIntProperty = 22,
				NullableLongProperty = 22,

				ManyEnd = manyEndModel2
			};
			_CrudDao.Store(primitiveModel2);

			var hierarchicalModel1 = new HierarchicalModel
			{
				Name = "HierarchicalModel1"
			};
			_CrudDao.Store(hierarchicalModel1);

			var hierarchicalModel2 = new HierarchicalModel
			{
				Name = "HierarchicalModel2",
				Parent = hierarchicalModel1,

				ManyEnd = manyEndModel2
			};
			_CrudDao.Store(hierarchicalModel2);

			var hierarchicalModel3 = new HierarchicalModel
			{
				Name = "HierarchicalModel3",
				Parent = hierarchicalModel2
			};
			_CrudDao.Store(hierarchicalModel3);

			var manyToMany2Model = new ManyToMany2Model { Name = "ManyToMany2Model" };
			_CrudDao.Store(manyToMany2Model);

			var manyToMany1Model = new ManyToMany1Model { Name = "ManyToMany1Model" };
			manyToMany1Model.AssociatedModels.Add(manyToMany2Model);
			_CrudDao.Store(manyToMany1Model);

			_SessionProvider.CurrentSession.Flush();
		}

		#region Container initialization

		protected override void Register()
		{
			RegisterEnvironment();
			RegisterPersistence();
		}

		protected override void AfterSingletonsInitialized(IList<IInitializable> initializedServices)
		{
			InitializeEnvironment(initializedServices);
			InitializePersistence(initializedServices);
		}

		#endregion
	}
}
