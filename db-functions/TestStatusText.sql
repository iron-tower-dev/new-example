USE [LabResultsDb]
GO

/****** Object:  UserDefinedFunction [dbo].[TestStatusText]    Script Date: 10/28/2025 12:04:23 PM ******/
DROP FUNCTION [dbo].[TestStatusText]
GO

/****** Object:  UserDefinedFunction [dbo].[TestStatusText]    Script Date: 10/28/2025 12:04:23 PM ******/
SET ANSI_NULLS OFF
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE FUNCTION [dbo].[TestStatusText] (@StatusChar varchar (1))  
RETURNS varchar (20) AS  
BEGIN 
	RETURN 
		CASE
			WHEN  @StatusChar='S' THEN 'Complete'
			WHEN 	@StatusChar='D' THEN 'Complete'
			WHEN  @StatusChar='A' THEN 'Awaiting Results'
			WHEN  @StatusChar='P' THEN 'Awaiting Results'
			WHEN  @StatusChar='E' THEN 'Awaiting Microscopic Eval'
			WHEN  @StatusChar='T' THEN 'Awaiting Review'
			ELSE ''
		END

END
GO

