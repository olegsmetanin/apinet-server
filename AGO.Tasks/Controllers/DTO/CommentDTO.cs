using System;

namespace AGO.Tasks.Controllers.DTO
{
    /// <summary>
    /// Task comment DTO
    /// </summary>
    public class CommentDTO: ModelDTO
    {
        public string Author { get; set; }

        public DateTime CreationTime { get; set; }

        public string Text { get; set; }
    }
}
