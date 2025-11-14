USE [LabResultsDb]
GO

/****** Object:  View [dbo].[vwTestIPDAS]    Script Date: 10/28/2025 11:37:14 AM ******/
DROP VIEW [dbo].[vwTestIPDAS]
GO

/****** Object:  View [dbo].[vwTestIPDAS]    Script Date: 10/28/2025 11:37:14 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[vwTestIPDAS]
AS
SELECT     ID AS test_id, name, abbrev, sampleVolumeRequired AS sample_amt_unit
FROM         dbo.Test
WHERE (ID IS NOT NULL) AND (name IS NOT NULL) AND (abbrev IS NOT NULL)



GO

