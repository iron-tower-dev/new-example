USE [LabResultsDb]
GO

/****** Object:  View [dbo].[vwFilterResidueIPDAS]    Script Date: 10/28/2025 10:56:36 AM ******/
DROP VIEW [dbo].[vwFilterResidueIPDAS]
GO

/****** Object:  View [dbo].[vwFilterResidueIPDAS]    Script Date: 10/28/2025 10:56:36 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[vwFilterResidueIPDAS]
AS
SELECT   t.sampleID AS sample_id, t.testID AS test_id, s.sampleDate AS request_date, t.trialNumber AS trial_num, t.value1 AS sample_size, 
                      t.value3 AS residue_wt, NULL AS std_sample, NULL AS accept_trial, c.comment AS comments
FROM         dbo.TestReadings t INNER JOIN
                      dbo.UsedLubeSamples s ON t.sampleID = s.ID LEFT OUTER JOIN
                      dbo.allsamplecomments c ON t.sampleID = c.sampleID and t.testID = c.testID 
WHERE     (t.testID = 180) AND (s.status = 250) AND (s.sampleDate IS NOT NULL) AND (t.trialNumber IS NOT NULL)

GO

