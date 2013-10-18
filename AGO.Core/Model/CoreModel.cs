using System;
using System.Collections.Generic;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Model;
using Newtonsoft.Json;

namespace AGO.Core.Model
{
	[RelationalModel]
	public abstract class CoreModel : IdentifiedModel, ICoreModel
	{
		#region Persistent

		[ModelVersion, JsonProperty]
		public virtual int? ModelVersion { get; set; }

		[NotNull, Timestamp, JsonProperty]
		public override DateTime? CreationTime { get; set; }

		#endregion

		#region Non-persistent

		public override bool IsNew()
		{
			return ModelVersion == null || ModelVersion.Value == 0;
		}

		protected override void AfterClone(AbstractModel obj, ISet<AbstractModel> modelsToSave)
		{
			base.AfterClone(obj, modelsToSave);
			var model = (CoreModel) obj;

			model.ModelVersion = null;
		}

		[NotMapped, JsonProperty("Id")]
		public override string UniqueId
		{
			get { return base.UniqueId; }
			set { base.UniqueId = value; }
		}

		[NotMapped, JsonProperty]
		public override string Description { get { return ToString(); } set { } }

		#endregion
	}

	[RelationalModel]
	public abstract class CoreModel<TIdType> : IdentifiedModel<TIdType>, ICoreModel
	{
		#region Persistent

		[ModelVersion, JsonProperty]
		public virtual int? ModelVersion { get; set; }

		[NotNull, Timestamp, JsonProperty]
		public override DateTime? CreationTime { get; set; }

		#endregion

		#region Non-persistent

		public override bool IsNew()
		{
			return ModelVersion == null || ModelVersion.Value == 0;
		}

		protected override void AfterClone(AbstractModel obj, ISet<AbstractModel> modelsToSave)
		{
			base.AfterClone(obj, modelsToSave);
			var model = (CoreModel<TIdType>)obj;

			model.ModelVersion = null;
		}

		[NotMapped, JsonProperty("Id")]
		public override string UniqueId
		{
			get { return !IsNew() ? Id.ToString() : base.UniqueId; }
			set { base.UniqueId = value; Id = value.ConvertSafe<TIdType>(); }
		}

		[NotMapped, JsonProperty]
		public override string Description { get { return ToString(); } set { } }

		#endregion
	}
}
