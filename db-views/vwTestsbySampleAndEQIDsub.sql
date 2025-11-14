USE [LabResultsDb]
GO

/****** Object:  View [dbo].[vwTestsBySampleAndEQIDsub]    Script Date: 10/28/2025 11:39:21 AM ******/
DROP VIEW [dbo].[vwTestsBySampleAndEQIDsub]
GO

/****** Object:  View [dbo].[vwTestsBySampleAndEQIDsub]    Script Date: 10/28/2025 11:39:21 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[vwTestsBySampleAndEQIDsub]
AS
SELECT     dbo.UsedLubeSamples.ID AS SampleID, dbo.UsedLubeSamples.tagNumber AS Tag, dbo.UsedLubeSamples.component AS ComponentCode, 
                      dbo.UsedLubeSamples.location AS LocationCode, dbo.UsedLubeSamples.lubeType, dbo.UsedLubeSamples.receivedOn, 
                      dbo.UsedLubeSamples.schedule AS SampleSchedule, dbo.vwTestsForLab.TestID, dbo.vwTestsForLab.TestName, dbo.vwTestsForLab.TestAbbrev, 
                      1 AS TrialNumber, dbo.UsedLubeSamples.sampleDate AS ApplicableDate
FROM         dbo.UsedLubeSamples CROSS JOIN
                      dbo.vwTestsForLab

GO

