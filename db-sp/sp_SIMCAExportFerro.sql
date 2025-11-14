USE [LabResultsDb]
GO

/****** Object:  StoredProcedure [dbo].[sp_SIMCAExportFerro]    Script Date: 10/28/2025 10:44:36 AM ******/
DROP PROCEDURE [dbo].[sp_SIMCAExportFerro]
GO

/****** Object:  StoredProcedure [dbo].[sp_SIMCAExportFerro]    Script Date: 10/28/2025 10:44:36 AM ******/
SET ANSI_NULLS OFF
GO

SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[sp_SIMCAExportFerro]
AS
BEGIN

BEGIN TRAN FERRO
UPDATE AllResults 
SET AllResults.DilutionFactor=

CASE f.DilutionFactor
WHEN '3:2' THEN 3
WHEN '1:10' THEN 10
WHEN '1:100' THEN 100
ELSE NULL
END,

AllResults.NormalRubbing=f.NormalRubbing,AllResults.SevereWearSliding=f.SevereWearSliding,AllResults.SevereWearFatigue=f.SevereWearFatigue,AllResults.CuttingPart=f.CuttingPart,AllResults.LaminarPart=f.LaminarPart,AllResults.Spheres=f.Spheres,AllResults.DarkMetalloOde=f.DarkMetalloOde,AllResults.RedOde=f.RedOde,AllResults.CorrosiveWearPart=f.CorrosiveWearPart,AllResults.NonFerrousMetal=f.NonFerrousMetal,AllResults.NonMetallicInorganic=f.NonMetallicInorganic,AllResults.BirefringentOrganic=f.BirefringentOrganic,AllResults.NonMetallicAmorphous=f.NonMetallicAmporphous,AllResults.FrictionPolymers=f.FrictionPolymers,AllResults.Fibers=f.Fibers,AllResults.Judgment=f.CsdrdJdgWearSit 
FROM Ferrogram f, AllResults a 
WHERE f.SampleID = a.SampleID
COMMIT TRAN FERRO

END

GO

