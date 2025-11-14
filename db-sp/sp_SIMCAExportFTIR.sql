USE [LabResultsDb]
GO

/****** Object:  StoredProcedure [dbo].[sp_SIMCAExportFTIR]    Script Date: 10/28/2025 10:45:01 AM ******/
DROP PROCEDURE [dbo].[sp_SIMCAExportFTIR]
GO

/****** Object:  StoredProcedure [dbo].[sp_SIMCAExportFTIR]    Script Date: 10/28/2025 10:45:01 AM ******/
SET ANSI_NULLS OFF
GO

SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[sp_SIMCAExportFTIR]
AS
BEGIN

UPDATE AllResults SET FTIR_H2O=H2O,FTIR_Anti_ox=anti_oxidant,FTIR_Ox=oxidation,FTIR_Dilution=fuel_dilution,FTIR_soot=soot,FTIR_Anti_Wear=zddp,FTIR_cor_Pro=f.nlgi,FTIR_Mixture=mixture,FTIR_Delta=contam 
  FROM FTIR f, AllResults a 
  WHERE f.SampleID = a.SampleID

END
GO

