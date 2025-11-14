USE [LabResultsDb]
GO

/****** Object:  StoredProcedure [dbo].[sp_OtherTests]    Script Date: 10/28/2025 10:43:24 AM ******/
DROP PROCEDURE [dbo].[sp_OtherTests]
GO

/****** Object:  StoredProcedure [dbo].[sp_OtherTests]    Script Date: 10/28/2025 10:43:24 AM ******/
SET ANSI_NULLS OFF
GO

SET QUOTED_IDENTIFIER ON
GO




CREATE    proc [dbo].[sp_OtherTests]
    @vType  int
as
if (@vType=20) or (@vType=110) or (@vType=170) or (@vType=220) or (@vType=230) or (@vType=240)
	begin
		INSERT dbo.ExportTestData 
		SELECT     
			group_name, 
			valueat, 
			tagNumber, 
			component, 
			location, 
			SampleID, 
			sampleDate, 
			@vType as testID,
			TestType,
			value1 as Val1
		FROM         
			dbo.vwOtherTests 
		WHERE     
			(TestID = @vType)
	end

if (@vType=50) or (@vType=60) or (@vType=80) 
	begin
		INSERT dbo.ExportTestData 
		SELECT     
			group_name, 
			valueat, 
			tagNumber, 
			component, 
			location, 
			SampleID, 
			sampleDate, 
			@vType as testID,
			TestType,
			value3 as Val1
		FROM         
			dbo.vwOtherTests 
		WHERE     
			(TestID = @vType)
	end

if (@vType=140) or (@vType=180) or (@vType=10) 
	begin
		INSERT dbo.ExportTestData 
		SELECT     
			group_name, 
			valueat, 
			tagNumber, 
			component, 
			location, 
			SampleID, 
			sampleDate, 
			@vType as testID,
			TestType,
			value2 as Val1
		FROM         
			dbo.vwOtherTests 
		WHERE     
			(TestID = @vType)
	end

if (@vType=120) 
	begin
		INSERT dbo.ExportTestData 
		SELECT     
			group_name, 
			valueat, 
			tagNumber, 
			component, 
			location, 
			SampleID, 
			sampleDate, 
			@vType as testID,
			TestType,
			ID1 as Val1
		FROM         
			dbo.vwOtherTests 
		WHERE     
			(TestID = @vType)
	end

if (@vType=130) 
	begin
		INSERT dbo.ExportTestData 
		SELECT     
			group_name, 
			valueat, 
			tagNumber, 
			component, 
			location, 
			SampleID, 
			sampleDate, 
			@vType as testID,
			TestType,
			trialCalc as Val1
		FROM         
			dbo.vwOtherTests 
		WHERE     
			(TestID = @vType)
	end
GO

