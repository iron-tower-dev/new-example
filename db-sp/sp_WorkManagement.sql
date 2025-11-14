USE [LabResultsDb]
GO

/****** Object:  StoredProcedure [dbo].[sp_WorkManagement]    Script Date: 10/28/2025 10:47:56 AM ******/
DROP PROCEDURE [dbo].[sp_WorkManagement]
GO

/****** Object:  StoredProcedure [dbo].[sp_WorkManagement]    Script Date: 10/28/2025 10:47:56 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO






-- =============================================
-- Author:		Richard Young
-- Create date: 2016-05-04
-- Description:	Calculate Lube Lab Work Management stats for given date (or use current date)
-- =============================================
CREATE PROCEDURE [dbo].[sp_WorkManagement] 
	@pDate date = NULL,
	@pFieldAvgAge numeric(10,3) = NULL,
	@pField7plus numeric(18,0) = NULL
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @processDate date, @cutoffDate date
	SET @processDate = ISNULL(@pDate, GETDATE())
	SELECT @cutoffDate = ControlValue FROM Control_Data WHERE Name = 'StartDate';

	DECLARE @OpenLabSamples int, @OpenLabDaysTotal int, @LoggedToday int,
		@Logged0to2 int, @Logged3to7 int, @Logged8to14 int, @Logged15to30 int,
		@Logged30plus int, @OpenEvalSamples int, @OpenEvalDaysTotal int,
		@ClosedEvalSamples int, @ClosedEvalDaysTotal int, @OpenEvalAvgAge numeric(18,0) = 0, 
		@ClosedEvalAvgAge numeric(18,0) = 0, @OpenLabAvgAge numeric(10,3), 
		@ClosedToday int, @ClosedTodayDaysTotal int, @ClosedTodayAvgAge numeric(10,3) = 0,
		@ReturnedToday int, @ReturnedTodayDaysTotal int, @ReturnedTodayAvgAge numeric(10,3) = 0

	--Samples open (in Lab)
	SELECT @OpenLabSamples = COUNT(*), @OpenLabDaysTotal = SUM(NOOFDAYS),
		@LoggedToday = sum(case when NOOFDAYS = 0 then 1 else 0 end),
		@Logged0to2 = sum(case when NOOFDAYS < 3 then 1 else 0 end),
		@Logged3to7 = sum(case when NOOFDAYS >= 3 and NOOFDAYS <= 7 then 1 else 0 end),
		@Logged8to14 = sum(case when NOOFDAYS >= 8 and NOOFDAYS <= 14 then 1 else 0 end),
		@Logged15to30 = sum(case when NOOFDAYS >= 15 and NOOFDAYS <= 30 then 1 else 0 end),
		@Logged30plus = sum(case when NOOFDAYS > 30 then 1 else 0 end)
	FROM (SELECT DATEDIFF(day,receivedon,@processDate) as NOOFDAYS 
			FROM usedlubesamples WHERE RECEIVEDON >= @cutoffDate AND RESULTS_AVAIL_DATE IS NULL) lab;

	IF @OpenLabSamples > 0
	BEGIN
		SET @OpenLabAvgAge = (CAST(@OpenLabDaysTotal AS FLOAT) / CAST(@OpenLabSamples AS FLOAT))
	END

	-- Samples released from lab on given date
	SELECT @ClosedToday = COUNT(*), @ClosedTodayDaysTotal = SUM(NOOFDAYS)
	FROM (SELECT DATEDIFF(day, receivedon, @processDate) as NOOFDAYS 
			FROM usedlubesamples WHERE RECEIVEDON >= @cutoffDate AND DATEDIFF(day, RESULTS_AVAIL_DATE, @processDate) = 0) lab;

	IF @ClosedToday > 0
	BEGIN
		SET @ClosedTodayAvgAge = (@ClosedTodayDaysTotal / @ClosedToday)
	END

	--Samples available for analysis evaulation
	SELECT @OpenEvalSamples = COUNT(ID), @OpenEvalDaysTotal = SUM(DATEDIFF(day,results_avail_date, @processDate)) 
	FROM USEDLUBESAMPLES WHERE STATUS IN (80,120) AND RESULTS_AVAIL_DATE IS NOT NULL AND RESULTS_REVIEW_DATE IS NULL AND NEWUSEDFLAG=0 AND RECEIVEDON >= @cutoffDate;

	IF @OpenEvalSamples > 0
	BEGIN
		SET @OpenEvalAvgAge = (@OpenEvalDaysTotal / @OpenEvalSamples)
	END

	----Samples completed analysis evaulation
	SELECT @ClosedEvalSamples = COUNT(ID), @ClosedEvalDaysTotal = SUM(DATEDIFF(day,RESULTS_AVAIL_DATE,RESULTS_REVIEW_DATE)) 
	FROM USEDLUBESAMPLES WHERE CAST(RESULTS_REVIEW_DATE AS DATE) = @processDate AND NEWUSEDFLAG=0 AND RECEIVEDON >= @cutoffDate;

	IF @ClosedEvalSamples > 0
	BEGIN
		SET @ClosedEvalAvgAge = ROUND((CAST(@ClosedEvalDaysTotal AS FLOAT) / CAST(@ClosedEvalSamples AS FLOAT)), 0)
	END

	-- Samples returned to lab
	SELECT @ReturnedToday = COUNT(*), @ReturnedTodayDaysTotal = SUM(NOOFDAYS)
	FROM (SELECT DATEDIFF(day, receivedon, @processDate) as NOOFDAYS 
			FROM usedlubesamples WHERE RECEIVEDON >= @cutoffDate AND STATUS = 90) lab;

	IF @ReturnedToday > 0
	BEGIN
		SET @ReturnedTodayAvgAge = (@ReturnedTodayDaysTotal / @ReturnedToday)
	END

	--Insert/Update record
	IF EXISTS (SELECT entrydate from workmgmt where entrydate = @processDate)
		BEGIN
			UPDATE workmgmt
			SET days2 = @Logged0to2,
				days3_7 = @Logged3to7,
				days8_14 = @Logged8to14,
				days15_30 = @Logged15to30,
				days_over_30 = @Logged30plus,
				pending_total_samples = @LoggedToday,
				released_since_last_entry = @ClosedToday,
				released_ave = @ClosedTodayAvgAge,
				testing_ave = @OpenLabAvgAge,
				entry_ave = @pFieldAvgAge,
				fielddays_7_over = @pField7plus,
				availforeval = @OpenEvalSamples,
				open_ave = @OpenEvalAvgAge,
				comp_ave = @ClosedEvalAvgAge,
				returnedTotal = @ReturnedToday,
				returnedAve = @ReturnedTodayAvgAge
			WHERE entrydate = @processDate
		END
	ELSE
		BEGIN
			INSERT INTO workmgmt (entrydate, days2, days3_7, days8_14, days15_30, days_over_30, pending_total_samples, released_since_last_entry, released_ave, testing_ave, entry_ave, fielddays_7_over, availforeval, open_ave, comp_ave, returnedTotal, returnedAve) 
			VALUES (@processDate, @Logged0to2, @Logged3to7, @Logged8to14, @Logged15to30, @Logged30plus, @LoggedToday, @ClosedToday, @ClosedTodayAvgAge, @OpenLabAvgAge, @pFieldAvgAge, @pField7plus, @OpenEvalSamples, @OpenEvalAvgAge, @ClosedEvalAvgAge, @ReturnedToday, @ReturnedTodayAvgAge)
		END

END





GO

