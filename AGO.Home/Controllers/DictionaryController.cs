using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AGO.Core.Attributes.Controllers;
using AGO.Core.Controllers;
using AGO.Core;
using AGO.Core.Filters;
using AGO.Core.Json;
using AGO.Core.Model.Dictionary;
using AGO.Core.Model.Security;
using AGO.Core.Modules.Attributes;
using AGO.Home.Model.Dictionary.Projects;
using AGO.Home.Model.Projects;
using NHibernate.Criterion;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AGO.Home.Controllers
{
	public enum TagsRequestMode
	{
		Personal,
		Common
	}

	public class DictionaryController : AbstractController
	{
		#region Constants

		public const string TagsRequestModeName = "mode";

		#endregion

		#region Properties, fields, constructors

		protected readonly AuthController _AuthController;

		public DictionaryController(
			IJsonService jsonService,
			IFilteringService filteringService,
			IJsonRequestService jsonRequestService,
			ICrudDao crudDao,
			IFilteringDao filteringDao,
			ISessionProvider sessionProvider,
			AuthController authController)
			: base(jsonService, filteringService, jsonRequestService, crudDao, filteringDao, sessionProvider)
		{
			if (authController == null)
				throw new ArgumentNullException("authController");
			_AuthController = authController;
		}

		#endregion

		#region Json endpoints

		[JsonEndpoint, RequireAuthorization]
		public void LookupProjectStatuses(JsonReader input, JsonWriter output)
		{
			var request = _JsonRequestService.ParseModelsRequest(input, DefaultPageSize, MaxPageSize);

			var termProperty = request.Body.Property("term");
			var term = termProperty != null ? termProperty.TokenValue() : null;
			var options = OptionsFromRequest(request);

			var query = _SessionProvider.CurrentSession.QueryOver<ProjectStatusModel>();
			if (!term.IsNullOrWhiteSpace())
				query = query.WhereRestrictionOn(m => m.Name).IsLike(term, MatchMode.Anywhere);

			var result = new List<object>();

			foreach (var model in query.Skip(options.Skip ?? 0).Take(options.Take ?? 1).List())
				result.Add(new {id = model.Id, text = model.Name});

			_JsonService.CreateSerializer().Serialize(output, new {rows = result});
		}

		[JsonEndpoint, RequireAuthorization]
		public void LookupProjectStatusDescriptions(JsonReader input, JsonWriter output)
		{
			var request = _JsonRequestService.ParseModelsRequest(input, DefaultPageSize, MaxPageSize);

			var termProperty = request.Body.Property("term");
			var term = termProperty != null ? termProperty.TokenValue() : null;
			var options = OptionsFromRequest(request);

			var query = _SessionProvider.CurrentSession.QueryOver<ProjectStatusModel>()
				.Select(Projections.Distinct(Projections.Property("Description")));
			if (!term.IsNullOrWhiteSpace())
				query = query.WhereRestrictionOn(m => m.Description).IsLike(term, MatchMode.Anywhere);

			var result = new List<object>();

			foreach (var str in query.Skip(options.Skip ?? 0).Take(options.Take ?? 1).List<string>())
				result.Add(new { id = str, text = str });

			_JsonService.CreateSerializer().Serialize(output, new { rows = result });
		}

		[JsonEndpoint, RequireAuthorization]
		public void GetProjectStatuses(JsonReader input, JsonWriter output)
		{
			var request = _JsonRequestService.ParseModelsRequest(input, DefaultPageSize, MaxPageSize);

			_JsonService.CreateSerializer().Serialize(output, new
			{
				totalRowsCount = _FilteringDao.RowCount<ProjectStatusModel>(request.Filters),
				rows = _FilteringDao.List<ProjectStatusModel>(request.Filters, OptionsFromRequest(request))
			});
		}

		[JsonEndpoint, RequireAuthorization]
		public void GetProjectStatus(JsonReader input, JsonWriter output)
		{
			var request = _JsonRequestService.ParseModelRequest<Guid>(input);

			var filter = new ModelFilterNode { Operator = ModelFilterOperators.And };
			filter.AddItem(new ValueFilterNode
			{
				Path = "Id",
				Operator = ValueFilterOperators.Eq,
				Operand = request.Id.ToStringSafe()
			});

			_JsonService.CreateSerializer().Serialize(output, _FilteringDao.List<ProjectStatusModel>(
				new[] { filter }, OptionsFromRequest(request)).FirstOrDefault());
		}

		[JsonEndpoint, RequireAuthorization]
		public void ProjectStatusMetadata(JsonReader input, JsonWriter output)
		{
			_JsonService.CreateSerializer().Serialize(
				output, MetadataForModelAndRelations<ProjectStatusModel>());
		}

		[JsonEndpoint, RequireAuthorization(true)]
		public void EditProjectStatus(JsonReader input, JsonWriter output)
		{
			var request = _JsonRequestService.ParseRequest(input);
			var validationResult = new ValidationResult();

			try
			{
				var modelProperty = request.Body.Property(ModelName);
				if (modelProperty == null)
					throw new MalformedRequestException();

				var requestModel = _JsonService.CreateSerializer().Deserialize<ProjectStatusModel>(
					new JTokenReader(modelProperty.Value));
				if (requestModel == null)
					throw new MalformedRequestException();

				var model = default(Guid).Equals(requestModel.Id)
					? new ProjectStatusModel { Creator = _AuthController.GetCurrentUser() }
					: _CrudDao.Get<ProjectStatusModel>(requestModel.Id, true);

				var name = requestModel.Name.TrimSafe();
				if (name.IsNullOrEmpty())
					validationResult.FieldErrors["Name"] = new RequiredFieldException().Message;
				if (_SessionProvider.CurrentSession.QueryOver<ProjectStatusModel>()
						.Where(m => m.Name == name && m.Id != requestModel.Id).RowCount() > 0)
					validationResult.FieldErrors["Name"] = new UniqueFieldException().Message;

				if (!validationResult.Success)
					return;

				model.Name = name;
				model.Description = requestModel.Description.TrimSafe();
				_CrudDao.Store(model);
			}
			catch (Exception e)
			{
				validationResult.GeneralError = e.Message;
			}
			finally
			{
				_JsonService.CreateSerializer().Serialize(output, validationResult);
			}
		}

		[JsonEndpoint, RequireAuthorization(true)]
		public void DeleteProjectStatus(JsonReader input, JsonWriter output)
		{
			var request = _JsonRequestService.ParseModelRequest<Guid>(input);

			var model = _CrudDao.Get<ProjectStatusModel>(request.Id, true);
			
			if (_SessionProvider.CurrentSession.QueryOver<ProjectModel>()
					.Where(m => m.Status == model).RowCount() > 0)
				throw new CannotDeleteReferencedItemException();

			if (_SessionProvider.CurrentSession.QueryOver<ProjectStatusHistoryModel>()
					.Where(m => m.Status == model).RowCount() > 0)
				throw new CannotDeleteReferencedItemException();

			_CrudDao.Delete(model);
		}

		[JsonEndpoint, RequireAuthorization]
		public void GetProjectTags(JsonReader input, JsonWriter output)
		{
			var request = _JsonRequestService.ParseModelsRequest(input, DefaultPageSize, MaxPageSize);

			var modeProperty = request.Body.Property(TagsRequestModeName);
			var mode = (modeProperty != null ? modeProperty.TokenValue() : string.Empty)
				.ParseEnumSafe(TagsRequestMode.Personal);

			IModelFilterNode modeFilter = new ModelFilterNode();
			if (mode == TagsRequestMode.Personal)
			{
				modeFilter.AddItem(new ValueFilterNode
				{
					Path = "Owner", 
					Operator = ValueFilterOperators.Eq, 
					Operand = _AuthController.GetCurrentUser().Id.ToString()
				});
			}
			else
			{
				modeFilter.AddItem(new ValueFilterNode
				{
					Path = "Owner",
					Operator = ValueFilterOperators.Exists,
					Negative = true
				});
			}
			request.Filters.Add(modeFilter);

			_JsonService.CreateSerializer().Serialize(output, new
			{
				totalRowsCount = _FilteringDao.RowCount<ProjectTagModel>(request.Filters),
				rows = _FilteringDao.List<ProjectTagModel>(request.Filters, OptionsFromRequest(request))
			});
		}

		[JsonEndpoint, RequireAuthorization]
		public void GetProjectTag(JsonReader input, JsonWriter output)
		{
			var request = _JsonRequestService.ParseModelRequest<Guid>(input);

			var filter = new ModelFilterNode { Operator = ModelFilterOperators.And };
			filter.AddItem(new ValueFilterNode
			{
				Path = "Id",
				Operator = ValueFilterOperators.Eq,
				Operand = request.Id.ToStringSafe()
			});

			_JsonService.CreateSerializer().Serialize(output, _FilteringDao.List<ProjectTagModel>(
				new[] { filter }, OptionsFromRequest(request)).FirstOrDefault());
		}

		[JsonEndpoint, RequireAuthorization]
		public void LookupProjectTags(JsonReader input, JsonWriter output)
		{
			var request = _JsonRequestService.ParseModelsRequest(input, DefaultPageSize, MaxPageSize);

			var termProperty = request.Body.Property("term");
			var term = termProperty != null ? termProperty.TokenValue() : null;
			var options = OptionsFromRequest(request);

			var query = _SessionProvider.CurrentSession.QueryOver<ProjectTagModel>();
			if (!term.IsNullOrWhiteSpace())
				query = query.WhereRestrictionOn(m => m.Name).IsLike(term, MatchMode.Anywhere);

			var result = new List<object>();

			foreach (var model in query.Skip(options.Skip ?? 0).Take(options.Take ?? 1).List())
				result.Add(new { id = model.Id, text = model.Name });

			_JsonService.CreateSerializer().Serialize(output, new { rows = result });
		}

		[JsonEndpoint, RequireAuthorization]
		public void ProjectTagMetadata(JsonReader input, JsonWriter output)
		{
			_JsonService.CreateSerializer().Serialize(
				output, MetadataForModelAndRelations<ProjectTagModel>());
		}

		[JsonEndpoint, RequireAuthorization]
		public void EditProjectTag(JsonReader input, JsonWriter output)
		{
			var request = _JsonRequestService.ParseRequest(input);
			var validationResult = new ValidationResult();

			try
			{
				var modelProperty = request.Body.Property(ModelName);
				if (modelProperty == null)
					throw new MalformedRequestException();

				var requestModel = _JsonService.CreateSerializer().Deserialize<ProjectTagModel>(
					new JTokenReader(modelProperty.Value));
				if (requestModel == null)
					throw new MalformedRequestException();

				var modeProperty = request.Body.Property(TagsRequestModeName);
				var mode = (modeProperty != null ? modeProperty.TokenValue() : string.Empty)
					.ParseEnumSafe(TagsRequestMode.Personal);

				var currentUser = _AuthController.GetCurrentUser();				
				var model = default(Guid).Equals(requestModel.Id) 
					? new ProjectTagModel
						{
							Creator = currentUser,
							Owner = mode == TagsRequestMode.Personal ? currentUser : null
						}	
					: _CrudDao.Get<ProjectTagModel>(requestModel.Id, true);

				if (model.Owner != null && !currentUser.Equals(model.Owner) && currentUser.SystemRole != SystemRole.Administrator)
					throw new AccessForbiddenException();

				var name = requestModel.Name.TrimSafe();
				if (name.IsNullOrEmpty())
					validationResult.FieldErrors["Name"] = new RequiredFieldException().Message;
				if (_SessionProvider.CurrentSession.QueryOver<ProjectTagModel>().Where(
						m => m.Name == name && m.Owner == requestModel.Owner && m.Id != requestModel.Id).RowCount() > 0)
					validationResult.FieldErrors["Name"] = new UniqueFieldException().Message;

				if (!validationResult.Success)
					return;

				model.Name = name;

				var parentsStack = new Stack<TagModel>();

				var current = model as TagModel;
				while (current != null)
				{
					parentsStack.Push(current);
					current = current.Parent;
				}

				var fullName = new StringBuilder();
				while (parentsStack.Count > 0)
				{
					current = parentsStack.Pop();
					if (fullName.Length > 0)
						fullName.Append(" / ");
					fullName.Append(current.Name);
				}
				model.FullName = fullName.ToString();

				_CrudDao.Store(model);
			}
			catch (Exception e)
			{
				validationResult.GeneralError = e.Message;
			}
			finally
			{
				_JsonService.CreateSerializer().Serialize(output, validationResult);
			}
		}

		[JsonEndpoint, RequireAuthorization]
		public void DeleteProjectTag(JsonReader input, JsonWriter output)
		{
			var request = _JsonRequestService.ParseModelRequest<Guid>(input);

			var model = _CrudDao.Get<ProjectTagModel>(request.Id, true);

			var currentUser = _AuthController.GetCurrentUser();
			if (model.Owner != null && !currentUser.Equals(model.Owner) && currentUser.SystemRole != SystemRole.Administrator)
				throw new AccessForbiddenException();

			if (_SessionProvider.CurrentSession.QueryOver<ProjectToTagModel>()
					.Where(m => m.Tag == model).RowCount() > 0)
				throw new CannotDeleteReferencedItemException();

			_CrudDao.Delete(model);
		}

		[JsonEndpoint, RequireAuthorization]
		public void GetProjectTypes(JsonReader input, JsonWriter output)
		{
			var request = _JsonRequestService.ParseModelsRequest(input, DefaultPageSize, MaxPageSize);

			_JsonService.CreateSerializer().Serialize(output, new
			{
				totalRowsCount = _FilteringDao.RowCount<ProjectTypeModel>(request.Filters),
				rows = _FilteringDao.List<ProjectTypeModel>(request.Filters, OptionsFromRequest(request))
			});
		}

		[JsonEndpoint, RequireAuthorization]
		public void GetProjectType(JsonReader input, JsonWriter output)
		{
			var request = _JsonRequestService.ParseModelRequest<Guid>(input);

			var filter = new ModelFilterNode { Operator = ModelFilterOperators.And };
			filter.AddItem(new ValueFilterNode
			{
				Path = "Id",
				Operator = ValueFilterOperators.Eq,
				Operand = request.Id.ToStringSafe()
			});

			_JsonService.CreateSerializer().Serialize(output, _FilteringDao.List<ProjectTypeModel>(
				new[] { filter }, OptionsFromRequest(request)).FirstOrDefault());
		}

		#endregion
	}
}