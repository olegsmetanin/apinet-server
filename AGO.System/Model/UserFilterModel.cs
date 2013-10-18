using System;
using AGO.Core.Model;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Mapping;
using AGO.Core.Attributes.Model;
using AGO.Core.Model.Security;
using Newtonsoft.Json;

namespace AGO.System.Model
{
	[MetadataExclude]
	public class UserFilterModel : CoreModel<Guid>
	{
		#region Persistent

		[NotLonger(64), JsonProperty, NotEmpty]
		public virtual string Name { get; set; }

		[NotLonger(64), JsonProperty, NotEmpty]
		public virtual string GroupName { get; set; }

		[JsonProperty, NotEmpty]
		public virtual string Filter { get; set; }

		[JsonProperty, NotNull]
		public virtual UserModel User { get; set; }
		[ReadOnlyProperty, MetadataExclude]
		public virtual Guid? UserId { get; set; }

		#endregion

		#region Non-persistent

		public override string ToString()
		{
			return Name;
		}

		#endregion
	}
}
