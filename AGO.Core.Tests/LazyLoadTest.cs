using System;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Dialect;
using NHibernate.Driver;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace AGO.Core.Tests
{
	[TestFixture]
	public class LazyLoadTest
	{
		private ISessionFactory sf;

		[TestFixtureSetUp]
		public void Init()
		{
			var cfg = new Configuration();

			cfg.DataBaseIntegration(c =>
				                        {
				                        	c.ConnectionString =
				                        		@"Data Source=(local)\sql2008; Database=AGO_Docstore_Next; User ID=ago_user; Password=123;";
				                        	c.Driver<Sql2008ClientDriver>();
				                        	c.Dialect<MsSql2008Dialect>();

				                        	c.LogSqlInConsole = true;
				                        	c.LogFormattedSql = true;
				                        	c.AutoCommentSql = true;
				                        });
			cfg.AddFile("Person.hbm.xml");

			sf = cfg.BuildSessionFactory();
		}

		[TestFixtureTearDown]
		public void TearDown()
		{
			var s = sf.OpenSession();
			s.Delete(s.Get<Person>(1));
			s.Flush();
			s.Close();

			sf.Close();
		}

		[Test]
		public void LazyLoadBytesArray()
		{
			var data = new byte[] {0x0, 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9};

			var s = sf.OpenSession();
			var p = new Person {Id = 1};
			s.Save(p);
			s.Flush();
			s.Close();

			s = sf.OpenSession();
			p = s.Get<Person>(1);
			Load(p, data);
			s.Update(p);
			s.Flush();
			s.Close();

			s = sf.OpenSession();
			p = s.Get<Person>(1);
			Assert.IsNotNull(p);
			Assert.AreEqual(1, p.Id);
			CollectionAssert.AreEqual(data, p.Data);
			s.Close();
		}

		private void Load(IResource r, byte[] buffer)
		{
			r.Data = buffer;
		}
	}

	public interface IResource
	{
		byte[] Data { get; set; }
	}

	public class Person: IResource
	{
		public virtual int Id { get; set; }

		public virtual byte[] Data { get; set; }
	}
}