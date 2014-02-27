using AGO.Core.Migration;
using AGO.Tasks.Model.Dictionary;
using AGO.Tasks.Model.Task;
using FluentMigrator;

namespace AGO.Tasks.Migrations
{
    /// <summary>
    /// Первая миграция - основные таблицы и связи
    /// </summary>
    [MigrationVersion(2013, 09, 16, 01)]
    public class TasksBootstrapMigration: Migration
    {
    	internal const string MODULE_SCHEMA = "Tasks";

        public override void Up()
        {
        	Create.SecureModelTable<TaskTypeModel>()
        		.WithValueColumn<TaskTypeModel>(m => m.ProjectCode)
        		.WithValueColumn<TaskTypeModel>(m => m.Name);

        	Create.SecureModelTable<TaskModel>()
        		.WithValueColumn<TaskModel>(m => m.ProjectCode)
        		.WithValueColumn<TaskModel>(m => m.SeqNumber)
        		.WithValueColumn<TaskModel>(m => m.InternalSeqNumber)
        		.WithValueColumn<TaskModel>(m => m.Status)
        		.WithValueColumn<TaskModel>(m => m.Priority)
        		.WithRefColumn<TaskModel>(m => m.TaskType)
        		.WithValueColumn<TaskModel>(m => m.Content)
        		.WithValueColumn<TaskModel>(m => m.Note)
        		.WithValueColumn<TaskModel>(m => m.DueDate)
				.WithValueColumn<TaskModel>(m => m.EstimatedTime);

            Create.SecureModelTable<TaskStatusHistoryModel>()
                .WithValueColumn<TaskStatusHistoryModel>(m => m.Start)
                .WithValueColumn<TaskStatusHistoryModel>(m => m.Finish)
                .WithRefColumn<TaskStatusHistoryModel>(m => m.Task)
                .WithValueColumn<TaskStatusHistoryModel>(m => m.Status);

        	Create.SecureModelTable<TaskExecutorModel>()
        		.WithRefColumn<TaskExecutorModel>(m => m.Task)
        		.WithRefColumn<TaskExecutorModel>(m => m.Executor);

        	Create.SecureModelTable<TaskAgreementModel>()
        		.WithRefColumn<TaskAgreementModel>(m => m.Task)
        		.WithRefColumn<TaskAgreementModel>(m => m.Agreemer)
        		.WithValueColumn<TaskAgreementModel>(m => m.DueDate)
        		.WithValueColumn<TaskAgreementModel>(m => m.AgreedAt)
        		.WithValueColumn<TaskAgreementModel>(m => m.Done)
        		.WithValueColumn<TaskAgreementModel>(m => m.Comment);

        	Create.SecureModelTable<TaskToTagModel>()
        		.WithRefColumn<TaskToTagModel>(m => m.Task)
        		.WithRefColumn<TaskToTagModel>(m => m.Tag);

			Alter.ModelTable<TaskCustomPropertyModel>()
				 .AddRefColumn<TaskCustomPropertyModel>(m => m.Task);

	        Create.SecureModelTable<TaskFileModel>()
		        .WithValueColumn<TaskFileModel>(m => m.Name)
		        .WithValueColumn<TaskFileModel>(m => m.ContentType)
		        .WithValueColumn<TaskFileModel>(m => m.Size)
		        .WithValueColumn<TaskFileModel>(m => m.Path)
		        .WithValueColumn<TaskFileModel>(m => m.Uploaded)
		        .WithRefColumn<TaskFileModel>(m => m.Owner);
        }

        public override void Down()
        {
			Delete.ModelTable<TaskToTagModel>();
            Delete.ModelTable<TaskStatusHistoryModel>();
			Delete.ModelTable<TaskExecutorModel>();
			Delete.ModelTable<TaskAgreementModel>();
            Delete.ModelTable<TaskModel>();
			Delete.ModelTable<TaskTypeModel>();

        	Delete.Column<TaskCustomPropertyModel>(m => m.Task);
        }
    }
}