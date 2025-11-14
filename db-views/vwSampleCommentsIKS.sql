USE [LabResultsDb]
GO

/****** Object:  View [dbo].[vwSampleCommentsIKS]    Script Date: 10/28/2025 11:33:31 AM ******/
DROP VIEW [dbo].[vwSampleCommentsIKS]
GO

/****** Object:  View [dbo].[vwSampleCommentsIKS]    Script Date: 10/28/2025 11:33:31 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[vwSampleCommentsIKS]
AS
SELECT     dbo.AllResults.SampleID, dbo.allsamplecomments.CommentArea, dbo.allsamplecomments.Comment, dbo.allsamplecomments.CommentDate, 
                      dbo.allsamplecomments.TestID, dbo.allsamplecomments.CommentID, dbo.allsamplecomments.UserID
FROM         dbo.AllResults INNER JOIN
                      dbo.allsamplecomments ON dbo.AllResults.SampleID = dbo.allsamplecomments.SampleID
WHERE     (dbo.allsamplecomments.SiteID = 1)

GO

