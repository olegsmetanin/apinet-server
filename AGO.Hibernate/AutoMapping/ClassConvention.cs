using System.Linq;
using AGO.Hibernate.Attributes.Mapping;
using AGO.Hibernate.Attributes.Model;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;

namespace AGO.Hibernate.AutoMapping
{
	public class ClassConvention : IClassConvention
	{
		public void Apply(IClassInstance instance)
		{
			var tablePerSubclassAttribute = instance.EntityType.FirstAttribute<TablePerSubclassAttribute>(false);
			if (tablePerSubclassAttribute != null)
			{
				if(!tablePerSubclassAttribute.TableName.IsNullOrWhiteSpace())
					instance.Table(tablePerSubclassAttribute.TableName.Trim());
			}

			var optimisticLockAttribute = instance.EntityType.FirstAttribute<OptimisticLockAttribute>(true);
			if (optimisticLockAttribute == null)
				return;

			if (optimisticLockAttribute.LockType == OptimisticLockType.All)
			{
				instance.OptimisticLock.All();
				instance.DynamicUpdate();
			}
			else if (optimisticLockAttribute.LockType == OptimisticLockType.Dirty)
			{
				instance.OptimisticLock.Dirty();
				instance.DynamicUpdate();
			}
			else if (optimisticLockAttribute.LockType == OptimisticLockType.Version)
			{
				var modelVersionAttribute = instance.EntityType.GetProperties()
					.Where(pi => pi.FirstAttribute<NotMappedAttribute>(true) == null)
					.Select(pi => pi.FirstAttribute<ModelVersionAttribute>(true))
					.FirstOrDefault(v => v != null);
				if (modelVersionAttribute != null)
					instance.OptimisticLock.Version();
			}		
		}
	}
}
