USE [LabResultsDb]
GO

/****** Object:  View [dbo].[vwAllResultsIKS]    Script Date: 10/28/2025 10:52:32 AM ******/
DROP VIEW [dbo].[vwAllResultsIKS]
GO

/****** Object:  View [dbo].[vwAllResultsIKS]    Script Date: 10/28/2025 10:52:32 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[vwAllResultsIKS]
AS
SELECT     dbo.AllResults.SampleID, dbo.AllResults.GroupName, dbo.AllResults.Lube, dbo.AllResults.EQID, dbo.AllResults.Comp, dbo.AllResults.Loc, 
                      dbo.AllResults.Temperature, dbo.AllResults.[Time(M)], dbo.AllResults.Goodness, dbo.AllResults.Vis_40, dbo.AllResults.Vis_100, dbo.AllResults.AN, 
                      dbo.AllResults.KF, dbo.AllResults.FTIR_H2O, dbo.AllResults.Filter_Res, dbo.AllResults.Fe_std, dbo.AllResults.Sn_std, dbo.AllResults.Pb_std, 
                      dbo.AllResults.Al_std, dbo.AllResults.Zn_std, dbo.AllResults.Cr_std, dbo.AllResults.Cu_std, dbo.AllResults.Yield_Stress, dbo.AllResults.Su_Max, 
                      dbo.AllResults.Su_Rmp, dbo.AllResults.Su_Flow, dbo.AllResults.G_30, dbo.AllResults.G_100, dbo.AllResults.FTIR_Anti_ox, dbo.AllResults.FTIR_Ox, 
                      dbo.AllResults.FTIR_Dilution, dbo.AllResults.Oil_Content, dbo.AllResults.Bleed, dbo.AllResults.RBOT, dbo.AllResults.TFOUT, dbo.AllResults.TBN, 
                      dbo.AllResults.Flash_Pt, dbo.AllResults.FTIR_soot, dbo.AllResults.FTIR_Anti_Wear, dbo.AllResults.FTIR_cor_Pro, dbo.AllResults.FTIR_Mixture, 
                      dbo.AllResults.FTIR_Delta, dbo.AllResults.str_10s, dbo.AllResults.str_max, dbo.AllResults.str_min, dbo.AllResults.str_rcvry, 
                      dbo.AllResults.Td_0_1_r_s, dbo.AllResults.Td_100_r_s, dbo.AllResults.[eta*_0_1_r_s], dbo.AllResults.eta_0_1_r_s, dbo.AllResults.G_0_1_r_s, 
                      dbo.AllResults.G_1_r_s, dbo.AllResults.G_10_r_s, dbo.AllResults.G_100_r_s, dbo.AllResults.tswp_init, dbo.AllResults.tswp_final, 
                      dbo.AllResults.G_20a, dbo.AllResults.G_85, dbo.AllResults.G_20b, dbo.AllResults.Cone_Pen, dbo.AllResults.NLGI, dbo.AllResults.drop_pt, 
                      dbo.AllResults.Ni_std, dbo.AllResults.Ag_std, dbo.AllResults.Na_std, dbo.AllResults.Mo_std, dbo.AllResults.Mg_std, dbo.AllResults.P_std, 
                      dbo.AllResults.B_std, dbo.AllResults.Ca_std, dbo.AllResults.Mn_std, dbo.AllResults.Ba_std, dbo.AllResults.Si_std, dbo.AllResults.H_std, 
                      dbo.AllResults.PC_5_10, dbo.AllResults.PC_10_15, dbo.AllResults.PC_15_25, dbo.AllResults.PC_25_50, dbo.AllResults.PC_50_100, 
                      dbo.AllResults.PC_100_plus, dbo.AllResults.NAS, dbo.AllResults.Resis, dbo.AllResults.Chlorides, dbo.AllResults.Phenol, dbo.AllResults.Amine, 
                      dbo.AllResults.Friction, dbo.AllResults.Rust, dbo.AllResults.Fe_lrg, dbo.AllResults.Cr_lrg, dbo.AllResults.Cu_lrg, dbo.AllResults.Sn_lrg, 
                      dbo.AllResults.Pb_lrg, dbo.AllResults.Al_lrg, dbo.AllResults.Ni_lrg, dbo.AllResults.Ag_lrg, dbo.AllResults.Zn_lrg, dbo.AllResults.Na_lrg, 
                      dbo.AllResults.Mo_lrg, dbo.AllResults.Mg_lrg, dbo.AllResults.P_lrg, dbo.AllResults.B_lrg, dbo.AllResults.Ca_lrg, dbo.AllResults.Mn_lrg, 
                      dbo.AllResults.Ba_lrg, dbo.AllResults.Si_lrg, dbo.AllResults.H_lrg, dbo.AllResults.DilutionFactor, dbo.AllResults.NormalRubbing, 
                      dbo.AllResults.SevereWearSliding, dbo.AllResults.SevereWearFatigue, dbo.AllResults.CuttingPart, dbo.AllResults.LaminarPart, 
                      dbo.AllResults.Spheres, dbo.AllResults.DarkMetalloOde, dbo.AllResults.RedOde, dbo.AllResults.CorrosiveWearPart, dbo.AllResults.NonFerrousMetal, 
                      dbo.AllResults.NonMetallicInorganic, dbo.AllResults.BirefringentOrganic, dbo.AllResults.NonMetallicAmorphous, dbo.AllResults.FrictionPolymers, 
                      dbo.AllResults.Fibers, dbo.AllResults.Judgment, dbo.Lube_Sampling_Point.applid, dbo.UsedLubeSamples.sampleDate
FROM         dbo.Lube_Sampling_Point INNER JOIN
                      dbo.Component ON dbo.Lube_Sampling_Point.component = dbo.Component.code INNER JOIN
                      dbo.Location ON dbo.Lube_Sampling_Point.location = dbo.Location.code INNER JOIN
                      dbo.AllResults ON dbo.Component.name = dbo.AllResults.Comp AND dbo.Location.name = dbo.AllResults.Loc AND 
                      dbo.Lube_Sampling_Point.tagNumber = dbo.AllResults.EQID INNER JOIN
                      dbo.UsedLubeSamples ON dbo.AllResults.SampleID = dbo.UsedLubeSamples.ID
WHERE     (dbo.Component.site_id = 1) AND (dbo.Location.site_id = 1)

GO

