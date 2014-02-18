using AGO.Core.Model;

namespace AGO.Core.Security
{
	/// <summary>
	/// Error when user does not have permissions for change model instance
	/// </summary>
	public abstract class OperationDeniedException: AbstractApplicationException
	{
		protected OperationDeniedException(IIdentifiedModel model)
			: base(string.Format("Недостаточно прав для выполнения операции над моделью {0}", model))
		{
// ReSharper disable DoNotCallOverridableMethodsInConstructor
			Data["ModelType"] = model.GetType().AssemblyQualifiedName;
			Data["ModelId"] = model.UniqueId;
// ReSharper restore DoNotCallOverridableMethodsInConstructor
		}
	}

	public class CreationDeniedException : OperationDeniedException
	{
		public CreationDeniedException(IIdentifiedModel model) : base(model)
		{
		}
	}

	public class ChangeDeniedException : OperationDeniedException
	{
		public ChangeDeniedException(IIdentifiedModel model) : base(model)
		{
		}
	}

	public class DeleteDeniedException : OperationDeniedException
	{
		public DeleteDeniedException(IIdentifiedModel model) : base(model)
		{
		}
	}
}