USE [LabResultsDb]
GO

/****** Object:  View [dbo].[vwParticleCount]    Script Date: 10/28/2025 11:29:42 AM ******/
DROP VIEW [dbo].[vwParticleCount]
GO

/****** Object:  View [dbo].[vwParticleCount]    Script Date: 10/28/2025 11:29:42 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



CREATE VIEW [dbo].[vwParticleCount]
AS
SELECT     dbo.vwGroupSamples.group_name, dbo.vwGroupSamples.valueat, dbo.vwGroupSamples.tagNumber, dbo.vwGroupSamples.component, 
                      dbo.vwGroupSamples.location, dbo.vwGroupSamples.SampleID, dbo.vwGroupSamples.sampleDate, dbo.ParticleCount.micron_5_10, 
                      dbo.ParticleCount.micron_10_15, dbo.ParticleCount.micron_15_25, dbo.ParticleCount.micron_100, dbo.ParticleCount.micron_50_100, 
                      dbo.ParticleCount.micron_25_50
FROM         dbo.vwGroupSamples INNER JOIN
                      dbo.ParticleCount ON dbo.vwGroupSamples.SampleID = dbo.ParticleCount.ID


GO

