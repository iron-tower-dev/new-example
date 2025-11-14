USE [LabResultsDb]
GO

/****** Object:  View [dbo].[vwRbotIPDAS]    Script Date: 10/28/2025 11:31:25 AM ******/
DROP VIEW [dbo].[vwRbotIPDAS]
GO

/****** Object:  View [dbo].[vwRbotIPDAS]    Script Date: 10/28/2025 11:31:25 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[vwRbotIPDAS]
AS
SELECT     t.sampleID AS sample_id, t.testID AS test_id, s.sampleDate AS request_date, t.trialNumber AS trial_num, t.value1 AS time_to_pressure_drop, 
                      c.comment AS comments
FROM         dbo.TestReadings t INNER JOIN
                      dbo.UsedLubeSamples s ON t.sampleID = s.ID  LEFT OUTER JOIN
                      dbo.allsamplecomments c ON t.sampleID = c.sampleID and t.testID = c.testID 
WHERE     (t.testID = 170) AND (s.status = 250) AND (s.sampleDate IS NOT NULL) AND (t.trialNumber IS NOT NULL) AND (t.value1 IS NOT NULL)

GO

