USE [LabResultsDb]
GO

EXEC sys.sp_dropextendedproperty @name=N'MS_DiagramPaneCount' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'vwSampleReleaseIF'
GO

EXEC sys.sp_dropextendedproperty @name=N'MS_DiagramPane2' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'vwSampleReleaseIF'
GO

EXEC sys.sp_dropextendedproperty @name=N'MS_DiagramPane1' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'vwSampleReleaseIF'
GO

/****** Object:  View [dbo].[vwSampleReleaseIF]    Script Date: 10/28/2025 11:34:08 AM ******/
DROP VIEW [dbo].[vwSampleReleaseIF]
GO

/****** Object:  View [dbo].[vwSampleReleaseIF]    Script Date: 10/28/2025 11:34:08 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[vwSampleReleaseIF]
AS
SELECT        dbo.TestReadings.sampleID, dbo.TestReadings.testID, dbo.TestStatusText(dbo.TestReadings.status) AS TestStatusText, dbo.vwTestsForLab.TestName, 
                         dbo.vwTestsForLab.TestAbbrev, dbo.vwTestsForLab.ShortAbbrev, dbo.TestReadings.entryID, dbo.TestReadings.validateID, dbo.InspectFilter.narrative, 
                         Comments_1.remark AS Major, Comments_2.remark AS Minor, Comments_3.remark AS Trace, dbo.TestReadings.value2 AS Result, dbo.TestReadings.value1, 
                         dbo.TestReadings.value3, dbo.TestReadings.ID1, dbo.TestReadings.ID2, dbo.TestReadings.ID3, dbo.InspectFilter.ID AS IFId, CASE WHEN (InspectFilter.ID IS NULL 
                         AND TestReadings.sampleID > 47900) THEN 'Y' ELSE 'N' END AS IsNew
FROM            dbo.TestReadings INNER JOIN
                         dbo.vwTestsForLab ON dbo.TestReadings.testID = dbo.vwTestsForLab.TestID LEFT OUTER JOIN
                         dbo.InspectFilter ON dbo.TestReadings.sampleID = dbo.InspectFilter.ID AND dbo.TestReadings.testID = dbo.InspectFilter.testID LEFT OUTER JOIN
                         dbo.Comments AS Comments_3 ON dbo.InspectFilter.trace = Comments_3.ID LEFT OUTER JOIN
                         dbo.Comments AS Comments_2 ON dbo.InspectFilter.minor = Comments_2.ID LEFT OUTER JOIN
                         dbo.Comments AS Comments_1 ON dbo.InspectFilter.major = Comments_1.ID
WHERE        (dbo.TestReadings.testID = 120) OR
                         (dbo.TestReadings.testID = 180) OR
                         (dbo.TestReadings.testID = 240)
GO

EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[41] 4[20] 2[10] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "TestReadings (dbo)"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 135
               Right = 213
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "vwTestsForLab (dbo)"
            Begin Extent = 
               Top = 6
               Left = 251
               Bottom = 135
               Right = 421
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "InspectFilter (dbo)"
            Begin Extent = 
               Top = 138
               Left = 38
               Bottom = 267
               Right = 208
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "Comments_3"
            Begin Extent = 
               Top = 138
               Left = 246
               Bottom = 267
               Right = 416
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "Comments_2"
            Begin Extent = 
               Top = 270
               Left = 38
               Bottom = 399
               Right = 208
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "Comments_1"
            Begin Extent = 
               Top = 270
               Left = 246
               Bottom = 399
               Right = 416
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
      Begin ColumnWidths = 21
         Width = 284
       ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'vwSampleReleaseIF'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane2', @value=N'  Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 4065
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'vwSampleReleaseIF'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=2 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'vwSampleReleaseIF'
GO

