USE [LabResultsDb]
GO

/****** Object:  StoredProcedure [dbo].[sp_ExportTestData]    Script Date: 10/28/2025 10:42:38 AM ******/
DROP PROCEDURE [dbo].[sp_ExportTestData]
GO

/****** Object:  StoredProcedure [dbo].[sp_ExportTestData]    Script Date: 10/28/2025 10:42:38 AM ******/
SET ANSI_NULLS OFF
GO

SET QUOTED_IDENTIFIER OFF
GO



CREATE     PROCEDURE [dbo].[sp_ExportTestData] AS

truncate table dbo.ExportTestData

exec sp_Spectroscopy 30
exec sp_Spectroscopy 40
exec sp_ParticleCount
exec sp_FTIR
exec sp_Rheometer 1
exec sp_Rheometer 2
exec sp_Rheometer 3
exec sp_Rheometer 4
exec sp_Rheometer 5
exec sp_Rheometer 6
exec sp_Rheometer 7
exec sp_OtherTests 10
exec sp_OtherTests 20
exec sp_OtherTests 50
exec sp_OtherTests 60
exec sp_OtherTests 80
/* exec sp_OtherTests 100 As per Richard Young's comment, 100 removed but left in view where clause */
exec sp_OtherTests 110
exec sp_OtherTests 120
exec sp_OtherTests 130
exec sp_OtherTests 140
exec sp_OtherTests 170
exec sp_OtherTests 180
exec sp_OtherTests 220
exec sp_OtherTests 230
exec sp_OtherTests 240
exec sp_TempLimits
GO

