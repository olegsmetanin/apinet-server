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
			var table = instance.EntityType.Name;
			var parts = instance.EntityType.Assembly.GetName().Name.Split('.');
			var schema = parts[parts.Length - 1];

			var tablePerSubclassAttribute = instance.EntityType.FirstAttribute<TablePerSubclassAttribute>(false);
			if (tablePerSubclassAttribute != null && !tablePerSubclassAttribute.TableName.IsNullOrWhiteSpace())
				table = tablePerSubclassAttribute.TableName.TrimSafe();
			if (tablePerSubclassAttribute != null && !tablePerSubclassAttribute.SchemaName.IsNullOrWhiteSpace())
				schema = tablePerSubclassAttribute.SchemaName.TrimSafe();

			var tableAttribute = instance.EntityType.FirstAttribute<TableAttribute>(false);
			if (tableAttribute != null && !tableAttribute.TableName.IsNullOrWhiteSpace())
				table = tableAttribute.TableName.TrimSafe();
			if (tableAttribute != null && !tableAttribute.SchemaName.IsNullOrWhiteSpace())
				schema = tableAttribute.SchemaName.TrimSafe();

			instance.Table(table);
			if(!AutoMappedSessionFactoryBuilder.DisableSchemas)
				instance.Schema(schema);

			var lazyLoadAttribute = instance.EntityType.FirstAttribute<LazyLoadAttribute>(true);
			if (lazyLoadAttribute != null)
				instance.LazyLoad();

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
