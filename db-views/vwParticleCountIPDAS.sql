USE [LabResultsDb]
GO

/****** Object:  View [dbo].[vwParticleCountIPDAS]    Script Date: 10/28/2025 11:29:53 AM ******/
DROP VIEW [dbo].[vwParticleCountIPDAS]
GO

/****** Object:  View [dbo].[vwParticleCountIPDAS]    Script Date: 10/28/2025 11:29:53 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[vwParticleCountIPDAS]
AS
SELECT  distinct p.ID AS sample_id, 160 AS test_id, s.sampleDate AS request_date, t.trialNumber AS trial_num, p.micron_5_10 AS m_5_10, 
                      p.micron_10_15 AS m_10_15, p.micron_15_25 AS m_15_25, p.micron_25_50 AS m_25_50, p.micron_50_100 AS m_50_100, 
                      p.micron_100 AS m_over_100, c.comment AS comments
FROM         dbo.ParticleCount p INNER JOIN
                      dbo.UsedLubeSamples s ON p.ID = s.ID INNER JOIN
                      dbo.TestReadings t ON p.ID = t.sampleID
		left join dbo.allsamplecomments c ON p.ID = c.SampleID and t.testID = c.TestID and c.SiteID = 1
WHERE     (t.testID = 160) AND (s.status = 250) AND (s.sampleDate IS NOT NULL) AND (t.trialNumber IS NOT NULL)

GO

