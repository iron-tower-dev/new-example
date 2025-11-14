USE [LabResultsDb]
GO

/****** Object:  View [dbo].[vwSamplesInProgress]    Script Date: 10/28/2025 11:35:46 AM ******/
DROP VIEW [dbo].[vwSamplesInProgress]
GO

/****** Object:  View [dbo].[vwSamplesInProgress]    Script Date: 10/28/2025 11:35:46 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[vwSamplesInProgress]
AS
SELECT     *
FROM         dbo.vwLabOverall
WHERE     (SampleStatus NOT IN (250, 120, 80)) AND (TestDisplayStatus IN ('X', 'R', 'T','E','P'))
GO

