using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace AGO.Hibernate.Model
{
	[JsonObject(MemberSerialization.OptIn), Serializable]
	public abstract class AbstractModel : ICloneable
	{
		public override string ToString()
		{
			return GetType().Name;
		}

		public virtual object Clone()
		{
			return MemberwiseClone();
		}

		public virtual TModel Clone<TModel>(ISet<AbstractModel> modelsToSave)
			where TModel : AbstractModel
		{
			var result = Clone() as TModel;
			if(result == null)
				return null;
			AfterClone(result, modelsToSave);
			return result;
		}

		protected virtual void AfterClone(AbstractModel obj, ISet<AbstractModel> modelsToSave)
		{			
		}
	}
}
