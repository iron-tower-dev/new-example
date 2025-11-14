USE [LabResultsDb]
GO

/****** Object:  UserDefinedFunction [dbo].[TestScheduleLimit]    Script Date: 10/28/2025 12:03:59 PM ******/
DROP FUNCTION [dbo].[TestScheduleLimit]
GO

/****** Object:  UserDefinedFunction [dbo].[TestScheduleLimit]    Script Date: 10/28/2025 12:03:59 PM ******/
SET ANSI_NULLS OFF
GO

SET QUOTED_IDENTIFIER OFF
GO


CREATE FUNCTION [dbo].[TestScheduleLimit] (@GroupID int, @TestID int, @LimitType char (1))  
RETURNS varchar (20) AS  
BEGIN 
	RETURN 
		CASE
			WHEN  @LimitType='U' THEN 
				CASE
					WHEN (SELECT distinct top 1 tu3 FROM limits WHERE limits_xref_id=@GroupID AND testid=@TestID ORDER BY 1 DESC)='Y' THEN 
						(SELECT distinct top 1 ulim3 FROM limits WHERE limits_xref_id=@GroupID AND testid=@TestID ORDER BY 1 DESC)
					WHEN (SELECT distinct top 1 tu2 FROM limits WHERE limits_xref_id=@GroupID AND testid=@TestID ORDER BY 1 DESC)='Y' THEN 
						(SELECT distinct top 1 ulim2 FROM limits WHERE limits_xref_id=@GroupID AND testid=@TestID ORDER BY 1 DESC)
					WHEN (SELECT distinct top 1 tu1 FROM limits WHERE limits_xref_id=@GroupID AND testid=@TestID ORDER BY 1 DESC)='Y' THEN 
						(SELECT distinct top 1 ulim1 FROM limits WHERE limits_xref_id=@GroupID AND testid=@TestID ORDER BY 1 DESC)
					ELSE NULL
				END
			WHEN 	@LimitType='L' THEN
				CASE
					WHEN (SELECT distinct top 1 tl3 FROM limits WHERE limits_xref_id=@GroupID AND testid=@TestID ORDER BY 1 DESC)='Y' THEN 
						(SELECT distinct top 1 llim3 FROM limits WHERE limits_xref_id=@GroupID AND testid=@TestID ORDER BY 1 DESC)
					WHEN (SELECT distinct top 1 tl2 FROM limits WHERE limits_xref_id=@GroupID AND testid=@TestID ORDER BY 1 DESC)='Y' THEN 
						(SELECT distinct top 1 llim2 FROM limits WHERE limits_xref_id=@GroupID AND testid=@TestID ORDER BY 1 DESC)
					WHEN (SELECT distinct top 1 tl1 FROM limits WHERE limits_xref_id=@GroupID AND testid=@TestID ORDER BY 1 DESC)='Y' THEN 
						(SELECT distinct top 1 llim1 FROM limits WHERE limits_xref_id=@GroupID AND testid=@TestID ORDER BY 1 DESC)
					ELSE NULL
				END
			ELSE NULL
		END

END


GO

