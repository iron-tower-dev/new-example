USE [LabResultsDb]
GO

/****** Object:  View [dbo].[vwMTEErrorsFixed]    Script Date: 10/28/2025 11:29:15 AM ******/
DROP VIEW [dbo].[vwMTEErrorsFixed]
GO

/****** Object:  View [dbo].[vwMTEErrorsFixed]    Script Date: 10/28/2025 11:29:15 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[vwMTEErrorsFixed]
AS
	SELECT * FROM system_log
	WHERE entrycomment LIKE 'FIXED%'
GO

