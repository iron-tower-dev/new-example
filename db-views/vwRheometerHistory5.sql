USE [LabResultsDb]
GO

/****** Object:  View [dbo].[vwRheometerHistory5]    Script Date: 10/28/2025 11:33:00 AM ******/
DROP VIEW [dbo].[vwRheometerHistory5]
GO

/****** Object:  View [dbo].[vwRheometerHistory5]    Script Date: 10/28/2025 11:33:00 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[vwRheometerHistory5]
AS
SELECT     SampleID, Calc1 AS [Yield stress]
FROM         dbo.RheometerCalcs
WHERE     (TestType = 5)

GO

