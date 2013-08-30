using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.AcceptanceCriteria;
using FluentNHibernate.Conventions.Inspections;
using FluentNHibernate.Conventions.Instances;
using AGO.Core.Model.Lob;

namespace AGO.Core.AutoMapping
{
	public class UserTypeConvention: IUserTypeConvention
	{
		public void Accept(IAcceptanceCriteria<IPropertyInspector> criteria)
		{
			criteria.Expect(pi => typeof (Blob).IsAssignableFrom(pi.Property.PropertyType) ||
				typeof (Clob).IsAssignableFrom(pi.Property.PropertyType));
		}

		public void Apply(IPropertyInstance instance)
		{
			if (typeof(Blob).IsAssignableFrom(instance.Property.PropertyType))
				instance.CustomType(typeof(BlobType));
			if (typeof(Clob).IsAssignableFrom(instance.Property.PropertyType))
				instance.CustomType(typeof(ClobType));
		}
	}
}
