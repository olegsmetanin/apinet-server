using AGO.Tasks.Model.Task;
using FluentMigrator;
using AGO.Core.Migration;

namespace AGO.Tasks.Migrations
{
    /// <summary>
    /// Первая миграция - основные таблицы и связи
    /// </summary>
    [MigrationVersion(2013, 09, 16, 01)]
    public class TasksBootstrapMigration: Migration
    {
        public override void Up()
        {
            //Schema created on create VersionInfo table
            //Create.Schema("Tasks");

            //Create.Table("test").InSchema("Tasks").WithColumn("num").AsInt32();

            Create.SecureModelTable<Task>("Tasks")
                .WithValueColumn<Task>(m => m.SeqNumber)
                .WithValueColumn<Task>(m => m.InternalSeqNumber)
                .WithValueColumn<Task>(m => m.Status);

            Create.SecureModelTable<TaskStatusHistoryModel>("Tasks")
                .WithValueColumn<TaskStatusHistoryModel>(m => m.Start)
                .WithValueColumn<TaskStatusHistoryModel>(m => m.Finish)
                .WithRefColumn<TaskStatusHistoryModel>(m => m.Task, true, "Tasks")
                .WithValueColumn<TaskStatusHistoryModel>(m => m.Status);
        }

        public override void Down()
        {
            Delete.ModelTable<TaskStatusHistoryModel>();
            Delete.ModelTable<Task>();
            //Delete.Schema("Tasks");
        }
    }
}