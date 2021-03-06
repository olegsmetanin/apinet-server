﻿using System;
using AGO.Core.Attributes.Mapping;
using AGO.Core.Attributes.Constraints;
using Newtonsoft.Json;

namespace AGO.Core.Model.Dictionary.Projects
{
	public class ProjectTypeModel : CoreModel<Guid>, IDictionaryItemModel
	{
		#region Persistent

		[JsonProperty, NotLonger(32)]
		public virtual string ProjectCode { get; set; }

		[NotEmpty, NotLonger(64), JsonProperty, UniqueProperty("ProjectCode")]
		public virtual string Name { get; set; }

		[NotLonger(512), JsonProperty]
		public virtual new string Description { get; set; }

		[NotLonger(1024), JsonProperty, NotEmpty, MetadataExclude]
		public virtual string Module { get; set; }

		#endregion

		#region Non-persistent

		public override string ToString()
		{
			return !Name.IsNullOrWhiteSpace() ? Name : base.ToString();
		}

		#endregion
	}
}