USE [LabResultsDb]
GO

/****** Object:  View [dbo].[vwFTIR]    Script Date: 10/28/2025 11:19:01 AM ******/
DROP VIEW [dbo].[vwFTIR]
GO

/****** Object:  View [dbo].[vwFTIR]    Script Date: 10/28/2025 11:19:01 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



CREATE VIEW [dbo].[vwFTIR]
AS
SELECT     dbo.vwGroupSamples.group_name, dbo.vwGroupSamples.valueat, dbo.vwGroupSamples.tagNumber, dbo.vwGroupSamples.component, 
                      dbo.vwGroupSamples.location, dbo.vwGroupSamples.SampleID, dbo.vwGroupSamples.sampleDate, dbo.FTIR.anti_oxidant, dbo.FTIR.oxidation, 
                      dbo.FTIR.H2O, dbo.FTIR.zddp, dbo.FTIR.soot, dbo.FTIR.fuel_dilution, dbo.FTIR.mixture, dbo.FTIR.NLGI, dbo.FTIR.contam
FROM         dbo.vwGroupSamples INNER JOIN
                      dbo.FTIR ON dbo.vwGroupSamples.SampleID = dbo.FTIR.sampleID


GO

