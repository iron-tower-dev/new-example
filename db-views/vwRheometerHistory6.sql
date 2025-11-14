USE [LabResultsDb]
GO

/****** Object:  View [dbo].[vwRheometerHistory6]    Script Date: 10/28/2025 11:33:11 AM ******/
DROP VIEW [dbo].[vwRheometerHistory6]
GO

/****** Object:  View [dbo].[vwRheometerHistory6]    Script Date: 10/28/2025 11:33:11 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[vwRheometerHistory6]
AS
SELECT     SampleID, Calc1 AS [Tswp init], Calc2 AS Tswpfinal
FROM         dbo.RheometerCalcs
WHERE     (TestType = 6)

GO

