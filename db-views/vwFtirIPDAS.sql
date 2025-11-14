USE [LabResultsDb]
GO

/****** Object:  View [dbo].[vwFtirIPDAS]    Script Date: 10/28/2025 11:19:15 AM ******/
DROP VIEW [dbo].[vwFtirIPDAS]
GO

/****** Object:  View [dbo].[vwFtirIPDAS]    Script Date: 10/28/2025 11:19:15 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[vwFtirIPDAS]
AS
SELECT     f.sampleID AS sample_id, 70 AS test_id, s.sampleDate AS request_date, t.trialNumber AS trial_num, f.anti_oxidant AS anti_oxy, f.oxidation, 
                      c.comment AS comments
FROM         dbo.FTIR f INNER JOIN
                      dbo.UsedLubeSamples s ON f.sampleID = s.ID INNER JOIN
                      dbo.TestReadings t ON f.sampleID = t.sampleID LEFT OUTER JOIN
                      dbo.allsamplecomments c ON t.sampleID = c.sampleID and t.testID = c.testID 
WHERE     (t.testID = 70) AND (s.status = 250) AND (s.sampleDate IS NOT NULL) AND (t.trialNumber IS NOT NULL)



GO

