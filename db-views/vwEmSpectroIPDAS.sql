USE [LabResultsDb]
GO

/****** Object:  View [dbo].[vwEmSpectroIPDAS]    Script Date: 10/28/2025 10:53:27 AM ******/
DROP VIEW [dbo].[vwEmSpectroIPDAS]
GO

/****** Object:  View [dbo].[vwEmSpectroIPDAS]    Script Date: 10/28/2025 10:53:27 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[vwEmSpectroIPDAS]
AS
SELECT   e.ID AS sample_id, e.testID AS test_id, e.trialDate AS request_date, e.trialNum AS trial_num, e.Na, e.Mo, e.Mg, e.P, e.B, NULL AS H, e.Cr, e.Ca, e.Ni, 
                      e.Ag, e.Cu, e.Sn, e.Al, e.Mn, e.Pb, e.Fe, e.Si, e.Ba, NULL AS Sb, e.Zn, c.comment AS comments
FROM         dbo.EmSpectro e INNER JOIN
                      dbo.UsedLubeSamples s ON e.ID = s.ID
			left join dbo.allsamplecomments c ON e.ID = c.SampleID and e.testID = c.TestID and c.SiteID = 1 
WHERE     (s.status = 250) AND (NOT (e.trialDate IS NULL)) AND (NOT (e.trialNum IS NULL)) and (e.TestID in ('30','40'))

GO

