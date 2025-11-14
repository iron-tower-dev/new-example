USE [LabResultsDb]
GO

/****** Object:  View [dbo].[vwViscosityIPDAS]    Script Date: 10/28/2025 11:41:34 AM ******/
DROP VIEW [dbo].[vwViscosityIPDAS]
GO

/****** Object:  View [dbo].[vwViscosityIPDAS]    Script Date: 10/28/2025 11:41:34 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[vwViscosityIPDAS]
AS
SELECT    t.sampleID AS sample_id, t.testID AS test_id, s.sampleDate AS request_date, t.trialNumber AS trial_num, t.ID1 AS timer_num, t.ID2 AS tube_num, NULL 
                      AS cal_const, t.value1 AS elapsed_time, t.value3 AS result, NULL AS accept_trial, c.comment AS comments
FROM         dbo.TestReadings t INNER JOIN
                      dbo.UsedLubeSamples s ON t.sampleID = s.ID LEFT OUTER JOIN
                      dbo.allsamplecomments c ON t.sampleID = c.sampleID and t.testID = c.testID 
WHERE     (t.testID IN (50, 60)) AND (s.status = 250) AND (s.sampleDate IS NOT NULL) AND (t.trialNumber IS NOT NULL)
GO

