USE [LabResultsDb]
GO

/****** Object:  View [dbo].[vwUsedLubeSamplesIPDAS]    Script Date: 10/28/2025 11:41:18 AM ******/
DROP VIEW [dbo].[vwUsedLubeSamplesIPDAS]
GO

/****** Object:  View [dbo].[vwUsedLubeSamplesIPDAS]    Script Date: 10/28/2025 11:41:18 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[vwUsedLubeSamplesIPDAS]
AS
SELECT     siteId AS site_id, ID AS sample_id, tagNumber AS eq_tag_num, component AS lube_component_code, location AS lube_location_code, NULL 
                      AS lube_id, woNumber AS wo_num, sampleDate AS sample_date, results_review_date
FROM         dbo.UsedLubeSamples
WHERE     (status = 250) AND (siteId IS NOT NULL)


GO

