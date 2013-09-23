USE [AGOExamples]
GO
CREATE SCHEMA [Core]
GO
/****** Object:  Table [Core].[ManyToMany1Model]    Script Date: 06/10/2013 20:12:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [Core].[ManyToMany1Model](
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
/****** Object:  Table [Core].[ManyEndModel]    Script Date: 06/10/2013 20:12:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [Core].[ManyEndModel](
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
/****** Object:  Table [Core].[ManyToMany2Model]    Script Date: 06/10/2013 20:12:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [Core].[ManyToMany2Model](
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
/****** Object:  Table [Core].[PrimitiveModel]    Script Date: 06/10/2013 20:12:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [Core].[PrimitiveModel](
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
/****** Object:  Table [Core].[OneEndModel]    Script Date: 06/10/2013 20:12:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [Core].[OneEndModel](
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
/****** Object:  Table [Core].[ManyToMany1ModelToManyToMany2Model]    Script Date: 06/10/2013 20:12:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [Core].[ManyToMany1ModelToManyToMany2Model](
	[ManyToMany1Id] [uniqueidentifier] NOT NULL,
	[ManyToMany2Id] [uniqueidentifier] NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [Core].[HierarchicalModel]    Script Date: 06/10/2013 20:12:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [Core].[HierarchicalModel](
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
ALTER TABLE [Core].[PrimitiveModel]  WITH CHECK ADD  CONSTRAINT [FK_PrimitiveModel_ManyEndId] FOREIGN KEY([ManyEndId])
REFERENCES [Core].[ManyEndModel] ([Id])
GO
ALTER TABLE [Core].[PrimitiveModel] CHECK CONSTRAINT [FK_PrimitiveModel_ManyEndId]
GO
/****** Object:  ForeignKey [FK_OneEndModel_ManyEndId]    Script Date: 06/10/2013 20:12:45 ******/
ALTER TABLE [Core].[OneEndModel]  WITH CHECK ADD  CONSTRAINT [FK_OneEndModel_ManyEndId] FOREIGN KEY([ManyEndId])
REFERENCES [Core].[ManyEndModel] ([Id])
GO
ALTER TABLE [Core].[OneEndModel] CHECK CONSTRAINT [FK_OneEndModel_ManyEndId]
GO
/****** Object:  ForeignKey [FK_ManyToMany1ModelToManyToMany2Model_ManyToMany1Id_ManyToMany1Model_Id]    Script Date: 06/10/2013 20:12:45 ******/
ALTER TABLE [Core].[ManyToMany1ModelToManyToMany2Model]  WITH CHECK ADD  CONSTRAINT [FK_ManyToMany1ModelToManyToMany2Model_ManyToMany1Id_ManyToMany1Model_Id] FOREIGN KEY([ManyToMany1Id])
REFERENCES [Core].[ManyToMany1Model] ([Id])
GO
ALTER TABLE [Core].[ManyToMany1ModelToManyToMany2Model] CHECK CONSTRAINT [FK_ManyToMany1ModelToManyToMany2Model_ManyToMany1Id_ManyToMany1Model_Id]
GO
/****** Object:  ForeignKey [FK_ManyToMany1ModelToManyToMany2Model_ManyToMany2Id_ManyToMany2Model_Id]    Script Date: 06/10/2013 20:12:45 ******/
ALTER TABLE [Core].[ManyToMany1ModelToManyToMany2Model]  WITH CHECK ADD  CONSTRAINT [FK_ManyToMany1ModelToManyToMany2Model_ManyToMany2Id_ManyToMany2Model_Id] FOREIGN KEY([ManyToMany2Id])
REFERENCES [Core].[ManyToMany2Model] ([Id])
GO
ALTER TABLE [Core].[ManyToMany1ModelToManyToMany2Model] CHECK CONSTRAINT [FK_ManyToMany1ModelToManyToMany2Model_ManyToMany2Id_ManyToMany2Model_Id]
GO
/****** Object:  ForeignKey [FK_HierarchicalModel_ManyEndId]    Script Date: 06/10/2013 20:12:45 ******/
ALTER TABLE [Core].[HierarchicalModel]  WITH CHECK ADD  CONSTRAINT [FK_HierarchicalModel_ManyEndId] FOREIGN KEY([ManyEndId])
REFERENCES [Core].[ManyEndModel] ([Id])
GO
ALTER TABLE [Core].[HierarchicalModel] CHECK CONSTRAINT [FK_HierarchicalModel_ManyEndId]
GO
/****** Object:  ForeignKey [FK_HierarchicalModel_ParentId]    Script Date: 06/10/2013 20:12:45 ******/
ALTER TABLE [Core].[HierarchicalModel]  WITH CHECK ADD  CONSTRAINT [FK_HierarchicalModel_ParentId] FOREIGN KEY([ParentId])
REFERENCES [Core].[HierarchicalModel] ([Id])
GO
ALTER TABLE [Core].[HierarchicalModel] CHECK CONSTRAINT [FK_HierarchicalModel_ParentId]
GO
