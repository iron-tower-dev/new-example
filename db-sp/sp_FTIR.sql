USE [LabResultsDb]
GO

/****** Object:  StoredProcedure [dbo].[sp_FTIR]    Script Date: 10/28/2025 10:43:06 AM ******/
DROP PROCEDURE [dbo].[sp_FTIR]
GO

/****** Object:  StoredProcedure [dbo].[sp_FTIR]    Script Date: 10/28/2025 10:43:06 AM ******/
SET ANSI_NULLS OFF
GO

SET QUOTED_IDENTIFIER OFF
GO



CREATE  PROCEDURE [dbo].[sp_FTIR]
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
	70 as testID,
	'anti_oxidant' as TestType,
	anti_oxidant as Val1
FROM
	dbo.vwFTIR

INSERT dbo.ExportTestData
SELECT
	group_name,
	valueat,
	tagNumber,
	component,
	location,
	SampleID,
	sampleDate,
	70 as testID,
	'oxidation' as TestType,
	oxidation as Val1
FROM
	dbo.vwFTIR

INSERT dbo.ExportTestData
SELECT
	group_name,
	valueat,
	tagNumber,
	component,
	location,
	SampleID,
	sampleDate,
	70 as testID,
	'H2O' as TestType,
	H2O as Val1
FROM
	dbo.vwFTIR

INSERT dbo.ExportTestData
SELECT
	group_name,
	valueat,
	tagNumber,
	component,
	location,
	SampleID,
	sampleDate,
	70 as testID,
	'Anti-wear' as TestType,
	zddp as Val1
FROM
	dbo.vwFTIR

INSERT dbo.ExportTestData
SELECT
	group_name,
	valueat,
	tagNumber,
	component,
	location,
	SampleID,
	sampleDate,
	70 as testID,
	'soot' as TestType,
	soot as Val1
FROM
	dbo.vwFTIR

INSERT dbo.ExportTestData
SELECT
	group_name,
	valueat,
	tagNumber,
	component,
	location,
	SampleID,
	sampleDate,
	70 as testID,
	'dilution' as TestType,
	fuel_dilution as Val1
FROM
	dbo.vwFTIR

INSERT dbo.ExportTestData
SELECT
	group_name,
	valueat,
	tagNumber,
	component,
	location,
	SampleID,
	sampleDate,
	70 as testID,
	'mixture' as TestType,
	mixture as Val1
FROM
	dbo.vwFTIR

INSERT dbo.ExportTestData
SELECT
	group_name,
	valueat,
	tagNumber,
	component,
	location,
	SampleID,
	sampleDate,
	70 as testID,
	'Corr_pro' as TestType,
	NLGI as Val1
FROM
	dbo.vwFTIR

INSERT dbo.ExportTestData
SELECT
	group_name,
	valueat,
	tagNumber,
	component,
	location,
	SampleID,
	sampleDate,
	70 as testID,
	'Delta_area' as TestType,
	contam as Val1
FROM
	dbo.vwFTIR


GO

