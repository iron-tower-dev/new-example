USE [LabResultsDb]
GO

/****** Object:  View [dbo].[vwFlashPtIPDAS]    Script Date: 10/28/2025 11:18:51 AM ******/
DROP VIEW [dbo].[vwFlashPtIPDAS]
GO

/****** Object:  View [dbo].[vwFlashPtIPDAS]    Script Date: 10/28/2025 11:18:51 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[vwFlashPtIPDAS]
AS
SELECT  t.sampleID AS sample_id, t.testID AS test_id, s.sampleDate AS request_date, t.trialNumber AS trial_num, t.value1 AS ambient_pressure, 
                      t.value2 AS temp_at_pt, NULL AS accept_trial, c.comment AS comments
FROM         dbo.TestReadings t INNER JOIN
                      dbo.UsedLubeSamples s ON t.sampleID = s.ID LEFT OUTER JOIN
                      dbo.allsamplecomments c ON t.sampleID = c.sampleID and t.testID = c.testID 
WHERE     (t.testID = 80) AND (s.status = 250) AND (s.sampleDate IS NOT NULL) AND (t.trialNumber IS NOT NULL)

GO

