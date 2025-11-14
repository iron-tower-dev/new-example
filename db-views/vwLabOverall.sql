USE [LabResultsDb]
GO

EXEC sys.sp_dropextendedproperty @name=N'MS_DiagramPaneCount' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'vwLabOverall'
GO

EXEC sys.sp_dropextendedproperty @name=N'MS_DiagramPane2' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'vwLabOverall'
GO

EXEC sys.sp_dropextendedproperty @name=N'MS_DiagramPane1' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'vwLabOverall'
GO

/****** Object:  View [dbo].[vwLabOverall]    Script Date: 10/28/2025 11:21:14 AM ******/
DROP VIEW [dbo].[vwLabOverall]
GO

/****** Object:  View [dbo].[vwLabOverall]    Script Date: 10/28/2025 11:21:14 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[vwLabOverall]
AS
SELECT        dbo.UsedLubeSamples.ID AS SampleID, dbo.UsedLubeSamples.tagNumber, dbo.UsedLubeSamples.component, dbo.UsedLubeSamples.location, 
                         dbo.UsedLubeSamples.lubeType, dbo.UsedLubeSamples.sampleDate, dbo.UsedLubeSamples.receivedOn, dbo.UsedLubeSamples.status AS SampleStatus, 
                         dbo.vwLabSampleTests.TestID, dbo.vwLabSampleTests.TestName, dbo.vwLabSampleTests.TestAbbrev, dbo.vwLabSampleTests.ShortAbbrev, 
                         dbo.TestReadings.status AS TestStatus, dbo.TestReadings.trialNumber, dbo.TestReadings.entryID, dbo.TestReadings.validateID, 
                         dbo.TestStatusForDisplay(dbo.TestReadings.status, dbo.TestReadings.entryID) AS TestDisplayStatus, dbo.Component.name AS ComponentName, 
                         dbo.Location.name AS LocationName, DATEDIFF(d, UsedLubeSamples.receivedOn, GETDATE()) AS Age
FROM            dbo.UsedLubeSamples INNER JOIN
                         dbo.vwLabSampleTests ON dbo.UsedLubeSamples.ID = dbo.vwLabSampleTests.sampleID LEFT OUTER JOIN
                         dbo.Component ON dbo.UsedLubeSamples.component = dbo.Component.code LEFT OUTER JOIN
                         dbo.Location ON dbo.UsedLubeSamples.location = dbo.Location.code LEFT OUTER JOIN
                         dbo.TestReadings ON dbo.vwLabSampleTests.sampleID = dbo.TestReadings.sampleID AND dbo.vwLabSampleTests.TestID = dbo.TestReadings.testID
GO

EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
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
         Begin Table = "UsedLubeSamples (dbo)"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 135
               Right = 228
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "vwLabSampleTests (dbo)"
            Begin Extent = 
               Top = 6
               Left = 266
               Bottom = 135
               Right = 436
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "Component (dbo)"
            Begin Extent = 
               Top = 138
               Left = 38
               Bottom = 250
               Right = 208
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "Location (dbo)"
            Begin Extent = 
               Top = 138
               Left = 246
               Bottom = 250
               Right = 416
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "TestReadings (dbo)"
            Begin Extent = 
               Top = 252
               Left = 38
               Bottom = 381
               Right = 213
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
      Begin ColumnWidths = 9
         Width = 284
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
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'vwLabOverall'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane2', @value=N'         Column = 2820
         Alias = 3615
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
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'vwLabOverall'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=2 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'vwLabOverall'
GO

