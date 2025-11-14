USE [LabResultsDb]
GO

/****** Object:  StoredProcedure [dbo].[sp_SIMCAExportSpecLrg]    Script Date: 10/28/2025 10:46:25 AM ******/
DROP PROCEDURE [dbo].[sp_SIMCAExportSpecLrg]
GO

/****** Object:  StoredProcedure [dbo].[sp_SIMCAExportSpecLrg]    Script Date: 10/28/2025 10:46:25 AM ******/
SET ANSI_NULLS OFF
GO

SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[sp_SIMCAExportSpecLrg]
AS
BEGIN

UPDATE AllResults SET Fe_lrg=Fe,Sn_lrg=Sn,Pb_lrg=Pb,Al_lrg=Al,Zn_lrg=Zn,Cr_lrg=Cr,Cu_lrg=Cu,Ni_lrg=Ni,Ag_lrg=Ag,Na_lrg=Na,Mo_lrg=Mo,Mg_lrg=Mg,P_lrg=P,B_lrg=B,Ca_lrg=Ca,Mn_lrg=Mn,Ba_lrg=Ba,Si_lrg=Si,H_lrg=H
FROM EmSpectro s, AllResults a 
WHERE s.ID = a.SampleID 
AND s.testID=40 
AND s.trialNum=1

END
GO

