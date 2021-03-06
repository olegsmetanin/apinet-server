﻿using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;
using AGO.Core.Attributes.Mapping;
using AGO.Core.Attributes.Model;

namespace AGO.Core.AutoMapping
{
	public class ReferenceConvention : IReferenceConvention
	{
		public void Apply(IManyToOneInstance instance)
		{
			instance.Column(instance.Property.MemberInfo.Name + "Id");

			var prefetchedAttribute = instance.Property.MemberInfo.FirstAttribute<PrefetchedAttribute>(true);
			if (prefetchedAttribute != null)
				instance.Fetch.Join();
			var readOnly = instance.Property.MemberInfo.FirstAttribute<ReadOnlyPropertyAttribute>(true);
			if (readOnly != null)
				instance.ReadOnly();

			var manyToOneAttribute = instance.Property.MemberInfo.FirstAttribute<ManyToOneAttribute>(true);
			if (manyToOneAttribute == null)
				return;

			if (manyToOneAttribute.CascadeType == CascadeType.None)
				instance.Cascade.None();
			if (manyToOneAttribute.CascadeType == CascadeType.Merge)
				instance.Cascade.Merge();
			if (manyToOneAttribute.CascadeType == CascadeType.All)
				instance.Cascade.All();
			if (manyToOneAttribute.CascadeType == CascadeType.SaveUpdate)
				instance.Cascade.SaveUpdate();
			if (manyToOneAttribute.CascadeType == CascadeType.Delete)
				instance.Cascade.Delete();
		}
	}
}
