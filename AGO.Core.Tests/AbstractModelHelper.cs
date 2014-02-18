using System;
using System.Collections.Generic;
using AGO.Core.Model;
using AGO.Core.Model.Security;
using NHibernate;

namespace AGO.Core.Tests
{
	public abstract class AbstractModelHelper
	{
		protected readonly Func<ISession> Session;
		protected readonly Func<UserModel> CurrentUser;
		private readonly Stack<Tuple<Type, Guid>> createdModels;

		public AbstractModelHelper(Func<ISession> session, Func<UserModel> currentUser)
		{
			Session = session;
			CurrentUser = currentUser;
			createdModels = new Stack<Tuple<Type, Guid>>();
		}

		public T Track<T>(Func<T> factory) where T : IIdentifiedModel<Guid>
		{
			var model = factory();
			createdModels.Push(new Tuple<Type, Guid>(model.GetType(), model.Id));
			return model;
		}

		public void DropCreated()
		{
			while (createdModels.Count > 0)
			{
				var info = createdModels.Pop();
				var model = Session().Get(info.Item1, info.Item2);
				if (model != null)
					InternalDelete(model);
			}
			Session().Flush();
		}

		protected virtual void InternalDelete(object model)
		{
			Session().Delete(model);
		}
	}
}