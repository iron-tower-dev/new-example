USE [LabResultsDb]
GO

/****** Object:  StoredProcedure [dbo].[sp_SIMCAExportSpecStd]    Script Date: 10/28/2025 10:46:49 AM ******/
DROP PROCEDURE [dbo].[sp_SIMCAExportSpecStd]
GO

/****** Object:  StoredProcedure [dbo].[sp_SIMCAExportSpecStd]    Script Date: 10/28/2025 10:46:49 AM ******/
SET ANSI_NULLS OFF
GO

SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[sp_SIMCAExportSpecStd]
AS
BEGIN

UPDATE AllResults SET Fe_std=Fe,Sn_std=Sn,Pb_std=Pb,Al_std=Al,Zn_std=Zn,Cr_std=Cr,Cu_std=Cu,Ni_std=Ni,Ag_std=Ag,Na_std=Na,Mo_std=Mo,Mg_std=Mg,P_std=P,B_std=B,Ca_std=Ca,Mn_std=Mn,Ba_std=Ba,Si_std=Si,H_std=H
FROM EmSpectro s, AllResults a 
WHERE s.ID = a.SampleID 
AND s.testID=30 
AND s.trialNum=1

END
GO

