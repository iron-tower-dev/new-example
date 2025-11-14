USE [LabResultsDb]
GO

/****** Object:  StoredProcedure [dbo].[sp_SIMCAExportPC]    Script Date: 10/28/2025 10:45:18 AM ******/
DROP PROCEDURE [dbo].[sp_SIMCAExportPC]
GO

/****** Object:  StoredProcedure [dbo].[sp_SIMCAExportPC]    Script Date: 10/28/2025 10:45:18 AM ******/
SET ANSI_NULLS OFF
GO

SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[sp_SIMCAExportPC]
AS
BEGIN

UPDATE AllResults SET PC_5_10=micron_5_10,PC_10_15=micron_10_15,PC_15_25=micron_15_25,PC_25_50=micron_25_50,PC_50_100=micron_50_100,PC_100_plus=micron_100 ,NAS=nas_class
FROM ParticleCount p, AllResults a 
WHERE p.ID = a.SampleID

END
GO

