using System;

namespace AGO.Core.Controllers
{
	public class PropChangeDTO
	{
		public PropChangeDTO()
		{
		}

		public PropChangeDTO(Guid id, int? version, string prop, object value = null)
		{
			Id = id;
			ModelVersion = version;
			Prop = prop;
			Value = value;
		}

		public Guid Id { get; set; }

		public int? ModelVersion { get; set; }

		public string Prop { get; set; }

		public object Value { get; set; }
	}
}
