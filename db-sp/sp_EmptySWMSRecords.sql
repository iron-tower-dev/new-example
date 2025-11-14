USE [LabResultsDb]
GO

/****** Object:  StoredProcedure [dbo].[sp_EmptySWMSRecords]    Script Date: 10/28/2025 10:42:21 AM ******/
DROP PROCEDURE [dbo].[sp_EmptySWMSRecords]
GO

/****** Object:  StoredProcedure [dbo].[sp_EmptySWMSRecords]    Script Date: 10/28/2025 10:42:21 AM ******/
SET ANSI_NULLS OFF
GO

SET QUOTED_IDENTIFIER OFF
GO





-- =============================================
-- Author:		Richard Young
-- Create date: 2016-05-11
-- Description:	Clear all records in the SWMSRecords table
-- =============================================
CREATE PROCEDURE [dbo].[sp_EmptySWMSRecords] AS

--truncate table dbo.SWMSRecords
delete from SWMSRecords

GO

