using System;
using System.Collections.Generic;
using AGO.Core.Attributes.Model;

namespace AGO.Core.Model
{
	public abstract class IdentifiedModel : AbstractModel, IIdentifiedModel
	{
		#region Non-persistent

		private string _UniqueId;
		[NotMapped]
		public virtual string UniqueId
		{
			get
			{
				if (IsNew())
					_UniqueId = _UniqueId ?? "new" + GetType().FullName + "_" + Guid.NewGuid();
				return _UniqueId ?? string.Empty;
			}
			set { _UniqueId = value; }
		}

		[NotMapped]
		public virtual DateTime? CreationTime { get; set; }

		[NotMapped]
		public virtual string Description { get; set; }

		[NotMapped]
		public virtual Type RealType { get { return GetType(); } }

		public override bool Equals(object obj)
		{
			if (obj == null)
				return false;
			var otherModel = obj as IIdentifiedModel;
			if (otherModel == null)
				return false;
			return otherModel.RealType.IsAssignableFrom(RealType) &&
				UniqueId.Equals(otherModel.UniqueId);
		}

		public override int GetHashCode()
		{
			return UniqueId.GetHashCode();
		}

		public virtual int CompareTo(object obj)
		{
			return CompareTo(obj as IIdentifiedModel);
		}

		public virtual int CompareTo(IIdentifiedModel other)
		{
			return other != null ? String.Compare(UniqueId, other.UniqueId, StringComparison.Ordinal) : 0;
		}

		public virtual bool IsNew()
		{
			return false;
		}

		public override string ToString()
		{
			return string.Format("{0}: {1}", GetType().Name, UniqueId);
		}

		protected override void AfterClone(AbstractModel obj, ISet<AbstractModel> modelsToSave)
		{
			base.AfterClone(obj, modelsToSave);
			var model = (IdentifiedModel)obj;

			model.UniqueId = null;
			model.CreationTime = DateTime.UtcNow;
		}

		protected IdentifiedModel()
		{
			CreationTime = DateTime.UtcNow;
		} 

		#endregion
	}

	public abstract class IdentifiedModel<TIdType> : IdentifiedModel, IIdentifiedModel<TIdType>
	{
		#region Persistent

		[Identifier]
		public virtual TIdType Id { get; set; }

		#endregion

		#region Non-persistent

		[NotMapped]
		public override string UniqueId
		{
			get { return !IsNew() ? Id.ToString() : base.UniqueId; }
			set { base.UniqueId = value; Id = value.ConvertSafe<TIdType>(); }
		}

		public override bool IsNew()
		{
			return Equals(default(TIdType), Id);
		}

		public override bool Equals(object obj)
		{
			var other = obj as IIdentifiedModel<TIdType>;

			if (IsNew() || (other != null && other.IsNew()))
				return base.Equals(obj);
			
			return other != null && Equals(Id, other.Id);
		}

		public override int GetHashCode()
		{
			if (IsNew())
				return base.GetHashCode();
			return Id.GetHashCode();
		}

		#endregion
	}
}
