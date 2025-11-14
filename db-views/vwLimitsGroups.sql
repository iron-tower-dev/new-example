USE [LabResultsDb]
GO

/****** Object:  View [dbo].[vwLimitsGroups]    Script Date: 10/28/2025 11:22:59 AM ******/
DROP VIEW [dbo].[vwLimitsGroups]
GO

/****** Object:  View [dbo].[vwLimitsGroups]    Script Date: 10/28/2025 11:22:59 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[vwLimitsGroups]
AS
SELECT     valueat AS GroupID, group_name AS GroupName, tagNumber AS Tag, component, location
FROM         dbo.limits_xref
WHERE     (exclude IS NULL OR
                      exclude = 'N')

GO

