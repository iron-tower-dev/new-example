USE [LabResultsDb]
GO

/****** Object:  StoredProcedure [dbo].[sp_SIMCAExportRheo]    Script Date: 10/28/2025 10:46:05 AM ******/
DROP PROCEDURE [dbo].[sp_SIMCAExportRheo]
GO

/****** Object:  StoredProcedure [dbo].[sp_SIMCAExportRheo]    Script Date: 10/28/2025 10:46:05 AM ******/
SET ANSI_NULLS OFF
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[sp_SIMCAExportRheo]
AS
BEGIN

BEGIN TRAN T1
UPDATE AllResults SET G_30=Calc1,G_100=Calc2 from RheometerCalcs r, AllResults a WHERE r.SampleID = a.SampleID AND r.TestType=1
COMMIT TRAN T1

BEGIN TRAN T2
UPDATE AllResults SET G_0_1_r_s=Calc1,G_1_r_s=Calc2,G_10_r_s=Calc3,G_100_r_s=Calc4,Td_0_1_r_s=Calc5,Td_100_r_s=Calc6,[eta*_0_1_r_s]=Calc7,eta_0_1_r_s=Calc8 from RheometerCalcs r, AllResults a WHERE r.SampleID = a.SampleID AND r.TestType=2
COMMIT TRAN T2

BEGIN TRAN T3
UPDATE AllResults SET str_10s=Calc1,str_max=Calc2,str_min=Calc3,str_rcvry=Calc4 from RheometerCalcs r, AllResults a WHERE r.SampleID = a.SampleID AND r.TestType=3
COMMIT TRAN T3

BEGIN TRAN T4
UPDATE AllResults SET Su_Max=Calc1,Su_Rmp=Calc2,Su_Flow=Calc3 from RheometerCalcs r, AllResults a WHERE r.SampleID = a.SampleID AND r.TestType=4
COMMIT TRAN T4

BEGIN TRAN T5
UPDATE AllResults SET Yield_Stress=Calc1 from RheometerCalcs r, AllResults a WHERE r.SampleID = a.SampleID AND r.TestType=5
COMMIT TRAN T5

BEGIN TRAN T6
UPDATE AllResults SET tswp_init=Calc1,tswp_final=Calc2 from RheometerCalcs r, AllResults a WHERE r.SampleID = a.SampleID AND r.TestType=6
COMMIT TRAN T6

BEGIN TRAN T7
UPDATE AllResults SET G_20a=Calc1,G_85=Calc2,G_20b=Calc3 from RheometerCalcs r, AllResults a WHERE r.SampleID = a.SampleID AND r.TestType=7
COMMIT TRAN T7

END
GO

