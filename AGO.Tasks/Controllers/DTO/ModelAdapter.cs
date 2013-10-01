using System;
using AGO.Core.Model;
using AGO.Core.Model.Dictionary;
using AGO.Core.Model.Security;

namespace AGO.Tasks.Controllers.DTO
{
	/// <summary>
	/// Базовый класс для сборки DTO из данных моделей
	/// </summary>
	/// <typeparam name="TModel">Тип модели</typeparam>
	/// <typeparam name="TDTO">Тип DTO</typeparam>
	public abstract class ModelAdapter<TModel, TDTO> 
		where TModel: DocstoreModel<Guid>, ISecureModel
		where TDTO: ModelDTO, new()
	{
		public virtual TDTO Fill(TModel model)
		{
			return new TDTO {Id = model.Id, ModelVersion = model.ModelVersion};
		}

		public string ToAuthor(ISecureModel model)
		{
			return model.Creator != null ? model.Creator.FIO : null;
		}
	}

	/// <summary>
	/// Базовый класс для сборки DTO справочников из данных моделей
	/// </summary>
	/// <typeparam name="TModel">Тип модели</typeparam>
	/// <typeparam name="TDTO">Тип DTO</typeparam>
	public abstract class DictionaryModelAdapter<TModel, TDTO> : ModelAdapter<TModel, TDTO> 
		where TModel: DocstoreModel<Guid>, IDictionaryItemModel
		where TDTO: DictionaryDTO, new()
	{
		public override TDTO Fill(TModel model)
		{
			var dto = base.Fill(model);
			dto.Name = model.Name;
			dto.Author = ToAuthor(model);
			dto.CreationTime = model.CreationTime;

			return dto;
		}
	}
}