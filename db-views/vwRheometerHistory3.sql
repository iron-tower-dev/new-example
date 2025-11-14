USE [LabResultsDb]
GO

/****** Object:  View [dbo].[vwRheometerHistory3]    Script Date: 10/28/2025 11:32:43 AM ******/
DROP VIEW [dbo].[vwRheometerHistory3]
GO

/****** Object:  View [dbo].[vwRheometerHistory3]    Script Date: 10/28/2025 11:32:43 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[vwRheometerHistory3]
AS
SELECT     SampleID, Calc1 AS [str% 10s], Calc2 AS [str% max], Calc3 AS [str% min], Calc4 AS [str% rcvry]
FROM         dbo.RheometerCalcs
WHERE     (TestType = 3)

GO

