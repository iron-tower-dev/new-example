USE [LabResultsDb]
GO

/****** Object:  View [dbo].[vwExportTestData]    Script Date: 10/28/2025 10:54:08 AM ******/
DROP VIEW [dbo].[vwExportTestData]
GO

/****** Object:  View [dbo].[vwExportTestData]    Script Date: 10/28/2025 10:54:08 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[vwExportTestData] AS
SELECT dbo.ExportTestData.group_name, dbo.ExportTestData.valueat, dbo.ExportTestData.tagNumber, dbo.Component.name AS component, dbo.Location.name AS location, dbo.ExportTestData.sampleid, dbo.ExportTestData.sampledate, dbo.ExportTestData.testID, dbo.ExportTestData.TestType, dbo.ExportTestData.Val1
FROM dbo.ExportTestData LEFT OUTER JOIN dbo.Location ON dbo.ExportTestData.location = dbo.Location.code LEFT OUTER JOIN dbo.Component ON dbo.ExportTestData.component = dbo.Component.code

GO

