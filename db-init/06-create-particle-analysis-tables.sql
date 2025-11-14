-- Create particle analysis tables
USE LabResultsDb;
GO

-- Create ParticleTypeDefinition table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='ParticleTypeDefinition' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[ParticleTypeDefinition](
        [ID] [int] IDENTITY(1,1) NOT NULL,
        [Type] [nvarchar](50) NOT NULL,
        [Description] [nvarchar](500) NOT NULL,
        [Image1] [nvarchar](50) NOT NULL,
        [Image2] [nvarchar](50) NOT NULL,
        [Active] [nvarchar](1) NULL,
        [SortOrder] [int] NULL,
        CONSTRAINT [PK_ParticleTypeDefinition] PRIMARY KEY CLUSTERED ([ID] ASC)
    );
    PRINT 'Created ParticleTypeDefinition table.';
END
ELSE
BEGIN
    PRINT 'ParticleTypeDefinition table already exists.';
END
GO

-- Create ParticleSubTypeCategoryDefinition table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='ParticleSubTypeCategoryDefinition' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[ParticleSubTypeCategoryDefinition](
        [ID] [int] IDENTITY(1,1) NOT NULL,
        [Description] [nvarchar](50) NOT NULL,
        [Active] [nvarchar](1) NULL,
        [SortOrder] [int] NULL,
        CONSTRAINT [PK_ParticleSubTypeCategoryDefinition] PRIMARY KEY CLUSTERED ([ID] ASC)
    );
    PRINT 'Created ParticleSubTypeCategoryDefinition table.';
END
ELSE
BEGIN
    PRINT 'ParticleSubTypeCategoryDefinition table already exists.';
END
GO

-- Create ParticleSubTypeDefinition table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='ParticleSubTypeDefinition' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[ParticleSubTypeDefinition](
        [ParticleSubTypeCategoryID] [int] NOT NULL,
        [Value] [int] NOT NULL,
        [Description] [nvarchar](50) NOT NULL,
        [Active] [nvarchar](1) NULL,
        [SortOrder] [int] NULL,
        CONSTRAINT [PK_ParticleSubTypeDefinition] PRIMARY KEY CLUSTERED 
        (
            [ParticleSubTypeCategoryID] ASC,
            [Value] ASC
        )
    );
    PRINT 'Created ParticleSubTypeDefinition table.';
END
ELSE
BEGIN
    PRINT 'ParticleSubTypeDefinition table already exists.';
END
GO

PRINT 'Particle analysis table creation completed.';