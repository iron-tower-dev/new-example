USE [LabResultsDb]
GO

/****** Object:  StoredProcedure [dbo].[sp_SIMCAExportStub]    Script Date: 10/28/2025 10:47:09 AM ******/
DROP PROCEDURE [dbo].[sp_SIMCAExportStub]
GO

/****** Object:  StoredProcedure [dbo].[sp_SIMCAExportStub]    Script Date: 10/28/2025 10:47:09 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[sp_SIMCAExportStub] AS
BEGIN

TRUNCATE TABLE AllResults

INSERT AllResults (SampleID, GroupName, Lube, EQID, Comp, Loc)

SELECT DISTINCT 
                      dbo.UsedLubeSamples.ID, dbo.limits_xref.group_name, dbo.UsedLubeSamples.lubeType, dbo.UsedLubeSamples.tagNumber, 
                      dbo.Component.name AS Component, dbo.Location.name AS Location
FROM         dbo.Component INNER JOIN
                      dbo.UsedLubeSamples ON dbo.Component.code = dbo.UsedLubeSamples.component INNER JOIN
                      dbo.Location ON dbo.UsedLubeSamples.location = dbo.Location.code LEFT OUTER JOIN
                      dbo.limits_xref ON dbo.UsedLubeSamples.tagNumber = dbo.limits_xref.tagNumber AND 
                      dbo.UsedLubeSamples.component = dbo.limits_xref.component AND dbo.UsedLubeSamples.location = dbo.limits_xref.location

END
GO

