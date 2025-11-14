USE [LabResultsDb]
GO

/****** Object:  View [dbo].[vwTestsBySampleAndEQID]    Script Date: 10/28/2025 11:39:05 AM ******/
DROP VIEW [dbo].[vwTestsBySampleAndEQID]
GO

/****** Object:  View [dbo].[vwTestsBySampleAndEQID]    Script Date: 10/28/2025 11:39:05 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[vwTestsBySampleAndEQID]
AS
SELECT     dbo.vwTestsBySampleAndEQIDsub.*, dbo.TestReadings.status AS TestStatus
FROM         dbo.vwTestsBySampleAndEQIDsub LEFT OUTER JOIN
                      dbo.TestReadings ON dbo.vwTestsBySampleAndEQIDsub.TrialNumber = dbo.TestReadings.trialNumber AND 
                      dbo.vwTestsBySampleAndEQIDsub.SampleID = dbo.TestReadings.sampleID AND dbo.vwTestsBySampleAndEQIDsub.TestID = dbo.TestReadings.testID

GO

