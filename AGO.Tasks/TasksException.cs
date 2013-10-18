using System;
using AGO.Core;

namespace AGO.Tasks
{
	public abstract class TasksException: AbstractApplicationException
	{
		protected TasksException(string message = null, Exception innerException = null)
			: base(message, innerException)
		{
		}		 
	}

	public class TaskCreationNotSupportedException: TasksException
	{
	}

	public class CanNotAddAgreemerToClosedTaskException: TasksException
	{
	}

	public class CanNotRemoveAgreemerFromClosedTaskException: TasksException
	{
	}

	public class AgreemerAlreadyAssignedToTaskException: TasksException
	{
		public AgreemerAlreadyAssignedToTaskException(string agreemer, string task)
		{
			_MessageArguments.Add(agreemer);
			_MessageArguments.Add(task);
		}
	}

	public class CurrentUserIsNotAgreemerInTaskException: TasksException
	{
	}

	public class CanNotAgreeClosedTaskException: TasksException
	{
	}

	public class CanNotRevokeAgreementFromClosedTaskException : TasksException
	{
	}

	public class CanNotReplaceWithItemThatWillBeDeletedTo: TasksException
	{
	}
}