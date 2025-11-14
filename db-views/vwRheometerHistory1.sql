USE [LabResultsDb]
GO

/****** Object:  View [dbo].[vwRheometerHistory1]    Script Date: 10/28/2025 11:32:25 AM ******/
DROP VIEW [dbo].[vwRheometerHistory1]
GO

/****** Object:  View [dbo].[vwRheometerHistory1]    Script Date: 10/28/2025 11:32:25 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[vwRheometerHistory1]
AS
SELECT     SampleID, Calc1 AS [G` 30], Calc2 AS [G` 100]
FROM         dbo.RheometerCalcs
WHERE     (TestType = 1)

GO

