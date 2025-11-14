USE [LabResultsDb]
GO

/****** Object:  UserDefinedFunction [dbo].[TestResultValue]    Script Date: 10/28/2025 12:03:49 PM ******/
DROP FUNCTION [dbo].[TestResultValue]
GO

/****** Object:  UserDefinedFunction [dbo].[TestResultValue]    Script Date: 10/28/2025 12:03:49 PM ******/
SET ANSI_NULLS OFF
GO

SET QUOTED_IDENTIFIER OFF
GO


CREATE FUNCTION [dbo].[TestResultValue] (@SampleID int, @TestID int)  
RETURNS real AS  
BEGIN 
	RETURN 
		CASE
			WHEN  @TestID IN (20,110,170,220,230) THEN 
				(SELECT distinct top 1 value1 FROM TestReadings WHERE sampleid=@SampleID AND testid=@TestID and trialNumber=1)
			WHEN  @TestID >279 AND @TestID<296 THEN 
				(SELECT distinct top 1 value1 FROM TestReadings WHERE sampleid=@SampleID AND testid=@TestID and trialNumber=1)
			WHEN  @TestID IN (10,140,180) THEN 
				(SELECT distinct top 1 value2 FROM TestReadings WHERE sampleid=@SampleID AND testid=@TestID and trialNumber=1)
			WHEN  @TestID IN (50,60,80) THEN 
				(SELECT distinct top 1 value3 FROM TestReadings WHERE sampleid=@SampleID AND testid=@TestID and trialNumber=1)
			WHEN  @TestID IN (130) THEN 
				(SELECT distinct top 1 trialCalc FROM TestReadings WHERE sampleid=@SampleID AND testid=@TestID and trialNumber=1)
			WHEN  @TestID IN (70) THEN 
				(SELECT distinct top 1 contam FROM FTIR WHERE sampleid=@SampleID)
			WHEN  @TestID IN (160) THEN 
				(SELECT distinct top 1 nas_class FROM ParticleCount WHERE id=@SampleID)
			WHEN  @TestID IN (210) THEN 
				(SELECT distinct top 1 CsdrdJdgWearSit FROM Ferrogram WHERE sampleid=@SampleID)
			WHEN  @TestID IN (400) THEN 
				(SELECT     distinct top 1 limits.ulim3 AS Result
				FROM         UsedLubeSamples INNER JOIN
				                      limits_xref ON UsedLubeSamples.tagNumber = limits_xref.tagNumber AND 
				                      UsedLubeSamples.component = limits_xref.component AND UsedLubeSamples.location = limits_xref.location INNER JOIN
				                      limits ON limits_xref.valueat = limits.limits_xref_id
				WHERE     (limits.testid = 400 and UsedLubeSamples.ID=@SampleID))
			ELSE NULL
		END

END






GO

