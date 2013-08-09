using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;

namespace AGO.Hibernate.AutoMapping
{
	public class SubclassConvention : ISubclassConvention
	{
		public void Apply(ISubclassInstance instance)
		{
			instance.DiscriminatorValue(instance.EntityType.FullName);
		}
	}
}
