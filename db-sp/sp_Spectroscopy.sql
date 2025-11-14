USE [LabResultsDb]
GO

/****** Object:  StoredProcedure [dbo].[sp_Spectroscopy]    Script Date: 10/28/2025 10:47:24 AM ******/
DROP PROCEDURE [dbo].[sp_Spectroscopy]
GO

/****** Object:  StoredProcedure [dbo].[sp_Spectroscopy]    Script Date: 10/28/2025 10:47:24 AM ******/
SET ANSI_NULLS OFF
GO

SET QUOTED_IDENTIFIER ON
GO





CREATE   proc [dbo].[sp_Spectroscopy]
    @vType  int
as
declare @@Text  [varchar] (40) 
select @@Text=
	case
		when @vType=30 then 'Na (std)'
		when @vType=40 then 'Na (lg)'
	end 

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
	@@Text as TestType,
	Na as Val1
FROM         
	dbo.vwSpectroscopy 
WHERE     
	(testID = @vType)

select @@Text=
	case
		when @vType=30 then 'Mo (std)'
		when @vType=40 then 'Mo (lg)'
	end 

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
	@@Text as TestType,
	Mo as Val1
FROM         
	dbo.vwSpectroscopy 
WHERE     
	(testID = @vType)

select @@Text=
	case
		when @vType=30 then 'Mg (std)'
		when @vType=40 then 'Mg (lg)'
	end 

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
	@@Text as TestType,
	Mg as Val1
FROM         
	dbo.vwSpectroscopy 
WHERE     
	(testID = @vType) 	

select @@Text=
	case
		when @vType=30 then 'P (std)'
		when @vType=40 then 'P (lg)'
	end 

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
	@@Text as TestType,
	P as Val1
FROM         
	dbo.vwSpectroscopy 
WHERE     
	(testID = @vType) 

select @@Text=
	case
		when @vType=30 then 'B (std)'
		when @vType=40 then 'B (lg)'
	end 

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
	@@Text as TestType,
	B as Val1
FROM         
	dbo.vwSpectroscopy 
WHERE     
	(testID = @vType) 

select @@Text=
	case
		when @vType=30 then 'H (std)'
		when @vType=40 then 'H (lg)'
	end 

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
	@@Text as TestType,
	H as Val1
FROM         
	dbo.vwSpectroscopy 
WHERE     
	(testID = @vType) 

select @@Text=
	case
		when @vType=30 then 'Cr (std)'
		when @vType=40 then 'Cr (lg)'
	end 

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
	@@Text as TestType,
	Cr as Val1
FROM         
	dbo.vwSpectroscopy 
WHERE     
	(testID = @vType) 

select @@Text=
	case
		when @vType=30 then 'Ca (std)'
		when @vType=40 then 'Ca (lg)'
	end 

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
	@@Text as TestType,
	Ca as Val1
FROM         
	dbo.vwSpectroscopy 
WHERE     
	(testID = @vType) 

select @@Text=
	case
		when @vType=30 then 'Ni (std)'
		when @vType=40 then 'Ni (lg)'
	end 

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
	@@Text as TestType,
	Ni as Val1
FROM         
	dbo.vwSpectroscopy 
WHERE     
	(testID = @vType) 

select @@Text=
	case
		when @vType=30 then 'Ag (std)'
		when @vType=40 then 'Ag (lg)'
	end 

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
	@@Text as TestType,
	Ag as Val1
FROM         
	dbo.vwSpectroscopy 
WHERE     
	(testID = @vType) 

select @@Text=
	case
		when @vType=30 then 'Cu (std)'
		when @vType=40 then 'Cu (lg)'
	end 

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
	@@Text as TestType,
	Cu as Val1
FROM         
	dbo.vwSpectroscopy 
WHERE     
	(testID = @vType) 

select @@Text=
	case
		when @vType=30 then 'Sn (std)'
		when @vType=40 then 'Sn (lg)'
	end 

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
	@@Text as TestType,
	Sn as Val1
FROM         
	dbo.vwSpectroscopy 
WHERE     
	(testID = @vType) 

select @@Text=
	case
		when @vType=30 then 'Al (std)'
		when @vType=40 then 'Al (lg)'
	end 

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
	@@Text as TestType,
	Al as Val1
FROM         
	dbo.vwSpectroscopy 
WHERE     
	(testID = @vType) 

select @@Text=
	case
		when @vType=30 then 'Mn (std)'
		when @vType=40 then 'Mn (lg)'
	end 

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
	@@Text as TestType,
	Mn as Val1
FROM         
	dbo.vwSpectroscopy 
WHERE     
	(testID = @vType) 

select @@Text=
	case
		when @vType=30 then 'Pb (std)'
		when @vType=40 then 'Pb (lg)'
	end 

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
	@@Text as TestType,
	Pb as Val1
FROM         
	dbo.vwSpectroscopy 
WHERE     
	(testID = @vType) 

select @@Text=
	case
		when @vType=30 then 'Fe (std)'
		when @vType=40 then 'Fe (lg)'
	end 

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
	@@Text as TestType,
	Fe as Val1
FROM         
	dbo.vwSpectroscopy 
WHERE     
	(testID = @vType) 

select @@Text=
	case
		when @vType=30 then 'Si (std)'
		when @vType=40 then 'Si (lg)'
	end 

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
	@@Text as TestType,
	Si as Val1
FROM         
	dbo.vwSpectroscopy 
WHERE     
	(testID = @vType) 

select @@Text=
	case
		when @vType=30 then 'Ba (std)'
		when @vType=40 then 'Ba (lg)'
	end 

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
	@@Text as TestType,
	Ba as Val1
FROM         
	dbo.vwSpectroscopy 
WHERE     
	(testID = @vType) 

select @@Text=
	case
		when @vType=30 then 'Zn (std)'
		when @vType=40 then 'Zn (lg)'
	end 

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
	@@Text as TestType,
	Zn as Val1
FROM         
	dbo.vwSpectroscopy 
WHERE     
	(testID = @vType)
GO

