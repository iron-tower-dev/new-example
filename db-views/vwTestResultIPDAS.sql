USE [LabResultsDb]
GO

/****** Object:  View [dbo].[vwTestResultIPDAS]    Script Date: 10/28/2025 11:37:57 AM ******/
DROP VIEW [dbo].[vwTestResultIPDAS]
GO

/****** Object:  View [dbo].[vwTestResultIPDAS]    Script Date: 10/28/2025 11:37:57 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[vwTestResultIPDAS]
AS
SELECT     sampleid AS sample_id, testID AS test_id, Val1 AS [value]
FROM         dbo.ExportTestData
WHERE     (NOT (Val1 IS NULL))

GO

