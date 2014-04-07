using System;
using System.Collections.Generic;
using System.Linq;
using AGO.Tasks.Model.Task;

namespace AGO.Tasks.Workflow
{
    public class TaskStatusWorkflow: IWorkflow<TaskStatus>
    {
        private readonly IList<Tuple<TaskStatus, TaskStatus>> ribs;

        public TaskStatusWorkflow()
        {
            ribs = new List<Tuple<TaskStatus, TaskStatus>>();

            ribs.Add(new Tuple<TaskStatus, TaskStatus>(TaskStatus.New, TaskStatus.Doing));

            ribs.Add(new Tuple<TaskStatus, TaskStatus>(TaskStatus.Doing, TaskStatus.Done));
            ribs.Add(new Tuple<TaskStatus, TaskStatus>(TaskStatus.Doing, TaskStatus.Discarded));

            ribs.Add(new Tuple<TaskStatus, TaskStatus>(TaskStatus.Done, TaskStatus.Closed));
            ribs.Add(new Tuple<TaskStatus, TaskStatus>(TaskStatus.Discarded, TaskStatus.Closed));
        }

        public TaskStatus[] Next(TaskStatus current)
        {
            return ribs.Where(r => r.Item1 == current).Select(r => r.Item2).ToArray();
        }

        public bool IsValidTransitions(TaskStatus from, TaskStatus to)
        {
            return Next(from).Contains(to);
        }
    }
}