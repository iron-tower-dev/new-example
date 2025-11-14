USE [LabResultsDb]
GO

/****** Object:  View [dbo].[vwRheometerHistory4]    Script Date: 10/28/2025 11:32:52 AM ******/
DROP VIEW [dbo].[vwRheometerHistory4]
GO

/****** Object:  View [dbo].[vwRheometerHistory4]    Script Date: 10/28/2025 11:32:52 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[vwRheometerHistory4]
AS
SELECT     SampleID, Calc1 AS [su max], Calc2 AS [su work], Calc3 AS [su flow]
FROM         dbo.RheometerCalcs
WHERE     (TestType = 4)

GO

