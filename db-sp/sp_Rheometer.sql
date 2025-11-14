USE [LabResultsDb]
GO

/****** Object:  StoredProcedure [dbo].[sp_Rheometer]    Script Date: 10/28/2025 10:44:04 AM ******/
DROP PROCEDURE [dbo].[sp_Rheometer]
GO

/****** Object:  StoredProcedure [dbo].[sp_Rheometer]    Script Date: 10/28/2025 10:44:04 AM ******/
SET ANSI_NULLS OFF
GO

SET QUOTED_IDENTIFIER ON
GO





CREATE     proc [dbo].[sp_Rheometer]
    @vType  int
as
if (@vType=1)
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
			270 as testID,
			'g30' as TestType,
			Calc1 as Val1
		FROM         
			dbo.vwRheometer 
		WHERE     
			(TestType = @vType)
	
		INSERT dbo.ExportTestData 
		SELECT     
			group_name, 
			valueat, 
			tagNumber, 
			component, 
			location, 
			SampleID, 
			sampleDate, 
			270 as testID,
			'g100' as TestType,
			Calc2 as Val1
		FROM         
			dbo.vwRheometer 
		WHERE     
			(TestType = @vType)

	end

if (@vType=2)
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
			270 as testID,
			'g10_-1' as TestType,
			Calc1 as Val1
		FROM         
			dbo.vwRheometer 
		WHERE     
			(TestType = @vType)
	
		INSERT dbo.ExportTestData 
		SELECT     
			group_name, 
			valueat, 
			tagNumber, 
			component, 
			location, 
			SampleID, 
			sampleDate, 
			270 as testID,
			'g10_0' as TestType,
			Calc2 as Val1
		FROM         
			dbo.vwRheometer 
		WHERE     
			(TestType = @vType)
		

		INSERT dbo.ExportTestData 
		SELECT     
			group_name, 
			valueat, 
			tagNumber, 
			component, 
			location, 
			SampleID, 
			sampleDate, 
			270 as testID,
			'g10_1' as TestType,
			Calc3 as Val1
		FROM         
			dbo.vwRheometer 
		WHERE     
			(TestType = @vType)

		

		INSERT dbo.ExportTestData 
		SELECT     
			group_name, 
			valueat, 
			tagNumber, 
			component, 
			location, 
			SampleID, 
			sampleDate, 
			270 as testID,
			'g10_2' as TestType,
			Calc4 as Val1
		FROM         
			dbo.vwRheometer 
		WHERE     
			(TestType = @vType)






		INSERT dbo.ExportTestData 
		SELECT     
			group_name, 
			valueat, 
			tagNumber, 
			component, 
			location, 
			SampleID, 
			sampleDate, 
			270 as testID,
			'Td_-1' as TestType,
			Calc5 as Val1
		FROM         
			dbo.vwRheometer 
		WHERE     
			(TestType = @vType)
	
		INSERT dbo.ExportTestData 
		SELECT     
			group_name, 
			valueat, 
			tagNumber, 
			component, 
			location, 
			SampleID, 
			sampleDate, 
			270 as testID,
			'Td_2' as TestType,
			Calc6 as Val1
		FROM         
			dbo.vwRheometer 
		WHERE     
			(TestType = @vType)
		

		INSERT dbo.ExportTestData 
		SELECT     
			group_name, 
			valueat, 
			tagNumber, 
			component, 
			location, 
			SampleID, 
			sampleDate, 
			270 as testID,
			'eta*_-1' as TestType,
			Calc7 as Val1
		FROM         
			dbo.vwRheometer 
		WHERE     
			(TestType = @vType)

		

		INSERT dbo.ExportTestData 
		SELECT     
			group_name, 
			valueat, 
			tagNumber, 
			component, 
			location, 
			SampleID, 
			sampleDate, 
			270 as testID,
			'eta_-1' as TestType,
			Calc8 as Val1
		FROM         
			dbo.vwRheometer 
		WHERE     
			(TestType = @vType)

	end

if (@vType=3)
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
			270 as testID,
			'str% 10' as TestType,
			Calc1 as Val1
		FROM         
			dbo.vwRheometer 
		WHERE     
			(TestType = @vType)
	
		INSERT dbo.ExportTestData 
		SELECT     
			group_name, 
			valueat, 
			tagNumber, 
			component, 
			location, 
			SampleID, 
			sampleDate, 
			270 as testID,
			'str% max' as TestType,
			Calc2 as Val1
		FROM         
			dbo.vwRheometer 
		WHERE     
			(TestType = @vType)
		

		INSERT dbo.ExportTestData 
		SELECT     
			group_name, 
			valueat, 
			tagNumber, 
			component, 
			location, 
			SampleID, 
			sampleDate, 
			270 as testID,
			'str% min' as TestType,
			Calc3 as Val1
		FROM         
			dbo.vwRheometer 
		WHERE     
			(TestType = @vType)

		

		INSERT dbo.ExportTestData 
		SELECT     
			group_name, 
			valueat, 
			tagNumber, 
			component, 
			location, 
			SampleID, 
			sampleDate, 
			270 as testID,
			'str% rcvry' as TestType,
			Calc4 as Val1
		FROM         
			dbo.vwRheometer 
		WHERE     
			(TestType = @vType)

	end

if (@vType=4)
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
			270 as testID,
			'sumax' as TestType,
			Calc1 as Val1
		FROM         
			dbo.vwRheometer 
		WHERE     
			(TestType = @vType)
	
		INSERT dbo.ExportTestData 
		SELECT     
			group_name, 
			valueat, 
			tagNumber, 
			component, 
			location, 
			SampleID, 
			sampleDate, 
			270 as testID,
			'su rmp' as TestType,
			Calc2 as Val1
		FROM         
			dbo.vwRheometer 
		WHERE     
			(TestType = @vType)
		

		INSERT dbo.ExportTestData 
		SELECT     
			group_name, 
			valueat, 
			tagNumber, 
			component, 
			location, 
			SampleID, 
			sampleDate, 
			270 as testID,
			'suflow' as TestType,
			Calc3 as Val1
		FROM         
			dbo.vwRheometer 
		WHERE     
			(TestType = @vType)
	end


if (@vType=5)
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
			270 as testID,
			'YieldStress' as TestType,
			Calc1 as Val1
		FROM         
			dbo.vwRheometer 
		WHERE     
			(TestType = @vType)
	
	end


if (@vType=6)
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
			270 as testID,
			'tswpinit' as TestType,
			Calc1 as Val1
		FROM         
			dbo.vwRheometer 
		WHERE     
			(TestType = @vType)
	
		INSERT dbo.ExportTestData 
		SELECT     
			group_name, 
			valueat, 
			tagNumber, 
			component, 
			location, 
			SampleID, 
			sampleDate, 
			270 as testID,
			'tswpfinal' as TestType,
			Calc2 as Val1
		FROM         
			dbo.vwRheometer 
		WHERE     
			(TestType = @vType)
	end


if (@vType=7)
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
			270 as testID,
			'g20a' as TestType,
			Calc1 as Val1
		FROM         
			dbo.vwRheometer 
		WHERE     
			(TestType = @vType)
	
		INSERT dbo.ExportTestData 
		SELECT     
			group_name, 
			valueat, 
			tagNumber, 
			component, 
			location, 
			SampleID, 
			sampleDate, 
			270 as testID,
			'g85' as TestType,
			Calc2 as Val1
		FROM         
			dbo.vwRheometer 
		WHERE     
			(TestType = @vType)
		

		INSERT dbo.ExportTestData 
		SELECT     
			group_name, 
			valueat, 
			tagNumber, 
			component, 
			location, 
			SampleID, 
			sampleDate, 
			270 as testID,
			'g20b' as TestType,
			Calc3 as Val1
		FROM         
			dbo.vwRheometer 
		WHERE     
			(TestType = @vType)

	end
GO

