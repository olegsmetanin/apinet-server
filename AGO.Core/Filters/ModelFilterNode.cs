using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using AGO.Core.Json;

namespace AGO.Core.Filters
{
	[JsonObject(MemberSerialization.OptIn)]
	public class ModelFilterNode : AbstractFilterNode, IModelFilterNode
	{
		public static IDictionary<ModelFilterOperators, string> OperatorConversionTable = new Dictionary<ModelFilterOperators, string>
		{
			{ ModelFilterOperators.And, "&&" },
			{ ModelFilterOperators.Or, "||" }
		};

		[JsonProperty(PropertyName = "op"), JsonConverter(typeof(ModelFilterOperatorConverter))]
		public ModelFilterOperators? Operator { get; set; }
		
		protected readonly IList<IFilterNode> _Items = new List<IFilterNode>();
		[JsonProperty(PropertyName = "items")]
		public IEnumerable<IFilterNode> Items { get { return _Items; } }

		public void AddItem(IFilterNode node)
		{
			if (node == null)
				throw new ArgumentNullException("node");

			_Items.Add(node);
		}
	}
}