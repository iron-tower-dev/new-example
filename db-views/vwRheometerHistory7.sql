USE [LabResultsDb]
GO

/****** Object:  View [dbo].[vwRheometerHistory7]    Script Date: 10/28/2025 11:33:19 AM ******/
DROP VIEW [dbo].[vwRheometerHistory7]
GO

/****** Object:  View [dbo].[vwRheometerHistory7]    Script Date: 10/28/2025 11:33:19 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[vwRheometerHistory7]
AS
SELECT     SampleID, Calc1 AS [G` 20a], Calc2 AS [G` 85], Calc3 AS [G` 20b]
FROM         dbo.RheometerCalcs
WHERE     (TestType = 7)

GO

