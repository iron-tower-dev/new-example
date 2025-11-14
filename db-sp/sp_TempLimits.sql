USE [LabResultsDb]
GO

/****** Object:  StoredProcedure [dbo].[sp_TempLimits]    Script Date: 10/28/2025 10:47:38 AM ******/
DROP PROCEDURE [dbo].[sp_TempLimits]
GO

/****** Object:  StoredProcedure [dbo].[sp_TempLimits]    Script Date: 10/28/2025 10:47:38 AM ******/
SET ANSI_NULLS OFF
GO

SET QUOTED_IDENTIFIER OFF
GO





CREATE   PROCEDURE [dbo].[sp_TempLimits]
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
	400 as testID,
	'Temperature' as TestType,
	ulim3 as Val1
FROM         
	dbo.vwTempLimits
GO

