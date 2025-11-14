USE [LabResultsDb]
GO

/****** Object:  View [dbo].[vwRheometer]    Script Date: 10/28/2025 11:31:50 AM ******/
DROP VIEW [dbo].[vwRheometer]
GO

/****** Object:  View [dbo].[vwRheometer]    Script Date: 10/28/2025 11:31:50 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[vwRheometer]
AS
SELECT dbo.vwGroupSamples.group_name, dbo.vwGroupSamples.valueat, dbo.vwGroupSamples.tagNumber, dbo.vwGroupSamples.component, 
               dbo.vwGroupSamples.location, dbo.vwGroupSamples.SampleID, dbo.vwGroupSamples.SampleID AS ID, dbo.vwGroupSamples.sampleDate, 
               dbo.RheometerCalcs.TestType, dbo.RheometerCalcs.Calc1, dbo.RheometerCalcs.Calc2, dbo.RheometerCalcs.Calc3, dbo.RheometerCalcs.Calc4, 
               dbo.RheometerCalcs.Calc5, dbo.RheometerCalcs.Calc6, dbo.RheometerCalcs.Calc7, dbo.RheometerCalcs.Calc8, 
               dbo.allsamplecomments.Comment AS remarks
FROM  dbo.vwGroupSamples INNER JOIN
               dbo.RheometerCalcs ON dbo.vwGroupSamples.SampleID = dbo.RheometerCalcs.SampleID INNER JOIN
               dbo.allsamplecomments ON dbo.vwGroupSamples.SampleID = dbo.allsamplecomments.SampleID AND dbo.allsamplecomments.TestID = '270'

GO

