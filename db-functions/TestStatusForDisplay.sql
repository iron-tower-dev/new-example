USE [LabResultsDb]
GO

/****** Object:  UserDefinedFunction [dbo].[TestStatusForDisplay]    Script Date: 10/28/2025 12:04:12 PM ******/
DROP FUNCTION [dbo].[TestStatusForDisplay]
GO

/****** Object:  UserDefinedFunction [dbo].[TestStatusForDisplay]    Script Date: 10/28/2025 12:04:12 PM ******/
SET ANSI_NULLS OFF
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE FUNCTION [dbo].[TestStatusForDisplay] (@StatusChar varchar (1),@EntryID nvarchar(5))  
RETURNS varchar (1) AS  
BEGIN 
	RETURN 
		CASE
			WHEN  @StatusChar='S' THEN 'C'
			WHEN  @StatusChar='D' THEN 'C'
			WHEN  @StatusChar='A' THEN 
				CASE
					WHEN @EntryID IS NOT NULL THEN 'R'
					ELSE 'X'
				END
			WHEN @StatusChar='E' THEN 'E'
			WHEN  @StatusChar='T' THEN 'T'
			WHEN @StatusChar='P' THEN 'P'
			ELSE ''
		END

END

GO

