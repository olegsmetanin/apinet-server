using AGO.Tasks.Model.Task;
using AGO.Tasks.Workflow;
using NUnit.Framework;
using NUnit.Framework.Constraints;


namespace AGO.Tasks.Test
{
    [TestFixture]
    public class WorkflowTest
    {
        private IWorkflow<TaskStatus> workflow;

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            workflow = new TaskStatusWorkflow();
        }

        [Test]
        public void FromNewToDoing()
        {
            var next = workflow.Next(TaskStatus.New);

            Assert.That(next, Has.Length.EqualTo(1));
            Assert.That(next[0], Is.EqualTo(TaskStatus.Doing));
        }

        [Test]
        public void FromDoingToDoneOrDiscarded()
        {
            var next = workflow.Next(TaskStatus.Doing);

            Assert.That(next, Has.Length.EqualTo(2));
            Assert.That(next[0], Is.EqualTo(TaskStatus.Done));
            Assert.That(next[1], Is.EqualTo(TaskStatus.Discarded));
        }

        [Test]
        public void FromDoneToClosed()
        {
            var next = workflow.Next(TaskStatus.Done);

            Assert.That(next, Has.Length.EqualTo(1));
            Assert.That(next[0], Is.EqualTo(TaskStatus.Closed));
        }

        [Test]
        public void FromDiscardedToClosed()
        {
            var next = workflow.Next(TaskStatus.Discarded);

            Assert.That(next, Has.Length.EqualTo(1));
            Assert.That(next[0], Is.EqualTo(TaskStatus.Closed));
        }

        [Test]
        public void ClosedIsEnd()
        {
            var next = workflow.Next(TaskStatus.Closed);

            Assert.That(next, Is.Empty);
        }

        [Test]
        public void IsValidTransitionReturnTrueOnlyOnValid()
        {
            ReusableConstraint valid = Is.True;
            ReusableConstraint invalid = Is.False;

            const TaskStatus neyw = TaskStatus.New;
            const TaskStatus doing = TaskStatus.Doing;
            const TaskStatus done = TaskStatus.Done;
            const TaskStatus closed = TaskStatus.Closed;
            const TaskStatus discarded = TaskStatus.Discarded;

            Assert.That(workflow.IsValidTransitions(neyw, doing), valid);
            Assert.That(workflow.IsValidTransitions(doing, done), valid);
            Assert.That(workflow.IsValidTransitions(doing, discarded), valid);
            Assert.That(workflow.IsValidTransitions(done, closed), valid);
            Assert.That(workflow.IsValidTransitions(discarded, closed), valid);

            Assert.That(workflow.IsValidTransitions(neyw, neyw), invalid);
            Assert.That(workflow.IsValidTransitions(neyw, done), invalid);
            Assert.That(workflow.IsValidTransitions(neyw, discarded), invalid);
            Assert.That(workflow.IsValidTransitions(neyw, closed), invalid);

            Assert.That(workflow.IsValidTransitions(doing, doing), invalid);
            Assert.That(workflow.IsValidTransitions(doing, neyw), invalid);
            Assert.That(workflow.IsValidTransitions(doing, closed), invalid);

            Assert.That(workflow.IsValidTransitions(done, done), invalid);
            Assert.That(workflow.IsValidTransitions(done, neyw), invalid);
            Assert.That(workflow.IsValidTransitions(done, doing), invalid);
            Assert.That(workflow.IsValidTransitions(done, discarded), invalid);

            Assert.That(workflow.IsValidTransitions(closed, closed), invalid);
            Assert.That(workflow.IsValidTransitions(closed, neyw), invalid);
            Assert.That(workflow.IsValidTransitions(closed, doing), invalid);
            Assert.That(workflow.IsValidTransitions(closed, discarded), invalid);
            Assert.That(workflow.IsValidTransitions(closed, done), invalid);

            Assert.That(workflow.IsValidTransitions(discarded, discarded), invalid);
            Assert.That(workflow.IsValidTransitions(discarded, neyw), invalid);
            Assert.That(workflow.IsValidTransitions(discarded, doing), invalid);
            Assert.That(workflow.IsValidTransitions(discarded, done), invalid);
        }
    }
}
