USE [LabResultsDb]
GO

/****** Object:  View [dbo].[vwSpectroscopy]    Script Date: 10/28/2025 11:36:16 AM ******/
DROP VIEW [dbo].[vwSpectroscopy]
GO

/****** Object:  View [dbo].[vwSpectroscopy]    Script Date: 10/28/2025 11:36:16 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



CREATE VIEW [dbo].[vwSpectroscopy]
AS
SELECT     dbo.vwGroupSamples.group_name, dbo.vwGroupSamples.valueat, dbo.vwGroupSamples.tagNumber, dbo.vwGroupSamples.component, 
                      dbo.vwGroupSamples.location, dbo.vwGroupSamples.SampleID, dbo.vwGroupSamples.sampleDate, dbo.EmSpectro.Na, dbo.EmSpectro.Mo, 
                      dbo.EmSpectro.Mg, dbo.EmSpectro.P, dbo.EmSpectro.B, dbo.EmSpectro.H, dbo.EmSpectro.Cr, dbo.EmSpectro.Ca, dbo.EmSpectro.Ni, 
                      dbo.EmSpectro.Ag, dbo.EmSpectro.Cu, dbo.EmSpectro.Sn, dbo.EmSpectro.Al, dbo.EmSpectro.Mn, dbo.EmSpectro.Pb, dbo.EmSpectro.Fe, 
                      dbo.EmSpectro.Si, dbo.EmSpectro.Ba, dbo.EmSpectro.Sb, dbo.EmSpectro.Zn, dbo.EmSpectro.testID
FROM         dbo.vwGroupSamples INNER JOIN
                      dbo.EmSpectro ON dbo.vwGroupSamples.SampleID = dbo.EmSpectro.ID
WHERE     (dbo.EmSpectro.testID = 30) OR
                      (dbo.EmSpectro.testID = 40)



GO

