using AGO.Core.Attributes.Constraints;

namespace AGO.Core.Model.Activity
{
	public class AttributeChangeActivityRecordModel : ActivityRecordModel
	{
		#region Persistent

		[NotEmpty, NotLonger(128)]
		public virtual string Attribute { get; set; }

		public virtual string OldValue { get; set; }

		public virtual string NewValue { get; set; }

		#endregion
	}
}
