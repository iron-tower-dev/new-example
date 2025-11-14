USE [LabResultsDb]
GO

/****** Object:  View [dbo].[vwEqLubricationPtIPDAS]    Script Date: 10/28/2025 10:53:45 AM ******/
DROP VIEW [dbo].[vwEqLubricationPtIPDAS]
GO

/****** Object:  View [dbo].[vwEqLubricationPtIPDAS]    Script Date: 10/28/2025 10:53:45 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[vwEqLubricationPtIPDAS]
AS
SELECT     tagNumber AS eq_tag_num, component AS lube_component_code, location AS lube_location_code, applid
FROM         dbo.Lube_Sampling_Point
WHERE     (component IS NOT NULL) AND (location IS NOT NULL) AND (applid IS NOT NULL)


GO

