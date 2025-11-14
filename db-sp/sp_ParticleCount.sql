USE [LabResultsDb]
GO

/****** Object:  StoredProcedure [dbo].[sp_ParticleCount]    Script Date: 10/28/2025 10:43:47 AM ******/
DROP PROCEDURE [dbo].[sp_ParticleCount]
GO

/****** Object:  StoredProcedure [dbo].[sp_ParticleCount]    Script Date: 10/28/2025 10:43:47 AM ******/
SET ANSI_NULLS OFF
GO

SET QUOTED_IDENTIFIER OFF
GO




CREATE  PROCEDURE [dbo].[sp_ParticleCount]
AS

INSERT dbo.ExportTestData 
SELECT     
	group_name, 
	valueat, 
	tagNumber, 
	component, 
	location, 
	SampleID, 
	sampleDate, 
	160 as testID,
	'micron_5_10' as TestType,
	micron_5_10 as Val1
FROM         
	dbo.vwParticleCount

INSERT dbo.ExportTestData 
SELECT     
	group_name, 
	valueat, 
	tagNumber, 
	component, 
	location, 
	SampleID, 
	sampleDate, 
	160 as testID,
	'micron_10_15' as TestType,
	micron_10_15 as Val1
FROM         
	dbo.vwParticleCount 


INSERT dbo.ExportTestData 
SELECT     
	group_name, 
	valueat, 
	tagNumber, 
	component, 
	location, 
	SampleID, 
	sampleDate, 
	160 as testID,
	'micron_15_25' as TestType,
	micron_15_25 as Val1
FROM         
	dbo.vwParticleCount 


INSERT dbo.ExportTestData 
SELECT     
	group_name, 
	valueat, 
	tagNumber, 
	component, 
	location, 
	SampleID, 
	sampleDate, 
	160 as testID,
	'micron_100' as TestType,
	micron_100 as Val1
FROM         
	dbo.vwParticleCount 

INSERT dbo.ExportTestData 
SELECT     
	group_name, 
	valueat, 
	tagNumber, 
	component, 
	location, 
	SampleID, 
	sampleDate, 
	160 as testID,
	'micron_50_100' as TestType,
	micron_50_100 as Val1
FROM         
	dbo.vwParticleCount 

INSERT dbo.ExportTestData 
SELECT     
	group_name, 
	valueat, 
	tagNumber, 
	component, 
	location, 
	SampleID, 
	sampleDate, 
	160 as testID,
	'micron_25_50' as TestType,
	micron_25_50 as Val1
FROM         
	dbo.vwParticleCount
GO

