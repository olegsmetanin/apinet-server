using System.Linq;
using AGO.Tasks.Controllers;
using AGO.Tasks.Controllers.DTO;
using AGO.Tasks.Model.Task;
using NUnit.Framework;

namespace AGO.Tasks.Test
{
    public class TaskCommentTest: AbstractTest
    {
        private TasksController controller;
        private TaskModel task;

        public override void FixtureSetUp()
        {
            base.FixtureSetUp();

            controller = IocContainer.GetInstance<TasksController>();
        }

        public override void SetUp()
        {
            base.SetUp();

            task = M.Task(1, executor: CurrentUser);
        }

        [Test]
        public void GetCommentsReturnDtoList()
        {
            var c1 = M.Comment(task, "aaa");
            var c2 = M.Comment(task, "bbb");

            var comments = controller.GetComments(task.ProjectCode, task.Id, 0).ToArray();

            Assert.That(comments, Is.Not.Null);
            Assert.That(comments, Has.Length.EqualTo(2));
            Assert.That(comments, Has.Exactly(1).Matches<CommentDTO>(c => c.Id == c1.Id));
            Assert.That(comments, Has.Exactly(1).Matches<CommentDTO>(c => c.Id == c2.Id));
        }

        [Test]
        public void GetCommentsCountReturnNumber()
        {
            M.Comment(task, "aaa");
            M.Comment(task, "bbb");

            var res = controller.GetCommentsCount(task.ProjectCode, task.Id);

            Assert.That(res, Is.EqualTo(2));
        }

        [Test]
        public void CreateCommentReturnDto()
        {
            var dto = controller.CreateComment(task.ProjectCode, task.Id, "aaa");

            Assert.That(dto, Is.Not.Null);
            Assert.That(dto.Author, Is.EqualTo(CurrentUser.FullName));
            Assert.That(dto.Text, Is.EqualTo("aaa"));
        }

        [Test]
        public void CannotCreateCommentInClosedTask()
        {
            task.ChangeStatus(TaskStatus.Closed, M.MemberFromUser(task.ProjectCode, CurrentUser));

            Assert.That(() => controller.CreateComment(task.ProjectCode, task.Id, "aaa"),
                Throws.Exception.TypeOf<CannotAddCommentToClosedTask>());
        }
    }
}