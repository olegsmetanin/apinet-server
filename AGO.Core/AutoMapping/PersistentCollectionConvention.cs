using AGO.Core.Attributes.Mapping;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;

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
			var relatedOneToMany = instance.Relationship as IOneToManyInstance;

			if (!attribute.Column.IsNullOrWhiteSpace())
				instance.Key.Column(attribute.Column.TrimSafe());
			else if (relatedOneToMany != null && relatedOneToMany.ChildType == instance.EntityType)
				instance.Key.Column("ParentId");
			else if (relatedManyToMany == null)
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

			if (AutoMappedSessionFactoryBuilder.DisableSchemas)
				return;

			if (!attribute.LinkSchema.IsNullOrWhiteSpace())
				instance.Schema(QuotedNamesNamingStrategy.DoubleQuote(attribute.LinkSchema.TrimSafe()));
			else
			{
				var parts = instance.EntityType.Assembly.GetName().Name.Split('.');
				instance.Schema(QuotedNamesNamingStrategy.DoubleQuote(parts[parts.Length - 1]));
			}
		}
	}
}
