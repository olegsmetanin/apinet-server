using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;
using AGO.Core.Attributes.Mapping;

namespace AGO.Core.AutoMapping
{
	public class PersistentCollectionConvention : AttributeCollectionConvention<PersistentCollectionAttribute>
	{
		protected override void Apply(PersistentCollectionAttribute attribute, ICollectionInstance instance)
		{
			if (attribute.Inverse)
				instance.Inverse();
			else
				instance.Not.Inverse();

			if (attribute.CascadeType == CascadeType.None)
				instance.Cascade.None();
			if (attribute.CascadeType == CascadeType.Merge)
				instance.Cascade.Merge();
			if (attribute.CascadeType == CascadeType.All)
				instance.Cascade.All();
			if (attribute.CascadeType == CascadeType.SaveUpdate)
				instance.Cascade.SaveUpdate();
			if (attribute.CascadeType == CascadeType.Delete)
				instance.Cascade.Delete();
			if (attribute.CascadeType == CascadeType.AllDeleteOrphan)
				instance.Cascade.AllDeleteOrphan();

			var relatedManyToMany = instance.Relationship as IManyToManyInstance;

			if (relatedManyToMany == null && !attribute.Column.IsNullOrWhiteSpace())
				instance.Key.Column(attribute.Column.TrimSafe());
			else
				instance.Key.Column(instance.EntityType.Name.RemoveSuffix("Model") + "Id");

			if (relatedManyToMany == null)
				return;

			relatedManyToMany.Column(relatedManyToMany.StringIdentifierForModel.RemoveSuffix("Model") + "Id");

			if (!attribute.LinkTable.IsNullOrWhiteSpace())
				instance.Table(attribute.LinkTable.TrimSafe());
			else
			{
				var first = !attribute.Inverse ? instance.EntityType.Name : relatedManyToMany.StringIdentifierForModel;
				var second = !attribute.Inverse ? relatedManyToMany.StringIdentifierForModel : instance.EntityType.Name;
				instance.Table(first + "To" + second);
			}

			if (!attribute.LinkSchema.IsNullOrWhiteSpace())
				instance.Schema(attribute.LinkSchema.TrimSafe());
			else
			{
				var parts = instance.EntityType.Assembly.GetName().Name.Split('.');
				instance.Schema(parts[parts.Length - 1]);
			}
		}
	}
}
