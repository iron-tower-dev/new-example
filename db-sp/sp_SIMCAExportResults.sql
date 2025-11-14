USE [LabResultsDb]
GO

/****** Object:  StoredProcedure [dbo].[sp_SIMCAExportResults]    Script Date: 10/28/2025 10:45:43 AM ******/
DROP PROCEDURE [dbo].[sp_SIMCAExportResults]
GO

/****** Object:  StoredProcedure [dbo].[sp_SIMCAExportResults]    Script Date: 10/28/2025 10:45:43 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[sp_SIMCAExportResults]
AS
BEGIN

BEGIN TRAN TAN_CI
UPDATE AllResults SET AN=value2 from TestReadings t, AllResults a WHERE t.SampleID = a.SampleID AND t.TestID=10 AND t.trialNumber=1
COMMIT TRAN TAN_CI

BEGIN TRAN KF
UPDATE AllResults SET KF=value1 from TestReadings t, AllResults a WHERE t.SampleID = a.SampleID AND t.TestID=20 AND t.trialNumber=1
COMMIT TRAN KF

BEGIN TRAN V40
UPDATE AllResults SET Vis_40=value3 from TestReadings t, AllResults a WHERE t.SampleID = a.SampleID AND t.TestID=50 AND t.trialNumber=1
COMMIT TRAN V40

BEGIN TRAN V100
UPDATE AllResults SET Vis_100=value3 from TestReadings t, AllResults a WHERE t.SampleID = a.SampleID AND t.TestID=60 AND t.trialNumber=1
COMMIT TRAN V100

BEGIN TRAN FLASH
UPDATE AllResults SET Flash_Pt=value3 from TestReadings t, AllResults a WHERE t.SampleID = a.SampleID AND t.TestID=80 AND t.trialNumber=1
COMMIT TRAN FLASH

BEGIN TRAN TBN
UPDATE AllResults SET TBN=value1 from TestReadings t, AllResults a WHERE t.SampleID = a.SampleID AND t.TestID=110 AND t.trialNumber=1
COMMIT TRAN TBN

BEGIN TRAN CONE
UPDATE AllResults SET Cone_Pen=trialCalc, NLGI=ID1 from TestReadings t, AllResults a WHERE t.SampleID = a.SampleID AND t.TestID=130 AND t.trialNumber=1
COMMIT TRAN CONE

BEGIN TRAN DROPPT
UPDATE AllResults SET drop_pt=value2 from TestReadings t, AllResults a WHERE t.SampleID = a.SampleID AND t.TestID=140 AND t.trialNumber=1
COMMIT TRAN DROPPT

BEGIN TRAN RBOT
UPDATE AllResults SET RBOT=value1 from TestReadings t, AllResults a WHERE t.SampleID = a.SampleID AND t.TestID=170 AND t.trialNumber=1
COMMIT TRAN RBOT

BEGIN TRAN FRES
UPDATE AllResults SET Filter_Res=value2 from TestReadings t, AllResults a WHERE t.SampleID = a.SampleID AND t.TestID=180 AND t.trialNumber=1
COMMIT TRAN FRES

BEGIN TRAN RUST
UPDATE AllResults SET Rust=value1 from TestReadings t, AllResults a WHERE t.SampleID = a.SampleID AND t.TestID=220 AND t.trialNumber=1
COMMIT TRAN RUST

BEGIN TRAN TFOUT
UPDATE AllResults SET TFOUT=value1 from TestReadings t, AllResults a WHERE t.SampleID = a.SampleID AND t.TestID=230 AND t.trialNumber=1
COMMIT TRAN TFOUT

BEGIN TRAN RESIS
UPDATE AllResults SET Resis=value1 from TestReadings t, AllResults a WHERE t.SampleID = a.SampleID AND t.TestID=280 AND t.trialNumber=1
COMMIT TRAN RESIS

BEGIN TRAN CHLORIDE
UPDATE AllResults SET Chlorides=value1 from TestReadings t, AllResults a WHERE t.SampleID = a.SampleID AND t.TestID=281 AND t.trialNumber=1
COMMIT TRAN CHLORIDE

BEGIN TRAN AMINE
UPDATE AllResults SET Amine=value1 from TestReadings t, AllResults a WHERE t.SampleID = a.SampleID AND t.TestID=282 AND t.trialNumber=1
COMMIT TRAN AMINE

BEGIN TRAN PHENOL
UPDATE AllResults SET Phenol=value1 from TestReadings t, AllResults a WHERE t.SampleID = a.SampleID AND t.TestID=283 AND t.trialNumber=1
COMMIT TRAN PHENOL

BEGIN TRAN BLEED
UPDATE AllResults SET Bleed=value1 from TestReadings t, AllResults a WHERE t.SampleID = a.SampleID AND t.TestID=284 AND t.trialNumber=1
COMMIT TRAN BLEED

BEGIN TRAN OIL
UPDATE AllResults SET Oil_Content=value1 from TestReadings t, AllResults a WHERE t.SampleID = a.SampleID AND t.TestID=285 AND t.trialNumber=1
COMMIT TRAN OIL

BEGIN TRAN FRICTION
UPDATE AllResults SET Friction=value1 from TestReadings t, AllResults a WHERE t.SampleID = a.SampleID AND t.TestID=286 AND t.trialNumber=1
COMMIT TRAN FRICTION

BEGIN TRAN GOODNESS
UPDATE AllResults SET Goodness=value1 from TestReadings t, AllResults a WHERE t.SampleID = a.SampleID AND t.TestID=300 AND t.trialNumber=1
COMMIT TRAN GOODNESS

BEGIN TRAN TEMPERATURE
UPDATE AllResults SET Temperature=ulim3
  FROM limits l, AllResults a, limits_xref x, component c, location lo 
  WHERE (l.exclude IS NULL)
  AND (l.testid = 400)
  AND (x.exclude IS NULL)
  AND l.limits_xref_id = x.valueat
  AND c.code=x.component
  AND lo.code=x.location
  AND a.EQID=x.tagNumber
  AND a.Comp=c.name
  AND a.Loc=lo.name
COMMIT TRAN TEMPERATURE

END
GO

