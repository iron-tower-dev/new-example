USE [LabResultsDb]
GO

/****** Object:  View [dbo].[vwRheometerHistory2]    Script Date: 10/28/2025 11:32:34 AM ******/
DROP VIEW [dbo].[vwRheometerHistory2]
GO

/****** Object:  View [dbo].[vwRheometerHistory2]    Script Date: 10/28/2025 11:32:34 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[vwRheometerHistory2]
AS
SELECT     SampleID, Calc1 AS [G` 0.1 r/s], Calc2 AS [G` 1 r/s], Calc3 AS [G` 10 r/s], Calc4 AS [G` 100 r/s], Calc5 AS [1/Td 0.1 r/s], Calc6 AS [1/Td 100 r/s], 
                      Calc7 AS [eta* 0.1 r/s], Calc8 AS [eta' 0.1 r/s]
FROM         dbo.RheometerCalcs
WHERE     (TestType = 2)

GO

