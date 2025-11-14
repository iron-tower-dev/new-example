USE [LabResultsDb]
GO

/****** Object:  View [dbo].[vwMTEErrors]    Script Date: 10/28/2025 11:29:01 AM ******/
DROP VIEW [dbo].[vwMTEErrors]
GO

/****** Object:  View [dbo].[vwMTEErrors]    Script Date: 10/28/2025 11:29:01 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[vwMTEErrors] 
AS
	SELECT * FROM system_log
	WHERE entrycomment LIKE '% MTE %'
		AND entrycomment NOT LIKE 'FIXED%'
GO

