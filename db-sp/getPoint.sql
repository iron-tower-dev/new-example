USE [LabResultsDb]
GO

/****** Object:  StoredProcedure [dbo].[getPoint]    Script Date: 10/28/2025 10:41:52 AM ******/
DROP PROCEDURE [dbo].[getPoint]
GO

/****** Object:  StoredProcedure [dbo].[getPoint]    Script Date: 10/28/2025 10:41:52 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[getPoint] 
	-- Add the parameters for the stored procedure here
	--<@Param1, sysname, @p1> <Datatype_For_Param1, , int> = <Default_Value_For_Param1, , 0>, 
	--<@Param2, sysname, @p2> <Datatype_For_Param2, , int> = <Default_Value_For_Param2, , 0>
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT a.Vis_100, sampleDate
	from vwAllResultsIKS a
	where 
	--P1="Ag_std" 
	eqid='1EPEAG01'
	--and 
	--loc='GENERATOR'
	--and
	--comp='OUTBOARD BEARING' 
	and sampleDate >'3/25/2017'
	and Vis_100 is not null
END
GO

