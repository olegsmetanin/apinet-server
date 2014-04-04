using System;
using AGO.Tasks.Model.Task;

namespace AGO.Tasks.Controllers.DTO
{
    /// <summary>
    /// Adapter for task comment dto
    /// </summary>
    public class CommentAdapter: ModelAdapter<TaskCommentModel, CommentDTO>
    {
        public override CommentDTO Fill(TaskCommentModel model)
        {
            var dto = base.Fill(model);
            dto.Author = ToAuthor(model);
            dto.CreationTime = model.CreationTime.GetValueOrDefault(DateTime.MinValue);
            dto.Text = model.Text;
            return dto;
        }
    }
}
