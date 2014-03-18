using AGO.Core.Model.Projects;

namespace AGO.Core.Model.Processing
{
	public interface IModelPostProcessor
	{
		bool Accepts(IIdentifiedModel model);

		void AfterModelCreated(IIdentifiedModel model, ProjectMemberModel creator = null);

		void AfterModelUpdated(IIdentifiedModel model, IIdentifiedModel original, ProjectMemberModel changer = null);

		void AfterModelDeleted(IIdentifiedModel model, ProjectMemberModel deleter = null);
	}
}
