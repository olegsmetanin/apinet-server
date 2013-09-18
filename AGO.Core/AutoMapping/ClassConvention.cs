using System.Linq;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;
using AGO.Core.Attributes.Mapping;
using AGO.Core.Attributes.Model;

namespace AGO.Core.AutoMapping
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
				if (!tablePerSubclassAttribute.SchemaName.IsNullOrWhiteSpace())
					instance.Schema(tablePerSubclassAttribute.SchemaName);
			}
			else
			{
				var tableAttribute = instance.EntityType.FirstAttribute<TableAttribute>(false);
				if (tableAttribute != null)
				{
					if (!tableAttribute.TableName.IsNullOrWhiteSpace())
						instance.Table(tableAttribute.TableName.Trim());
					if (!tableAttribute.SchemaName.IsNullOrWhiteSpace())
						instance.Schema(tableAttribute.SchemaName);
				}
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
