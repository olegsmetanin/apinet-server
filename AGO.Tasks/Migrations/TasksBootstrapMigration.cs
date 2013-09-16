﻿using AGO.Tasks.Model.Dictionary;
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
    	internal const string MODULE_SCHEMA = "Tasks";

        public override void Up()
        {
            //Schema created on create VersionInfo table
            //Create.Schema("Tasks");

        	Create.SecureModelTable<TaskTypeModel>(MODULE_SCHEMA)
        		.WithValueColumn<TaskTypeModel>(m => m.ProjectCode)
        		.WithValueColumn<TaskTypeModel>(m => m.Name);

			Create.SecureModelTable<CustomTaskStatusModel>(MODULE_SCHEMA)
				.WithValueColumn<CustomTaskStatusModel>(m => m.ProjectCode)
				.WithValueColumn<CustomTaskStatusModel>(m => m.Name)
				.WithValueColumn<CustomTaskStatusModel>(m => m.ViewOrder);

        	Create.SecureModelTable<TaskModel>(MODULE_SCHEMA)
        		.WithValueColumn<TaskModel>(m => m.ProjectCode)
        		.WithValueColumn<TaskModel>(m => m.SeqNumber)
        		.WithValueColumn<TaskModel>(m => m.InternalSeqNumber)
        		.WithValueColumn<TaskModel>(m => m.Status)
        		.WithValueColumn<TaskModel>(m => m.Priority)
        		.WithRefColumn<TaskModel>(m => m.TaskType, true, MODULE_SCHEMA)
        		.WithValueColumn<TaskModel>(m => m.Content)
        		.WithValueColumn<TaskModel>(m => m.Note)
        		.WithValueColumn<TaskModel>(m => m.DueDate)
        		.WithRefColumn<TaskModel>(m => m.CustomStatus, true, MODULE_SCHEMA);

            Create.SecureModelTable<TaskStatusHistoryModel>(MODULE_SCHEMA)
                .WithValueColumn<TaskStatusHistoryModel>(m => m.Start)
                .WithValueColumn<TaskStatusHistoryModel>(m => m.Finish)
                .WithRefColumn<TaskStatusHistoryModel>(m => m.Task, true, MODULE_SCHEMA)
                .WithValueColumn<TaskStatusHistoryModel>(m => m.Status);

        	Create.SecureModelTable<CustomTaskStatusHistoryModel>(MODULE_SCHEMA)
        		.WithValueColumn<CustomTaskStatusHistoryModel>(m => m.Start)
        		.WithValueColumn<CustomTaskStatusHistoryModel>(m => m.Finish)
        		.WithRefColumn<CustomTaskStatusHistoryModel>(m => m.Task, true, MODULE_SCHEMA)
        		.WithRefColumn<CustomTaskStatusHistoryModel>(m => m.Status, true, MODULE_SCHEMA);

        	Create.SecureModelTable<TaskExecutorModel>(MODULE_SCHEMA)
        		.WithRefColumn<TaskExecutorModel>(m => m.Task, true, MODULE_SCHEMA)
        		.WithRefColumn<TaskExecutorModel>(m => m.Executor);
        }

        public override void Down()
        {
            Delete.ModelTable<TaskStatusHistoryModel>();
            Delete.ModelTable<TaskModel>();
            //Delete.Schema("Tasks");
        }
    }
}