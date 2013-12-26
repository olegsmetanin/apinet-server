using System;
using System.Collections;
using FluentNHibernate;
using FluentNHibernate.Automapping;
using AGO.Core.Attributes.Mapping;
using AGO.Core.Attributes.Model;
using AGO.Core.Model;

namespace AGO.Core.AutoMapping
{
	public class DefaultAutoMappingConfiguration : DefaultAutomappingConfiguration
	{
		public override bool IsId(Member member)
		{
			return member.MemberInfo.FirstAttribute<IdentifierAttribute>(false)!=null;
		}

		public override bool ShouldMap(Type type)
		{
			return type.FirstAttribute<RelationalModelAttribute>(true) != null && 
				type.FirstAttribute<NotMappedAttribute>(true) == null;
		}

		public override bool AbstractClassIsLayerSupertype(Type type)
		{
			return type.FirstAttribute<TablePerSubclassAttribute>(false) == null;
		}

		public override bool ShouldMap(Member member)
		{
			if (typeof(ICollection).IsAssignableFrom(member.PropertyType) &&
				!typeof(byte[]).IsAssignableFrom(member.PropertyType) &&
				member.MemberInfo.FirstAttribute<PersistentCollectionAttribute>(true) == null)
			{
				return false;
			}
			return member.MemberInfo.FirstAttribute<NotMappedAttribute>(true) == null && base.ShouldMap(member);
		}

		public override bool IsComponent(Type type)
		{
			return typeof(IComponent).IsAssignableFrom(type);
		}

		public override string GetComponentColumnPrefix(Member member)
		{
			var attribute = member.MemberInfo.FirstAttribute<ComponentPrefixAttribute>(false);
			return attribute != null ? attribute.Prefix : "";
		}

		public override bool IsDiscriminated(Type type)
		{
			return type.FirstAttribute<TablePerSubclassAttribute>(true) != null;
		}

		public override string GetDiscriminatorColumn(Type type)
		{
			var attribute = type.FirstAttribute<TablePerSubclassAttribute>(true);
			if (attribute == null || attribute.DiscriminatorColumn.IsNullOrEmpty())
				return base.GetDiscriminatorColumn(type);
			return attribute.DiscriminatorColumn;
		}

		public override bool IsVersion(Member member)
		{
			return member.MemberInfo.FirstAttribute<ModelVersionAttribute>(true) != null &&
				member.MemberInfo.FirstAttribute<NotMappedAttribute>(true) == null;
		}
	}
}
