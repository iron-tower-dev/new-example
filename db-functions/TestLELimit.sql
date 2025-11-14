USE [LabResultsDb]
GO

/****** Object:  UserDefinedFunction [dbo].[TestLELimit]    Script Date: 10/28/2025 12:03:24 PM ******/
DROP FUNCTION [dbo].[TestLELimit]
GO

/****** Object:  UserDefinedFunction [dbo].[TestLELimit]    Script Date: 10/28/2025 12:03:24 PM ******/
SET ANSI_NULLS OFF
GO

SET QUOTED_IDENTIFIER ON
GO





CREATE    FUNCTION [dbo].[TestLELimit] (@GroupID int, @TestID int, @TestName varchar(25), @LimitType char (1))  
RETURNS varchar (20) AS  
BEGIN 
	RETURN 
		CASE
			WHEN  @LimitType='U' THEN 
				CASE
					WHEN (SELECT distinct top 1 lu1 FROM limits WHERE limits_xref_id=@GroupID AND testid=@TestID and TestName=@TestName ORDER BY 1 DESC)='Y' THEN 
						(SELECT distinct top 1 ulim1 FROM limits WHERE limits_xref_id=@GroupID AND testid=@TestID and TestName=@TestName  ORDER BY 1 DESC)
					WHEN (SELECT distinct top 1 lu2 FROM limits WHERE limits_xref_id=@GroupID AND testid=@TestID and TestName=@TestName  ORDER BY 1 DESC)='Y' THEN 
						(SELECT distinct top 1 ulim2 FROM limits WHERE limits_xref_id=@GroupID AND testid=@TestID and TestName=@TestName  ORDER BY 1 DESC)
					WHEN (SELECT distinct top 1 lu3 FROM limits WHERE limits_xref_id=@GroupID AND testid=@TestID and TestName=@TestName  ORDER BY 1 DESC)='Y' THEN 
						(SELECT distinct top 1 ulim3 FROM limits WHERE limits_xref_id=@GroupID AND testid=@TestID and TestName=@TestName  ORDER BY 1 DESC)
					ELSE NULL
				END
			WHEN 	@LimitType='L' THEN
				CASE
					WHEN (SELECT distinct top 1 ll1 FROM limits WHERE limits_xref_id=@GroupID AND testid=@TestID and TestName=@TestName  ORDER BY 1 DESC)='Y' THEN 
						(SELECT distinct top 1 llim1 FROM limits WHERE limits_xref_id=@GroupID AND testid=@TestID and TestName=@TestName  ORDER BY 1 DESC)
					WHEN (SELECT distinct top 1 ll2 FROM limits WHERE limits_xref_id=@GroupID AND testid=@TestID and TestName=@TestName  ORDER BY 1 DESC)='Y' THEN 
						(SELECT distinct top 1 llim2 FROM limits WHERE limits_xref_id=@GroupID AND testid=@TestID and TestName=@TestName  ORDER BY 1 DESC)
					WHEN (SELECT distinct top 1 ll3 FROM limits WHERE limits_xref_id=@GroupID AND testid=@TestID and TestName=@TestName  ORDER BY 1 DESC)='Y' THEN 
						(SELECT distinct top 1 llim3 FROM limits WHERE limits_xref_id=@GroupID AND testid=@TestID and TestName=@TestName  ORDER BY 1 DESC)
					ELSE NULL
				END
			ELSE NULL
		END

END




GO

