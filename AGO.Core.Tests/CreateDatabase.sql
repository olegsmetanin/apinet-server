USE [master]
GO
IF EXISTS(SELECT name FROM sys.databases WHERE name = 'AGOExamples') BEGIN
	ALTER DATABASE [AGOExamples] SET SINGLE_USER WITH ROLLBACK IMMEDIATE
	DROP DATABASE [AGOExamples]
END
GO

/****** Object:  Database [AGOExamples]    Script Date: 06/10/2013 20:12:44 ******/
CREATE DATABASE [AGOExamples] ON  PRIMARY 
( NAME = N'AGOExamples', FILENAME = N'E:\Sql\Data\AGOExamples.mdf' , SIZE = 3072KB , MAXSIZE = UNLIMITED, FILEGROWTH = 1024KB )
 LOG ON 
( NAME = N'AGOExamples_log', FILENAME = N'E:\Sql\Data\AGOExamples_log.ldf' , SIZE = 1024KB , MAXSIZE = 2048GB , FILEGROWTH = 10%)
GO
ALTER DATABASE [AGOExamples] SET COMPATIBILITY_LEVEL = 100
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [AGOExamples].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [AGOExamples] SET ANSI_NULL_DEFAULT OFF
GO
ALTER DATABASE [AGOExamples] SET ANSI_NULLS OFF
GO
ALTER DATABASE [AGOExamples] SET ANSI_PADDING OFF
GO
ALTER DATABASE [AGOExamples] SET ANSI_WARNINGS OFF
GO
ALTER DATABASE [AGOExamples] SET ARITHABORT OFF
GO
ALTER DATABASE [AGOExamples] SET AUTO_CLOSE OFF
GO
ALTER DATABASE [AGOExamples] SET AUTO_CREATE_STATISTICS ON
GO
ALTER DATABASE [AGOExamples] SET AUTO_SHRINK OFF
GO
ALTER DATABASE [AGOExamples] SET AUTO_UPDATE_STATISTICS ON
GO
ALTER DATABASE [AGOExamples] SET CURSOR_CLOSE_ON_COMMIT OFF
GO
ALTER DATABASE [AGOExamples] SET CURSOR_DEFAULT  GLOBAL
GO
ALTER DATABASE [AGOExamples] SET CONCAT_NULL_YIELDS_NULL OFF
GO
ALTER DATABASE [AGOExamples] SET NUMERIC_ROUNDABORT OFF
GO
ALTER DATABASE [AGOExamples] SET QUOTED_IDENTIFIER OFF
GO
ALTER DATABASE [AGOExamples] SET RECURSIVE_TRIGGERS OFF
GO
ALTER DATABASE [AGOExamples] SET  DISABLE_BROKER
GO
ALTER DATABASE [AGOExamples] SET AUTO_UPDATE_STATISTICS_ASYNC OFF
GO
ALTER DATABASE [AGOExamples] SET DATE_CORRELATION_OPTIMIZATION OFF
GO
ALTER DATABASE [AGOExamples] SET TRUSTWORTHY OFF
GO
ALTER DATABASE [AGOExamples] SET ALLOW_SNAPSHOT_ISOLATION OFF
GO
ALTER DATABASE [AGOExamples] SET PARAMETERIZATION SIMPLE
GO
ALTER DATABASE [AGOExamples] SET READ_COMMITTED_SNAPSHOT OFF
GO
ALTER DATABASE [AGOExamples] SET HONOR_BROKER_PRIORITY OFF
GO
ALTER DATABASE [AGOExamples] SET  READ_WRITE
GO
ALTER DATABASE [AGOExamples] SET RECOVERY FULL
GO
ALTER DATABASE [AGOExamples] SET  MULTI_USER
GO
ALTER DATABASE [AGOExamples] SET PAGE_VERIFY CHECKSUM
GO
ALTER DATABASE [AGOExamples] SET DB_CHAINING OFF
GO
EXEC sys.sp_db_vardecimal_storage_format N'AGOExamples', N'ON'
GO
USE [AGOExamples]
GO
/****** Object:  Table [dbo].[ManyToMany1Model]    Script Date: 06/10/2013 20:12:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ManyToMany1Model](
	[Id] [uniqueidentifier] NOT NULL,
	[ModelVersion] [int] NULL,
	[CreationTime] [datetime] NULL,
	[Name] [nvarchar](64) NOT NULL,
 CONSTRAINT [PK_ManyToMany1Model] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ManyEndModel]    Script Date: 06/10/2013 20:12:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ManyEndModel](
	[Id] [uniqueidentifier] NOT NULL,
	[ModelVersion] [int] NULL,
	[CreationTime] [datetime] NULL,
	[Name] [nvarchar](64) NOT NULL,
 CONSTRAINT [PK_ManyEndModel] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ManyToMany2Model]    Script Date: 06/10/2013 20:12:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ManyToMany2Model](
	[Id] [uniqueidentifier] NOT NULL,
	[ModelVersion] [int] NULL,
	[CreationTime] [datetime] NULL,
	[Name] [nvarchar](64) NOT NULL,
 CONSTRAINT [PK_ManyToMany2Model] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PrimitiveModel]    Script Date: 06/10/2013 20:12:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PrimitiveModel](
	[Id] [uniqueidentifier] NOT NULL,
	[ModelVersion] [int] NULL,
	[CreationTime] [datetime] NULL,
	[ManyEndId] [uniqueidentifier] NULL,
	[StringProperty] [nvarchar](64) NULL,
	[GuidProperty] [uniqueidentifier] NULL,
	[DateTimeProperty] [datetime] NULL,
	[EnumProperty] [nvarchar](64) NULL,
	[BoolProperty] [bit] NULL,
	[ByteProperty] [tinyint] NULL,
	[CharProperty] [nchar](1) NULL,
	[DecimalProperty] [decimal](19, 5) NULL,
	[DoubleProperty] [float] NULL,
	[FloatProperty] [real] NULL,
	[IntProperty] [int] NULL,
	[LongProperty] [bigint] NULL,
	[NullableGuidProperty] [uniqueidentifier] NULL,
	[NullableDateTimeProperty] [datetime] NULL,
	[NullableEnumProperty] [nvarchar](64) NULL,
	[NullableBoolProperty] [bit] NULL,
	[NullableByteProperty] [tinyint] NULL,
	[NullableCharProperty] [nchar](1) NULL,
	[NullableDecimalProperty] [decimal](19, 5) NULL,
	[NullableDoubleProperty] [float] NULL,
	[NullableFloatProperty] [real] NULL,
	[NullableIntProperty] [int] NULL,
	[NullableLongProperty] [bigint] NULL,
 CONSTRAINT [PK_PrimitiveModel] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[OneEndModel]    Script Date: 06/10/2013 20:12:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OneEndModel](
	[Id] [uniqueidentifier] NOT NULL,
	[ModelVersion] [int] NULL,
	[CreationTime] [datetime] NULL,
	[Name] [nvarchar](64) NOT NULL,
	[ManyEndId] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_OneEndModel] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ManyToMany1ModelToManyToMany2Model]    Script Date: 06/10/2013 20:12:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ManyToMany1ModelToManyToMany2Model](
	[ManyToMany1Id] [uniqueidentifier] NOT NULL,
	[ManyToMany2Id] [uniqueidentifier] NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[HierarchicalModel]    Script Date: 06/10/2013 20:12:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[HierarchicalModel](
	[Id] [uniqueidentifier] NOT NULL,
	[ModelVersion] [int] NULL,
	[CreationTime] [datetime] NULL,
	[Name] [nvarchar](64) NOT NULL,
	[ParentId] [uniqueidentifier] NULL,
	[ManyEndId] [uniqueidentifier] NULL,
 CONSTRAINT [PK_HierarchicalModel] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  ForeignKey [FK_PrimitiveModel_ManyEndId]    Script Date: 06/10/2013 20:12:45 ******/
ALTER TABLE [dbo].[PrimitiveModel]  WITH CHECK ADD  CONSTRAINT [FK_PrimitiveModel_ManyEndId] FOREIGN KEY([ManyEndId])
REFERENCES [dbo].[ManyEndModel] ([Id])
GO
ALTER TABLE [dbo].[PrimitiveModel] CHECK CONSTRAINT [FK_PrimitiveModel_ManyEndId]
GO
/****** Object:  ForeignKey [FK_OneEndModel_ManyEndId]    Script Date: 06/10/2013 20:12:45 ******/
ALTER TABLE [dbo].[OneEndModel]  WITH CHECK ADD  CONSTRAINT [FK_OneEndModel_ManyEndId] FOREIGN KEY([ManyEndId])
REFERENCES [dbo].[ManyEndModel] ([Id])
GO
ALTER TABLE [dbo].[OneEndModel] CHECK CONSTRAINT [FK_OneEndModel_ManyEndId]
GO
/****** Object:  ForeignKey [FK_ManyToMany1ModelToManyToMany2Model_ManyToMany1Id_ManyToMany1Model_Id]    Script Date: 06/10/2013 20:12:45 ******/
ALTER TABLE [dbo].[ManyToMany1ModelToManyToMany2Model]  WITH CHECK ADD  CONSTRAINT [FK_ManyToMany1ModelToManyToMany2Model_ManyToMany1Id_ManyToMany1Model_Id] FOREIGN KEY([ManyToMany1Id])
REFERENCES [dbo].[ManyToMany1Model] ([Id])
GO
ALTER TABLE [dbo].[ManyToMany1ModelToManyToMany2Model] CHECK CONSTRAINT [FK_ManyToMany1ModelToManyToMany2Model_ManyToMany1Id_ManyToMany1Model_Id]
GO
/****** Object:  ForeignKey [FK_ManyToMany1ModelToManyToMany2Model_ManyToMany2Id_ManyToMany2Model_Id]    Script Date: 06/10/2013 20:12:45 ******/
ALTER TABLE [dbo].[ManyToMany1ModelToManyToMany2Model]  WITH CHECK ADD  CONSTRAINT [FK_ManyToMany1ModelToManyToMany2Model_ManyToMany2Id_ManyToMany2Model_Id] FOREIGN KEY([ManyToMany2Id])
REFERENCES [dbo].[ManyToMany2Model] ([Id])
GO
ALTER TABLE [dbo].[ManyToMany1ModelToManyToMany2Model] CHECK CONSTRAINT [FK_ManyToMany1ModelToManyToMany2Model_ManyToMany2Id_ManyToMany2Model_Id]
GO
/****** Object:  ForeignKey [FK_HierarchicalModel_ManyEndId]    Script Date: 06/10/2013 20:12:45 ******/
ALTER TABLE [dbo].[HierarchicalModel]  WITH CHECK ADD  CONSTRAINT [FK_HierarchicalModel_ManyEndId] FOREIGN KEY([ManyEndId])
REFERENCES [dbo].[ManyEndModel] ([Id])
GO
ALTER TABLE [dbo].[HierarchicalModel] CHECK CONSTRAINT [FK_HierarchicalModel_ManyEndId]
GO
/****** Object:  ForeignKey [FK_HierarchicalModel_ParentId]    Script Date: 06/10/2013 20:12:45 ******/
ALTER TABLE [dbo].[HierarchicalModel]  WITH CHECK ADD  CONSTRAINT [FK_HierarchicalModel_ParentId] FOREIGN KEY([ParentId])
REFERENCES [dbo].[HierarchicalModel] ([Id])
GO
ALTER TABLE [dbo].[HierarchicalModel] CHECK CONSTRAINT [FK_HierarchicalModel_ParentId]
GO
