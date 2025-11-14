USE [LabResultsDb]
GO

/****** Object:  View [dbo].[vwFerrogramIPDAS]    Script Date: 10/28/2025 10:56:07 AM ******/
DROP VIEW [dbo].[vwFerrogramIPDAS]
GO

/****** Object:  View [dbo].[vwFerrogramIPDAS]    Script Date: 10/28/2025 10:56:07 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[vwFerrogramIPDAS]
AS
SELECT     f.SampleID AS sample_id, 210 AS test_id, s.sampleDate AS request_date, t.trialNumber AS trial_num, f.CsdrdJdgWearSit AS wear_judgment, 
                      f.Comments
FROM         dbo.Ferrogram f INNER JOIN
                      dbo.UsedLubeSamples s ON f.SampleID = s.ID INNER JOIN
                      dbo.TestReadings t ON f.SampleID = t.sampleID
WHERE     (t.testID = 210) AND (s.status = 250) AND (s.sampleDate IS NOT NULL) AND (t.trialNumber IS NOT NULL)


GO

