USE [LabResultsDb]
GO

/****** Object:  UserDefinedFunction [dbo].[MiscTestResult]    Script Date: 10/28/2025 12:03:12 PM ******/
DROP FUNCTION [dbo].[MiscTestResult]
GO

/****** Object:  UserDefinedFunction [dbo].[MiscTestResult]    Script Date: 10/28/2025 12:03:12 PM ******/
SET ANSI_NULLS OFF
GO

SET QUOTED_IDENTIFIER OFF
GO


CREATE FUNCTION [dbo].[MiscTestResult] (@SampleID int, @TestID int)  
RETURNS float AS  
BEGIN 
	RETURN
	(SELECT distinct top 1 value1 FROM TestReadings WHERE sampleID=@SampleID AND testid=@TestID)
END


GO

